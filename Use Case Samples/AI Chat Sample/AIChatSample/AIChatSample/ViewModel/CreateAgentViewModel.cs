using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;



using CommunityToolkit.Mvvm.ComponentModel;
using Syncfusion.Maui.AIAssistView;

namespace AIChatSample;

/// <summary>
/// View-model for the Copilot-style "create new agent" prompt page.
/// Owns the prompt text, runs the AI-driven <see cref="IAIService.DraftAgentAsync"/>
/// flow when the user taps send, and raises <see cref="DraftReady"/>
/// so the host page can navigate to the agent-configure screen with
/// the proposed fields pre-filled.
/// </summary>
public partial class CreateAgentViewModel : ObservableObject
{
    private readonly IAgentStore? _agentStore;
    private readonly IAIService? _aiService;
    private CancellationTokenSource? _draftCts;

    public CreateAgentViewModel(IAgentStore? agentStore = null, IAIService? aiService = null)
    {
        this.EnableSendIcon = true;
        this.IsHeaderVisible = true;
        this.IsEditorVisible = true;

        this.Suggestions = new ObservableCollection<ISuggestion>
        {
            new TemplateCardSuggestion
            {
                Text = "Research agent",
                IconGlyph = "\ue707",
                IconFontFamily = "FontIcons",
                IconBackground = "#F1ECFE",
                TitleText = "Research agent",
                DescriptionText = "Finds and synthesizes information from multiple sources automatically.",
                Section = CardSection.Template,
                IsAgentTemplate = true,
            },
            new TemplateCardSuggestion
            {
                Text = "Customer support agent",
                IconGlyph = "\ue723",
                IconFontFamily = "FontIcons",
                IconBackground = "#F1ECFE",
                TitleText = "Customer support agent",
                DescriptionText = "Handles common inquiries and routes complex issues to your team.",
                Section = CardSection.Template,
                IsAgentTemplate = true,
            },
            new TemplateCardSuggestion
            {
                Text = "Data analysis agent",
                IconGlyph = "\ue745",
                IconFontFamily = "FontIcons",
                IconBackground = "#F1ECFE",
                TitleText = "Data analysis agent",
                DescriptionText = "Interprets datasets, spots trends, and generates visual reports.",
                Section = CardSection.Template,
                IsAgentTemplate = true,
            },
        };

        this.MyAgents = new ObservableCollection<TemplateCardSuggestion>
        {
            new()
            {
                Text = "Meeting notes assistant",
                IconGlyph = "\ue752",
                IconFontFamily = "FontIcons",
                IconBackground = "#F1ECFE",
                TitleText = "Meeting notes assistant",
                DescriptionText = "Turns raw meeting transcripts into structured action items.",
                Section = CardSection.MyAgents,
                IsAgentTemplate = true,
            },
            new()
            {
                Text = "Code reviewer",
                IconGlyph = "\ue737",
                IconFontFamily = "FontIcons",
                IconBackground = "#F1ECFE",
                TitleText = "Code reviewer",
                DescriptionText = "Reviews pull requests and suggests improvements to your code.",
                Section = CardSection.MyAgents,
                IsAgentTemplate = true,
            },
            new()
            {
                Text = "Social media writer",
                IconGlyph = "\ue73B",
                IconFontFamily = "FontIcons",
                IconBackground = "#F1ECFE",
                TitleText = "Social media writer",
                DescriptionText = "Creates engaging posts tailored to each platform's audience.",
                Section = CardSection.MyAgents,
                IsAgentTemplate = true,
            },
        };

        this.HeaderItemTappedCommand = new Command<object>(this.OnCardTapped);
        this.SendButtonCommand = new Command(this.OnSendRequested);
        this.AttachmentButtonCommand = new Command<object>(_ => Debug.WriteLine("[Attach] tapped"));
        this.VoiceCommand = new Command(() => Debug.WriteLine("[Voice] tapped"));

        // Keep the "My Agents" row in sync with created agents.
        _agentStore = agentStore;
        if (_agentStore is not null)
        {
            SyncAgentsFromStore();
            _agentStore.AgentAdded += OnAgentAddedToStore;
        }

        // Resolve the shared AI service so the prompt-driven draft flow can
        // hit the same IAIService the chat uses (mock or Azure depending on
        // configuration). ServiceHelper is optional in design-time.
        _aiService = aiService ?? ServiceHelper.GetService<IAIService>();

        // Copilot-style flow: the SendButtonCommand below drives the
        // prompt-driven draft; OnSendRequested kicks off OnDraftAgentAsync
        // and the page routes to the configure screen once DraftReady fires.
        CancelDraftCommand = new Command(
            () => _draftCts?.Cancel(),
            () => IsBusy);
    }

    /// <summary>Rebuilds the MyAgents collection from the shared store.</summary>
    public void SyncAgentsFromStore()
    {
        if (_agentStore is null)
        {
            return;
        }

        foreach (var agent in _agentStore.Agents)
        {
            if (this.MyAgents.Any(a => string.Equals(a.TitleText, agent.AgentName, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            this.MyAgents.Insert(0, new TemplateCardSuggestion
            {
                Text = agent.AgentName!,
                TitleText = agent.AgentName,
                DescriptionText = agent.AgentDescription ?? "Custom specialist agent.",
                IconGlyph = "\ue736",
                IconFontFamily = "MaterialAssets",
                IconBackground = "#F1ECFE",
                Section = CardSection.MyAgents,
                IsAgentTemplate = true,
            });
        }
    }

    private void OnAgentAddedToStore(object? sender, AgentConfiguration e) => SyncAgentsFromStore();

    #region Properties

    /// <summary>Active chat messages surfaced in <see cref="SfAIAssistView"/>.</summary>
    public ObservableCollection<IAssistItem> Messages { get; } = new();

    /// <summary>Cards fed to <see cref="SfAIAssistView.Suggestions"/>.</summary>
    public ObservableCollection<ISuggestion> Suggestions { get; }

    /// <summary>My Agents cards rendered in a separate SfListView below.</summary>
    public ObservableCollection<TemplateCardSuggestion> MyAgents { get; }

    [ObservableProperty] private string? promptText;
    [ObservableProperty] private bool isBusy;

    [ObservableProperty] private bool enableSendIcon;
    [ObservableProperty] private bool isHeaderVisible;
    [ObservableProperty] private bool isEditorVisible;

    /// <summary>Latest AI-drafted agent proposal; populated when the
    /// Copilot prompt flow returns. Null when no draft is pending.</summary>
    [ObservableProperty] private AgentDraft? lastDraft;

    #endregion

    #region Commands

    public ICommand HeaderItemTappedCommand { get; }

    public ICommand SendButtonCommand { get; }
    public ICommand AttachmentButtonCommand { get; }
    public ICommand VoiceCommand { get; }

    /// <summary>Cancels the in-flight draft request.</summary>
    public ICommand CancelDraftCommand { get; }

    /// <summary>Raised when the AI produces a usable draft and the page
    /// should navigate to the configure screen pre-filled with it.</summary>
    public event EventHandler<AgentDraft>? DraftReady;

    #endregion

    #region Command handlers

    private void OnCardTapped(object? parameter)
    {
        if (parameter is not SuggestionItemSelectedEventArgs args || args.SelectedItem is not TemplateCardSuggestion card)
        {
            return;
        }

        var request = new AssistItem
        {
            Text = card.Text ?? string.Empty,
            IsRequested = true,
        };
        this.Messages.Add(request);

        if (card.Section == CardSection.Template)
        {
            this.Suggestions.Clear();
            this.IsHeaderVisible = false;
        }
    }

    private void OnSendRequested()
    {
        // Capture the prompt locally before clearing it so the AI draft
        // routine has something to send (PromptText is the binding source
        // for the editor and was being reset to "" in this method).
        var prompt = this.PromptText?.Trim();
        if (string.IsNullOrWhiteSpace(prompt))
        {
            return;
        }

        var request = new AssistItem
        {
            Text = prompt,
            IsRequested = true,
        };
        this.Messages.Add(request);
        this.PromptText = string.Empty;

        // Kick off the AI draft asynchronously; when it completes the
        // DraftReady event fires so the page can navigate to the configure
        // screen with the proposed agent name / description / instructions.
        _ = OnDraftAgentAsync(prompt);
    }

    private async Task OnDraftAgentAsync(string? promptOverride = null)
    {
        var prompt = (promptOverride ?? this.PromptText)?.Trim();
        if (string.IsNullOrWhiteSpace(prompt))
        {
            return;
        }
        if (_aiService is null)
        {
            this.Messages.Add(new AssistItem
            {
                Text = "AI service is not configured. Please choose a template or fill the form manually.",
                IsRequested = false,
            });
            return;
        }
        // Capture the prompt locally because OnSendRequested clears it
        // immediately so the editor is ready for the next prompt.
        _draftCts?.Cancel();
        _draftCts = new CancellationTokenSource();
        var ct = _draftCts.Token;
        IsBusy = true;
        (CancelDraftCommand as Command)?.ChangeCanExecute();

        try
        {
            var draft = await _aiService.DraftAgentAsync(prompt, ct).ConfigureAwait(true);
            if (ct.IsCancellationRequested) return;
            if (draft is null)
            {
                this.Messages.Add(new AssistItem
                {
                    Text = "I couldn't draft an agent from that description. Try adding a few more details or pick a template.",
                    IsRequested = false,
                });
                return;
            }

            // Echo the AI's proposal back to the user and signal the page to
            // navigate to the configure screen pre-filled with the draft.
            this.Messages.Add(new AssistItem
            {
                Text = $"How does this look?\n• Name: {draft.Name}\n• Description: {draft.Description}",
                IsRequested = false,
            });
            this.LastDraft = draft;
            DraftReady?.Invoke(this, draft);
        }
        catch (OperationCanceledException)
        {
            // ignore — user cancelled
        }
        catch (Exception ex)
        {
            this.Messages.Add(new AssistItem
            {
                Text = $"Sorry, the draft failed: {ex.Message}",
                IsRequested = false,
            });
        }
        finally
        {
            IsBusy = false;
            (CancelDraftCommand as Command)?.ChangeCanExecute();
        }
    }

    #endregion
}