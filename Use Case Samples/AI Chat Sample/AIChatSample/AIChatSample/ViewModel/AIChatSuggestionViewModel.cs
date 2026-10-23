


using Syncfusion.Maui.AIAssistView;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AIChatSample;

/// <summary>
/// View-model for the new-chat screen. Owns the prompt + response flow
/// and delegates the actual AI call to <see cref="IAIService"/>. Uses
/// async/await so the UI thread stays responsive while a request is in
/// flight.
/// </summary>
public partial class AIChatSuggestionViewModel : ObservableObject
{
    private readonly IAIService _aiService;
    private readonly IChatDataService _data;

    private ObservableCollection<ISuggestion> _suggestions;
    private ObservableCollection<IAssistItem> _assistItems;
    private string _inputText = string.Empty;
    private bool _isBusy;
    private string? _systemContext;
    private ChatSession? _session;
    private CancellationTokenSource? _cts;
    public AIChatSuggestionViewModel()
        : this(
            aiService: ServiceHelper.GetService<IAIService>() ?? new MockAIService(),
            data: ServiceHelper.GetService<IChatDataService>() ?? new ChatDataService())
    {
    }

    public AIChatSuggestionViewModel(IAIService aiService, IChatDataService data)
    {
        _aiService = aiService ?? throw new ArgumentNullException(nameof(aiService));
        _data = data ?? throw new ArgumentNullException(nameof(data));

        Color iconColor = Color.FromArgb("#7633DA");
        if (Application.Current != null)
        {
            iconColor = Application.Current.UserAppTheme == AppTheme.Dark
                ? Color.FromArgb("#A572F7")
                : TryGetThemeColor("PrimaryColorLight", fallback: Color.FromArgb("#7633DA"));
        }

        // initial prompts surfaced as the welcome-state
        // suggestion grid. The list intentionally does not change based
        // on the active agent — these are global starters.
        this._suggestions = new ObservableCollection<ISuggestion>()
        {
            new AssistSuggestion() { Text = "Summarize a document", ImageSource = new FontImageSource() { Glyph = "\ue797", FontFamily = "MaterialAssets", Color = iconColor } },
            new AssistSuggestion() { Text = "Analyze business data", ImageSource = new FontImageSource() { Glyph = "\ue793", FontFamily = "MaterialAssets", Color = iconColor } },
            new AssistSuggestion() { Text = "Draft content", ImageSource = new FontImageSource() { Glyph = "\ue7f2", FontFamily = "MaterialAssets", Color = iconColor } },
            new AssistSuggestion() { Text = "Solve a technical problem", ImageSource = new FontImageSource() { Glyph = "\ue716", FontFamily = "MaterialAssets", Color = iconColor } },
        };

        this._assistItems = new ObservableCollection<IAssistItem>();
        this._assistItems.CollectionChanged += OnAssistItemsChanged;

        // The follow-up suggestions returned by the AI service
        // (e.g. "Tell me more", "Give an example") are surfaced via the
        // syncfusion AIAssistView's suggestion strip; binding
        // Suggestions above keeps that strip alive.

        // Agents shown in the AIAssistView agent selector: two defaults
        // plus any agent created by the user (kept in sync via IAgentStore).
        // Icons intentionally use ImageSource (not FontImageSource) on
        // Windows. The default template applies a DropShadow whose mask is
        // a CompositionBrush derived from the icon; FontImageSource reuses
        // a visual that gets disposed when the selector recycles, leading
        // to "object has been closed" on the next selection.
        this._agents = new ObservableCollection<AssistAgent>
        {
            new AssistAgent
            {
                Name = "Code Reviewer",
                Description = "Reviews code for bugs, readability, performance and best practices.",
                Instructions = "You are an expert code reviewer. Analyze the provided code for bugs, security issues, performance problems, readability and best practices. Give concise, actionable feedback with improved code snippets where helpful.",
                Icon = new FontImageSource { Glyph = "\ue71C", FontFamily = "MaterialAssets", Color = iconColor },
            },
            new AssistAgent
            {
                Name = "Social Media Writer",
                Description = "Writes engaging, platform-ready social media posts.",
                Instructions = "You are a creative social media writer. Produce engaging, concise posts tailored to the requested platform and tone, with relevant hashtags and a clear call to action.",
                Icon = new FontImageSource { Glyph = "\ue7B4", FontFamily = "MaterialAssets", Color = iconColor },
            },
        };

        this._agentStore = ServiceHelper.GetService<IAgentStore>();
        if (this._agentStore is not null)
        {
            foreach (var existing in this._agentStore.Agents)
            {
                this.AddAssistAgent(existing);
            }
            this._agentStore.AgentAdded += this.OnAgentStoreAgentAdded;
        }

        this.RequestCommand = new Command<object>(this.OnRequest);
        this.SendCommand = new Command(this.OnSendRequested, this.CanSend);
        this.SuggestionTappedCommand = new Command<ISuggestion>(this.OnSuggestionTapped);
        this.CancelRequestCommand = new Command(this.CancelActiveRequest, () => this._isBusy);
        // Real implementations for the editor's + and mic icons. The +
        // opens the system file picker and the mic drives the in-page
        // speech-to-text flow. Both update the shared InputText so the
        // user can review / edit the result before sending.
        this.AttachCommand = new Command(async () => await this.OnAttachFileAsync());
        this.MicCommand = new Command(async () => await this.OnStartVoiceInputAsync());
    }

    private readonly IAgentStore? _agentStore;
    private readonly ObservableCollection<AssistAgent> _agents;
    private AssistAgent? _selectedAgent;

    /// <summary>Agents bound to <c>SfAIAssistView.Agents</c>.</summary>
    public ObservableCollection<AssistAgent> Agents => this._agents;

    /// <summary>Agent bound to <c>SfAIAssistView.SelectedAgent</c>; its instructions drive the system context.</summary>
    public AssistAgent? SelectedAgent
    {
        get => this._selectedAgent;
        set
        {
            if (ReferenceEquals(this._selectedAgent, value)) return;
            this._selectedAgent = value;
            this.SetSystemContext(value?.Instructions);
            this.OnPropertyChanged();
        }
    }

    private void AddAssistAgent(AgentConfiguration config)
    {
        if (string.IsNullOrWhiteSpace(config.AgentName)
            || this._agents.Any(a => string.Equals(a.Name, config.AgentName, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        // Don't set an Icon on Windows. The default selector template
        // applies a DropShadow whose mask is a CompositionBrush sourced
        // from the icon. A FontImageSource-backed brush gets disposed when
        // the selector recycles its visuals, which surfaces as
        // "System.ObjectDisposedException: The object has been closed" the
        // next time the user selects an agent. Pass a file (AvatarSource)
        // if you want a real icon there.
        ImageSource? icon = null;
        if (DeviceInfo.Platform == DevicePlatform.WinUI)
        {
            if (!string.IsNullOrWhiteSpace(config.AvatarSource))
            {
                icon = ImageSource.FromFile(config.AvatarSource);
            }
        }
        else if (!string.IsNullOrWhiteSpace(config.AvatarSource))
        {
            icon = ImageSource.FromFile(config.AvatarSource);
        }
        else
        {
            Color bg = Color.FromArgb("#7633DA");
            try { if (!string.IsNullOrEmpty(config.Color)) bg = Color.FromArgb(config.Color); } catch { }
            icon = new FontImageSource
            {
                Glyph = config.Initial ?? config.AgentName.Substring(0, 1).ToUpperInvariant(),
                Color = bg,
            };
        }

        this._agents.Add(new AssistAgent
        {
            Name = config.AgentName,
            Description = config.AgentDescription!,
            Instructions = config.Instructions!,
            Icon = icon!,
        });
    }

    private void OnAgentStoreAgentAdded(object? sender, AgentConfiguration config)
    {
        if (MainThread.IsMainThread) this.AddAssistAgent(config);
        else MainThread.BeginInvokeOnMainThread(() => this.AddAssistAgent(config));
    }

    public ObservableCollection<ISuggestion> Suggestions
    {
        get { return this._suggestions; }
        set
        {
            this._suggestions = value;
            this.OnPropertyChanged(nameof(Suggestions));
        }
    }

    public ObservableCollection<IAssistItem> AssistItems
    {
        get { return this._assistItems; }
        set
        {
            if (this._assistItems != null)
            {
                this._assistItems.CollectionChanged -= OnAssistItemsChanged;
            }

            this._assistItems = value ?? new ObservableCollection<IAssistItem>();
            this._assistItems.CollectionChanged += OnAssistItemsChanged;
            this.OnPropertyChanged(nameof(AssistItems));
            this.OnPropertyChanged(nameof(HasMessages));
            this.OnPropertyChanged(nameof(ShowWelcomeSuggestions));
        }
    }

    /// <summary>True when the user has already started a conversation. Used to hide the suggestion grid.</summary>
    public bool HasMessages => this._assistItems != null && this._assistItems.Count > 0;

    /// <summary>
    /// True when the current session was opened from an agent (either a
    /// default or a user-created one). Drives the header copy and hides
    /// the "Suggested for you" grid so an agent chat opens straight to
    /// the input box.
    /// </summary>
    public bool IsAgentSession => this.CurrentSession?.Agent is not null;

    /// <summary>
    /// Welcome-time "Suggested for you" strip. Visible only on the first
    /// load of a New chat (no messages yet, no agent session) so the
    /// suggestions don't reappear once the user has actually started
    /// a conversation.
    /// </summary>
    public bool ShowWelcomeSuggestions => !HasMessages && !IsAgentSession;

    /// <summary>Header title shown in the welcome state. Falls back to a generic prompt when no agent is active.</summary>
    public string HeaderTitle => this.CurrentSession?.Agent?.AgentName ?? "How can I help you today?";

    /// <summary>Header subtitle shown in the welcome state. Falls back to a generic prompt when no agent is active.</summary>
    public string HeaderSubtitle =>
        !string.IsNullOrWhiteSpace(this.CurrentSession?.Agent?.AgentDescription)
            ? this.CurrentSession!.Agent!.AgentDescription
            : "I am ready to assist with writing, analysis, or answering your questions.";

    private void OnSessionChanged()
    {
        this.OnPropertyChanged(nameof(IsAgentSession));
        this.OnPropertyChanged(nameof(HeaderTitle));
        this.OnPropertyChanged(nameof(HeaderSubtitle));
        this.OnPropertyChanged(nameof(ShowWelcomeSuggestions));
    }

    public string InputText
    {
        get => this._inputText;
        set
        {
            if (this._inputText == value) return;
            this._inputText = value;
            this.OnPropertyChanged(nameof(InputText));
            (this.SendCommand as Command)?.ChangeCanExecute();
        }
    }

    public bool IsBusy
    {
        get => this._isBusy;
        set
        {
            if (this._isBusy == value) return;
            this._isBusy = value;
            this.OnPropertyChanged(nameof(IsBusy));
            (this.CancelRequestCommand as Command)?.ChangeCanExecute();
            (this.SendCommand as Command)?.ChangeCanExecute();
        }
    }

    public ICommand RequestCommand { get; }

    public ICommand SendCommand { get; }

    public ICommand SuggestionTappedCommand { get; }

    public ICommand CancelRequestCommand { get; }

    /// <summary>Bound to the editor's "+" / attachment icon. Hook for future file-attachment flows.</summary>
    public ICommand AttachCommand { get; }

    /// <summary>Bound to the editor's microphone icon. Hook for future voice-input flows.</summary>
    public ICommand MicCommand { get; }

    /// <summary>Currently bound <see cref="ChatSession"/>, or <c>null</c> when showing the welcome state.</summary>
    public ChatSession? CurrentSession
    {
        get => _session;
        private set
        {
            if (ReferenceEquals(_session, value)) return;
            _session = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasSession));
            OnPropertyChanged(nameof(CurrentAgent));
            this.OnSessionChanged();
        }
    }

    public AgentConfiguration? CurrentAgent => _session?.Agent;

    /// <summary>True when a chat session is bound. Drives the welcome-header visibility on the host page.</summary>
    public bool HasSession => _session is not null;

    /// <summary>
    /// Loads an existing chat session into the view-model, replacing the
    /// current conversation with the session's stored messages.
    /// </summary>
    public void LoadSession(ChatSession session)
    {
        if (session is null) return;

        CurrentSession = session;
        SetSystemContext(session.Agent?.Instructions);

        // Make sure the session is fresh in the Recent list.
        _data.TouchSession(session);

        _assistItems.Clear();
        foreach (var message in session.Messages)
        {
            _assistItems.Add(message);
        }
    }

    private bool CanSend() => !string.IsNullOrWhiteSpace(this._inputText) && !this._isBusy;

    private void OnSendRequested()
    {
        if (!this.CanSend())
        {
            return;
        }

        // Every new prompt lazily creates a ChatSession so the
        // Recent-chats list reflects what the user typed. This keeps the
        // behaviour consistent whether the user typed into the welcome
        // state or into an existing session.
        if (_session is null)
        {
            var session = _data.CreateChatSession();
            CurrentSession = session;
            SetSystemContext(session.Agent?.Instructions);
        }

        var request = new AssistItem
        {
            Text = this._inputText,
            IsRequested = true,
        };
        this._assistItems.Add(request);
        if (_session is not null && !_session.Messages.Contains(request))
        {
            _session.Messages.Add(request);
            PromoteFirstPromptToTitle(_session, request.Text);
        }
        var prompt = request.Text;
        // If a file is attached, surface its name in the prompt so the
        // assistant has context. Hookups for actual file content delivery
        // live behind the AI service layer and are out of scope here.
        if (!string.IsNullOrEmpty(this.SelectedFile))
        {
            prompt = $"[Attached: {this.SelectedFile}]\n{prompt}";
        }
        this.InputText = string.Empty;
        this.SelectedFile = string.Empty;
        TouchSession();

        // The custom input editor bypasses the AssistView's own request
        // flow, so the AI call must be triggered here.
        _ = SendPromptAsync(prompt);
    }

    /// <summary>
    /// The currently-attached file's display name (or null when no file is
    /// attached). Set by <see cref="OnAttachFileAsync"/>; cleared when the
    /// user sends the prompt or explicitly removes the attachment.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasAttachedFile))]
    private string? selectedFile;

    public bool HasAttachedFile => !string.IsNullOrEmpty(this.SelectedFile);

    /// <summary>Removes the attached file.</summary>
    [RelayCommand]
    private void RemoveAttachment() => this.SelectedFile = string.Empty;

    private async Task OnAttachFileAsync()
    {
        try
        {
            var options = new Microsoft.Maui.Storage.PickOptions
            {
                PickerTitle = "Select a file to attach",
                FileTypes = Microsoft.Maui.Storage.FilePickerFileType.Images
            };
            var result = await Microsoft.Maui.Storage.FilePicker.Default.PickAsync(options);
            if (result is null) return;
            this.SelectedFile = result.FileName;
        }
        catch (Exception ex)
        {
            // File picker cancellation throws TaskCanceledException on some
            // platforms — swallow silently so the user isn't startled. Other
            // failures surface via the page's error banner path.
            System.Diagnostics.Debug.WriteLine($"File pick failed: {ex.Message}");
        }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MicGlyph))]
    private bool isListening;

    /// <summary>MaterialAssets glyph for the mic — toggles between mic and stop icons while listening.</summary>
    public string MicGlyph => this.IsListening ? "\ue71f" : "\ue7ec";

    private async Task OnStartVoiceInputAsync()
    {
        // Toggle off if already listening. The mic icon also acts as a
        // stop control while a recording session is in progress.
        if (this.IsListening)
        {
            this.IsListening = false;
            return;
        }

        try
        {
            this.IsListening = true;
            var recognized = await VoiceInputRecognizer.RecognizeAsync(this.InputText);
            this.IsListening = false;
            if (!string.IsNullOrWhiteSpace(recognized))
            {
                this.InputText = string.IsNullOrEmpty(this._inputText)
                    ? recognized
                    : (this._inputText + " " + recognized).Trim();
            }
        }
        catch (Exception ex)
        {
            this.IsListening = false;
            System.Diagnostics.Debug.WriteLine($"Voice input failed: {ex.Message}");
        }
    }

    private async void OnRequest(object? parameter)
    {
        if (parameter is not RequestEventArgs args || args.RequestItem is not IAssistItem userItem)
        {
            return;
        }

        // The SfAIAssistView already added the request item to AssistItems
        // before raising the event. We just need to call the AI service
        // and append the response.
        args.Handled = true;

        var prompt = userItem.Text;
        if (string.IsNullOrWhiteSpace(prompt))
        {
            return;
        }

        if (_session is not null)
        {
            PromoteFirstPromptToTitle(_session, prompt);
        }

        await SendPromptAsync(prompt).ConfigureAwait(true);
    }

    /// <summary>
    /// Promote the first user prompt to the session title so Recent chats /
    /// Search show what the user actually asked instead of a placeholder.
    /// Replaces the default "New chat" or agent-name title only.
    /// </summary>
    private static void PromoteFirstPromptToTitle(ChatSession session, string prompt)
    {
        if (session is null || string.IsNullOrWhiteSpace(prompt)) return;
        var current = session.Title;
        var isDefault =
            string.IsNullOrWhiteSpace(current) ||
            string.Equals(current, "New chat", StringComparison.OrdinalIgnoreCase) ||
            (session.Agent is not null &&
             string.Equals(current, session.Agent.AgentName, StringComparison.OrdinalIgnoreCase));
        if (!isDefault) return;
        session.Title = prompt.Length > 80 ? prompt.Substring(0, 80) + "…" : prompt;
    }

    /// <summary>
    /// Sends a prompt to the AI service on a background thread and
    /// appends the response to the chat surface. Marshals UI updates
    /// back to the main thread.
    /// </summary>
    public async Task SendPromptAsync(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt) || this._isBusy)
        {
            return;
        }

        CancelActiveRequest();
        this.IsBusy = true;
        this.ShowResponseLoader = true;

        _cts = new CancellationTokenSource();
        var ct = _cts.Token;

        try
        {
            // The AI service methods are already async — wrapping them in
            // Task.Run just hops threads without adding concurrency. Call
            // directly and let the inner implementation (and any ConfigureAwait
            // inside it) decide where to resume.
            var result = await _aiService.GetResponseAsync(prompt, _systemContext, ct).ConfigureAwait(true);

            if (ct.IsCancellationRequested || result is null)
            {
                return;
            }
            AddResponse(result.Answer ?? string.Empty);
            // Surface the AI's follow-up suggestions so the suggestion
            // strip below the editor stays live. The mock
            // always returns suggestions; Azure may not, in which case
            // AppendFollowUpSuggestions is a no-op.
            AppendFollowUpSuggestions(result.Suggestions);
        }
        catch (OperationCanceledException)
        {
            // User-cancelled.
        }
        catch (Exception ex)
        {
            AddResponse($"Error: {ex.Message}");
        }
        finally
        {
            this.ShowResponseLoader = false;
            this.IsBusy = false;
            _cts?.Dispose();
            _cts = null;
            TouchSession();
        }
    }

    private void AddResponse(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        var item = new AssistItem { Text = text, IsRequested = false };
        this.ShowResponseLoader = false;
        this._assistItems.Add(item);
        if (_session is not null && !_session.Messages.Contains(item))
        {
            _session.Messages.Add(item);
        }
    }

    private bool _showResponseLoader;

    /// <summary>Drives SfAIAssistView.ShowResponseLoader while a response is pending.</summary>
    public bool ShowResponseLoader
    {
        get => _showResponseLoader;
        set
        {
            if (_showResponseLoader == value) return;
            _showResponseLoader = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// replace the static follow-up suggestions on the
    /// AIAssistView with the context-aware ones returned by the AI
    /// service. We don't surface them as additional chat bubbles — the
    /// suggestion strip below the editor is the canonical place.
    /// </summary>
    private void AppendFollowUpSuggestions(IEnumerable<string>? suggestions)
    {
        if (suggestions is null) return;

        var iconColor = Color.FromArgb("#7633DA");
        if (Application.Current != null)
        {
            iconColor = Application.Current.UserAppTheme == AppTheme.Dark
                ? Color.FromArgb("#A572F7")
                : TryGetThemeColor("PrimaryColorLight", fallback: Color.FromArgb("#7633DA"));
        }

        // Replace any existing suggestions so we never accumulate stale
        // ones from a previous turn.
        this._suggestions.Clear();
        foreach (var text in suggestions)
        {
            if (string.IsNullOrWhiteSpace(text)) continue;
            this._suggestions.Add(new AssistSuggestion
            {
                Text = text,
                ImageSource = new FontImageSource { Glyph = "\ue7f5", FontFamily = "MaterialAssets", Color = iconColor },
            });
        }
        this.OnPropertyChanged(nameof(Suggestions));
    }


    private void CancelActiveRequest()
    {
        try
        {
            _cts?.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // Already disposed.
        }
    }

    private void TouchSession()
    {
        if (_session is null) return;
        _data.TouchSession(_session);
    }

    private void OnSuggestionTapped(ISuggestion? suggestion)
    {
        if (suggestion is null || string.IsNullOrWhiteSpace(suggestion.Text))
        {
            return;
        }

        if (_session is null)
        {
            var session = _data.CreateChatSession();
            CurrentSession = session;
            SetSystemContext(session.Agent?.Instructions);
        }

        var item = new AssistItem
        {
            Text = suggestion.Text,
            IsRequested = true,
        };
        this._assistItems.Add(item);
        if (_session is not null && !_session.Messages.Contains(item))
        {
            _session.Messages.Add(item);
            PromoteFirstPromptToTitle(_session, item.Text);
        }

        _ = SendPromptAsync(suggestion.Text);
    }

    private void OnAssistItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        this.OnPropertyChanged(nameof(HasMessages));
        this.OnPropertyChanged(nameof(ShowWelcomeSuggestions));
    }

    /// <summary>
    /// Clears the current conversation so the AIAssistView renders the
    /// initial welcome state again. Called by <see cref="NewChat"/>
    /// when the page appears (for example after the user taps "New chat" in
    /// the navigation drawer).
    /// </summary>
    public void ResetConversation()
    {
        CancelActiveRequest();
        this._inputText = string.Empty;
            this.OnPropertyChanged(nameof(InputText));
        (this.SendCommand as Command)?.ChangeCanExecute();

        if (this._assistItems.Count == 0)
        {
            return;
        }

        this._assistItems.Clear();
    }

    /// <summary>Sets the system context used for the next prompt (e.g. an agent's Behaviour).</summary>
    public void SetSystemContext(string? systemContext)
    {
        _systemContext = systemContext;
    }

    /// <summary>
    /// Safely reads a Color resource from <see cref="Application.Current"/>'s
    /// merged dictionaries. Returns <paramref name="fallback"/> when the
    /// resource is missing or isn't a <see cref="Color"/>, so callers
    /// don't have to write unsafe casts at every use site.
    /// </summary>
    private static Color TryGetThemeColor(string key, Color fallback)
    {
        if (Application.Current?.Resources?.TryGetValue(key, out var value) == true
            && value is Color color)
        {
            return color;
        }
        return fallback;
    }


}
