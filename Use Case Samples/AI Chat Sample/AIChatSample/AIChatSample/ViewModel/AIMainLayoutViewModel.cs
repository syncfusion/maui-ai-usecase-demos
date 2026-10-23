using Syncfusion.Maui.AIAssistView;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AIChatSample;

public partial class AIMainLayoutViewModel : ObservableObject
{
    #region Fields
    public CreateAgentViewModel CreateAgentVM { get; }
    public AIChatNavigationViewModel chatVM { get; }
    public AgentConfigurationViewModel AgentVM { get; }
    public UserProfile UserProfile { get; }
    public LibraryViewModel LibraryVM { get; }
    public ImageLibraryViewModel ImageLibraryVM { get; }
    public AgentLogoPopupViewModel AgentPopUpVM { get; }

    // Sidebar / drawer state
    private bool isDrawerOpen;
    private bool isSecondaryDrawerOpen;
    private bool isAgentSectionExpanded = true;
    private bool isRecentChatSectionExpanded = true;

    // Selection state
    private SidebarItem? selectedMenuItem;
    private AgentItem? selectedAgent;

    private string userName = "Alexa John";
    private string userAvatar = "userprofile.png";
    private string profileName = "Alexa John";
    private string profileEmail = "alex.dev@example.com";
    private string profileErrorMessage = string.Empty;

    // Commands
    private ICommand? toggleSidebarCommand;
    private ICommand? openMessagesCommand;
    private ICommand? closeSecondaryDrawerCommand;
    private ICommand? toggleAgentSectionCommand;
    private ICommand? toggleRecentChatSectionCommand;
    private ICommand? menuItemSelectedCommand;
    private ICommand? agentSelectedCommand;
    private ICommand? newChatCommand;
    private ICommand? openChatCommand;
    private ICommand? openAgentChatCommand;
    /// <summary>Shared in-memory store. May be null in design-time contexts.</summary>
    private readonly IChatDataService? _chatDataService;

    /// <summary>Shared in-memory agent store. Surfaces new agents so the nav-drawer list and "My Agents" row stay in sync.</summary>
    private readonly IAgentStore? _agentStore;

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="AIMainLayoutViewModel"/> class
    /// and seeds the sidebar with default menu items, agents, and recent chats.
    /// </summary>
    public AIMainLayoutViewModel()
    {
        // Resolve from the app service provider when running; fall back
        // to isolated in-memory implementations for design/test contexts.
        var store = ServiceHelper.GetService<IAgentStore>() ?? new InMemoryAgentStore();
        var agentService = ServiceHelper.GetService<IAgentService>() ?? new DefaultAgentService(store);
        _chatDataService = ServiceHelper.GetService<IChatDataService>();
        _agentStore = store;

        CreateAgentVM = new CreateAgentViewModel(store);
        AgentVM = new AgentConfigurationViewModel(agentService, store);
        chatVM = new AIChatNavigationViewModel();
        UserProfile = _chatDataService?.Profile ?? new UserProfile();
        profileName = UserProfile.Name;
        profileEmail = UserProfile.Email;
        LibraryVM = new LibraryViewModel();
        ImageLibraryVM = new ImageLibraryViewModel();
        AgentPopUpVM = new AgentLogoPopupViewModel();

        // Surface the user profile (live across the app) and the shared
        // chat history. The Recent chats list re-sorts automatically
        // because the underlying store calls Move() on the observable
        // collection when LastActiveDate changes.
        if (_chatDataService is not null)
        {
            this.UserName = _chatDataService.Profile.Name;
            _chatDataService.Profile.PropertyChanged += OnUserProfileChanged;

            RecentChats = new ObservableCollection<RecentChatItem>(BuildRecentChatItems());
            FilteredRecentChats = new ObservableCollection<RecentChatItem>();
            _chatDataService.ChatSessionsChanged += OnChatSessionsChanged;
        }
        this.MenuItems = new ObservableCollection<SidebarItem>
        {
            new()
            {
                Title = "New chat",
                IconGlyph = "\ue70d",
                IconFontFamily = "MaterialAssets",
                IconImageSource = new FontImageSource
                {
                    Glyph = "\ue70d",
                    FontFamily = "MaterialAssets",
                    Color = Application.Current?.RequestedTheme == AppTheme.Dark ? Color.FromArgb("#C9C6C8") : Color.FromArgb("#474648"),
                    Size = 18
                }
            },
            new()
            {
                Title = "Search",
                IconGlyph = "\ue715",
                IconFontFamily = "MaterialAssets",
                IconImageSource = new FontImageSource
                {
                    Glyph = "\ue715",
                    FontFamily = "MaterialAssets",
                    Color = Application.Current?.RequestedTheme == AppTheme.Dark ? Microsoft.Maui.Graphics.Color.FromArgb("#C9C6C8") : Microsoft.Maui.Graphics.Color.FromArgb("#474648"),
                    Size = 18
                }
            }
        };

        this.Agents = new ObservableCollection<AgentItem>
        {
            new()
            {
                Id = DefaultAgentIds.MeetingNotesAssistant,
                Name = "Meeting notes assistant",
                Initial = "M",
                Color = Microsoft.Maui.Graphics.Color.FromArgb("#FF33DA"),
                AvatarGlyph = "FontIcons;\ue752;#FFFFFF",
            },
            new()
            {
                Id = DefaultAgentIds.CodeReviewer,
                Name = "Code Reviewer",
                Initial = "C",
                Color = Microsoft.Maui.Graphics.Color.FromArgb("#7633DA"),
                AvatarGlyph = "FontIcons;\ue737;#FFFFFF",
            },
            new()
            {
                Id = DefaultAgentIds.SocialMediaWriter,
                Name = "Social Media Writer",
                Initial = "S",
                Color = Microsoft.Maui.Graphics.Color.FromArgb("#D55E2C"),
                AvatarGlyph = "FontIcons;\ue73B;#FFFFFF",
            },
        };

        // Surface any agents already persisted in the store
        // (in this session; the in-memory store doesn't survive
        // restarts). We prepend so the most recently created agent
        // is closest to the top — matches the "Recent" pattern.
        if (_agentStore is not null)
        {
            SeedDefaultAgentsIfMissing(_agentStore);
            foreach (var existing in _agentStore.Agents)
            {
                AddAgentRow(existing);
            }
            _agentStore.AgentAdded += OnAgentAddedToStore;
        }

        // RecentChats is initialised above when a chat data service is
        // available; otherwise seed three demo entries so the UI
        // remains meaningful in design / XAML-preview contexts.
        if (this.RecentChats is null)
        {
            this.RecentChats = new ObservableCollection<RecentChatItem>
            {
                new() { Title = "Provide a prompt to create the",   Time = "2m", Glyph = "\ue711" },
                new() { Title = "What is API driven annotation for",   Time = "2m", Glyph = "\ue711" },
                new() { Title = "What is API driven annotation for", Time = "1h", Glyph = "\ue711" },
            };
        }

        Application.Current!.RequestedThemeChanged += OnRequestedThemeChanged;
        this.SelectedMenuItem = this.MenuItems.FirstOrDefault();
        this.SelectedAgent = this.Agents.FirstOrDefault();
    }

    #region Properties
    private void OnRequestedThemeChanged(object? sender, AppThemeChangedEventArgs e)
    {
        UpdateMenuItemColors(e.RequestedTheme);
    }
    private void UpdateMenuItemColors(AppTheme theme)
    {
        var iconColor = theme == AppTheme.Dark
            ? Color.FromArgb("#C9C6C8")
            : Color.FromArgb("#474648");

        foreach (var item in MenuItems)
        {
            item.IconImageSource = new FontImageSource
            {
                Glyph = item.IconGlyph,
                FontFamily = item.IconFontFamily,
                Color = iconColor,
                Size = 18
            };
        }
    }
    /// <summary>Top-level menu items shown in the primary drawer.</summary>
    public ObservableCollection<SidebarItem> MenuItems { get; }

    /// <summary>Agents shown in the "Agent" accordion section.</summary>
    public ObservableCollection<AgentItem> Agents { get; }

    /// <summary>Recently visited chats shown in the "Recent chat" accordion section. Backed by <see cref="IChatDataService"/> when available.</summary>
    public ObservableCollection<RecentChatItem> RecentChats { get; private set; } = new();

    /// <summary>Filtered view of <see cref="RecentChats"/> driven by <see cref="SearchText"/>.</summary>
    public ObservableCollection<RecentChatItem> FilteredRecentChats { get; private set; } = new();

    /// <summary>
    /// Free-text query bound to the Search page's input. Re-filters
    /// <see cref="FilteredRecentChats"/> on every keystroke.
    /// </summary>
    [ObservableProperty]
    private string? searchText;

    partial void OnSearchTextChanged(string? value) => ApplySearchFilter();

    /// <summary>Gets or sets a value indicating whether the primary drawer is open.</summary>
    public bool IsDrawerOpen
    {
        get => this.isDrawerOpen;
        set => this.SetProperty(ref this.isDrawerOpen, value);
    }

    /// <summary>Gets or sets a value indicating whether the secondary (right) drawer is open.</summary>
    public bool IsSecondaryDrawerOpen
    {
        get => this.isSecondaryDrawerOpen;
        set => this.SetProperty(ref this.isSecondaryDrawerOpen, value);
    }

    public bool IsAgentSectionExpanded
    {
        get => this.isAgentSectionExpanded;
        set => this.SetProperty(ref this.isAgentSectionExpanded, value);
    }

    public bool IsRecentChatSectionExpanded
    {
        get => this.isRecentChatSectionExpanded;
        set => this.SetProperty(ref this.isRecentChatSectionExpanded, value);
    }

    public SidebarItem? SelectedMenuItem
    {
        get => this.selectedMenuItem;
        set
        {
            if (this.selectedMenuItem == value) return;
            this.selectedMenuItem = value;
            this.OnPropertyChanged(nameof(this.SelectedMenuItem));
            this.OnPropertyChanged(nameof(this.HasSelectedMenuItem));
        }
    }

    public AgentItem? SelectedAgent
    {
        get => this.selectedAgent;
        set
        {
            if (this.selectedAgent == value) return;
            this.selectedAgent = value;
            this.OnPropertyChanged(nameof(this.SelectedAgent));
        }
    }

    public string UserName
    {
        get => this.userName;
        set => this.SetProperty(ref this.userName, value);
    }

    public string UserAvatar
    {
        get => this.userAvatar;
        set => this.SetProperty(ref this.userAvatar, value);
    }

    public string ProfileName
    {
        get => this.profileName;
        set => this.SetProperty(ref this.profileName, value);
    }

    public string ProfileEmail
    {
        get => this.profileEmail;
        set => this.SetProperty(ref this.profileEmail, value);
    }

    public string ProfileErrorMessage
    {
        get => this.profileErrorMessage;
        set => this.SetProperty(ref this.profileErrorMessage, value);
    }

    /// <summary>True when a menu item has been selected (used by drawer isVisible / chevron).</summary>
    public bool HasSelectedMenuItem => this.selectedMenuItem is not null;

    #endregion

    #region Commands

    /// <summary>Toggles the primary (left) navigation drawer open / closed.</summary>
    public ICommand ToggleSidebarCommand =>
        this.toggleSidebarCommand ??= new Command(() => this.IsDrawerOpen = !this.IsDrawerOpen);

    /// <summary>Opens the secondary (right) messages drawer.</summary>
    public ICommand OpenMessagesCommand =>
        this.openMessagesCommand ??= new Command(() => this.IsSecondaryDrawerOpen = true);

    /// <summary>Closes the secondary drawer.</summary>
    public ICommand CloseSecondaryDrawerCommand =>
        this.closeSecondaryDrawerCommand ??= new Command(() => this.IsSecondaryDrawerOpen = false);

    /// <summary>Expands / collapses the Agent accordion section.</summary>
    public ICommand ToggleAgentSectionCommand =>
        this.toggleAgentSectionCommand ??= new Command(() => this.IsAgentSectionExpanded = !this.IsAgentSectionExpanded);

    /// <summary>Expands / collapses the Recent chat accordion section.</summary>
    public ICommand ToggleRecentChatSectionCommand =>
        this.toggleRecentChatSectionCommand ??= new Command(() => this.IsRecentChatSectionExpanded = !this.IsRecentChatSectionExpanded);

    /// <summary>Receives the user's menu selection and updates <see cref="SelectedMenuItem"/>.</summary>
    public ICommand MenuItemSelectedCommand =>
        this.menuItemSelectedCommand ??= new Command<SidebarItem?>(this.OnMenuItemSelected);

    /// <summary>Receives the user's agent selection and updates <see cref="SelectedAgent"/>.</summary>
    public ICommand AgentSelectedCommand =>
        this.agentSelectedCommand ??= new Command<AgentItem?>(this.OnAgentSelected);

    /// <summary>Creates a new chat session and raises <see cref="ChatSessionReplaced"/> so the host page can swap the active conversation.</summary>
    public ICommand NewChatCommand =>
        this.newChatCommand ??= new Command(this.OnNewChatClicked);

    /// <summary>Opens an existing chat session (parameter: the <see cref="RecentChatItem"/> tapped by the user). Raises <see cref="ChatSessionReplaced"/>.</summary>
    public ICommand OpenChatCommand =>
        this.openChatCommand ??= new Command<RecentChatItem?>(this.OnOpenChatClicked);

    /// <summary>Opens an existing chat session (parameter: the <see cref="RecentChatItem"/> tapped by the user). Raises <see cref="ChatSessionReplaced"/>.</summary>
    public ICommand OpenAgentChatCommand =>
        this.openAgentChatCommand ??= new Command<AgentItem?>(this.OnOpenAgentChatClicked);

    /// <summary>Opened or created chat session. The host page (NewChat) listens to this and binds the new conversation into the AI AssistView.</summary>
    public event EventHandler<ChatSession>? ChatSessionReplaced;

    /// <summary>Currently open chat session, or null when the host should display the welcome header.</summary>
    public ChatSession? CurrentSession
    {
        get => _currentSession;
        private set
        {
            if (ReferenceEquals(_currentSession, value)) return;
            _currentSession = value;
            OnPropertyChanged();
        }
    }
    private ChatSession? _currentSession;

    /// <summary>Persists the user's profile changes (display name + email) to the shared <see cref="UserProfile"/>.</summary>
    public ICommand SaveProfileCommand => new Command(this.OnSaveProfileClicked);

    #endregion

    #region Methods

    private void OnMenuItemSelected(SidebarItem? item)
    {
        if (item is null) return;

        foreach (var i in this.MenuItems)
        {
            i.IsSelected = ReferenceEquals(i, item);
        }

        this.SelectedMenuItem = item;
    }

    private void OnAgentSelected(AgentItem? item)
    {
        if (item is null) return;

        foreach (var a in this.Agents)
        {
            a.IsSelected = ReferenceEquals(a, item);
        }

        this.SelectedAgent = item;
    }

    private void OnNewChatClicked()
    {
        // A fresh chat is a fresh ChatSession. We never mutate
        // an existing session in-place; opening "New chat" from the
        // drawer always creates a new one.
        if (_chatDataService is null) return;
        CurrentSession = _chatDataService.CreateChatSession();
        ChatSessionReplaced?.Invoke(this, CurrentSession);
    }

    private void OnOpenChatClicked(RecentChatItem? item)
    {
        if (item is null || _chatDataService is null) return;

        // Look the session back up by id; RecentChatItem carries the
        // ChatSession.Id under the hood via Tag.
        var session = item.Tag as ChatSession
            ?? _chatDataService.ChatSessions.FirstOrDefault(s => s.Id == item.Tag as string);
        if (session is null) return;

        _chatDataService.TouchSession(session);
        CurrentSession = session;
        ChatSessionReplaced?.Invoke(this, session);
    }

    /// <summary>Open the chat carried by the supplied recent item.</summary>
    public void OpenRecentChat(RecentChatItem? item) => OnOpenChatClicked(item);

    /// <summary>Delete the chat carried by the supplied recent item.</summary>
    public void DeleteRecentChat(RecentChatItem? item)
    {
        if (item is null || _chatDataService is null) return;
        var session = item.Tag as ChatSession
            ?? _chatDataService.ChatSessions.FirstOrDefault(s => s.Id == item.Tag as string);
        if (session is null) return;

        if (ReferenceEquals(CurrentSession, session))
        {
            CurrentSession = null;
        }
        _chatDataService.DeleteSession(session);
        ChatSessionReplaced?.Invoke(this, CurrentSession!);
    }

    /// <summary>Build a shareable deep link / display string for a chat.</summary>
    public string BuildShareLink(RecentChatItem? item)
    {
        if (item is null) return string.Empty;
        var session = item.Tag as ChatSession
            ?? _chatDataService?.ChatSessions.FirstOrDefault(s => s.Id == item.Tag as string);
        var title = session?.Title ?? item.Title ?? "chat";
        return $"aichat://chat/{session?.Id ?? item.Title}/{Uri.EscapeDataString(title)}";
    }

    private void OnOpenAgentChatClicked(AgentItem? item)
    {
        if (item is null || _chatDataService is null || _agentStore is null) return;

        // Resolve the drawer row against the shared store. Prefer the
        // visible Name so a user-created agent with the same name wins
        // over the seed entry, then fall back to the stable Id.
        var agent =
            _agentStore.Agents.FirstOrDefault(a =>
                string.Equals(a.AgentName, item.Name, StringComparison.OrdinalIgnoreCase))
            ?? _agentStore.Agents.FirstOrDefault(a =>
                !string.IsNullOrEmpty(item.Id) &&
                string.Equals(a.Id, item.Id, StringComparison.OrdinalIgnoreCase));

        if (agent is null)
        {
            return;
        }

        SelectedAgent = item;
        CurrentSession = _chatDataService.CreateChatSession(agent.AgentName, agent);
        ChatSessionReplaced?.Invoke(this, CurrentSession);
    }

    private void OnSaveProfileClicked()
    {
        var name = (ProfileName ?? string.Empty).Trim();
        var email = (ProfileEmail ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            ProfileErrorMessage = "Display name is required.";
            return;
        }

        if (!string.IsNullOrWhiteSpace(email) && !email.Contains('@'))
        {
            ProfileErrorMessage = "Enter a valid email address.";
            return;
        }

        ProfileErrorMessage = string.Empty;
        UserProfile.Name = name;
        UserProfile.Email = email;
        UserName = UserProfile.Name;

        if (_chatDataService is not null)
        {
            _chatDataService.Profile.Name = name;
            _chatDataService.Profile.Email = email;
        }
    }

    private void OnUserProfileChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_chatDataService is null) return;
        // Nav header initial/name updates live.
        if (e.PropertyName is nameof(UserProfile.Name) or nameof(UserProfile.Initial))
        {
            UserName = _chatDataService.Profile.Name;
            ProfileName = _chatDataService.Profile.Name;
        }
        else if (e.PropertyName is nameof(UserProfile.Email))
        {
            ProfileEmail = _chatDataService.Profile.Email;
        }
    }

    private void OnChatSessionsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Re-render the Recent chats list whenever the store is
        // mutated (e.g. a new session is created, or TouchSession
        // re-sorts the collection).
        if (_chatDataService is null) return;
        RecentChats = new ObservableCollection<RecentChatItem>(BuildRecentChatItems());
        OnPropertyChanged(nameof(RecentChats));
        ApplySearchFilter();
    }

    /// <summary>
    /// Inserts a single <see cref="AgentItem"/> row for the freshly
    /// created agent. De-duplicated by id and by
    /// case-insensitive name so the seeded design-time defaults
    /// don't appear twice. Future avatar edits reuse the same row to
    /// surface the customized avatar.
    /// </summary>
    private void AddAgentRow(AgentConfiguration agent)
    {
        if (agent is null || string.IsNullOrWhiteSpace(agent.AgentName))
        {
            return;
        }

        foreach (var existing in Agents)
        {
            if (!string.IsNullOrEmpty(existing.Id) && existing.Id == agent.Id)
            {
                return;
            }

            if (string.Equals(existing.Name, agent.AgentName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
        }

        // Convert stored/derived string color into Microsoft.Maui.Graphics.Color
        Color? parsedColor = null;
        if (!string.IsNullOrWhiteSpace(agent.Color))
        {
            if (Color.TryParse(agent.Color, out var c))
            {
                parsedColor = c;
            }
            else
            {
                // Fallback: try FromArgb (throws on invalid input) inside try/catch
                try
                {
                    parsedColor = Color.FromArgb(agent.Color);
                }
                catch
                {
                    parsedColor = null;
                }
            }
        }

        if (parsedColor is null)
        {
            var derived = AgentConfiguration.DeriveColor(agent.AgentName);
            if (!string.IsNullOrWhiteSpace(derived) && Color.TryParse(derived, out var c2))
            {
                parsedColor = c2;
            }
            else if (!string.IsNullOrWhiteSpace(derived))
            {
                try
                {
                    parsedColor = Color.FromArgb(derived);
                }
                catch
                {
                    parsedColor = null;
                }
            }
        }

        Agents.Insert(0, new AgentItem
        {
            Id = agent.Id,
            Name = agent.AgentName,
            Initial = agent.Initial ?? AgentConfiguration.DeriveInitial(agent.AgentName),
            Color = parsedColor,
            AvatarSource = agent.AvatarSource,
            AvatarGlyph = agent.AvatarGlyph,
        });
    }

    /// <summary>
    /// Raised by <see cref="IAgentStore.AgentAdded"/> when a new
    /// specialist agent is persisted. Pushes a row into the
    /// nav-drawer <see cref="Agents"/> list and leaves
    /// selection unchanged so the user keeps the current focus.
    /// </summary>
    private void OnAgentAddedToStore(object? sender, AgentConfiguration e)
    {
        AddAgentRow(e);
    }

    /// <summary>
    /// Persists the default agents into the shared store the first
    /// time the app runs in this session. The nav-drawer already shows
    /// them as <see cref="AgentItem"/> rows, but <c>OnOpenAgentChatClicked</c>
    /// resolves chats against the store — without these entries, clicking
    /// a default agent returns no match and the chat doesn't open.
    /// </summary>
    private static void SeedDefaultAgentsIfMissing(IAgentStore store)
    {
        if (store is null) return;

        EnsureDefault(store, DefaultAgentIds.MeetingNotesAssistant, "Meeting notes assistant",
            "Turns raw meeting transcripts into structured action items.",
            "You are an expert meeting-notes assistant. Take raw meeting transcripts or notes and produce a clean summary with attendees, decisions, action items, and follow-up dates. Use bullet points and short headings.");
        EnsureDefault(store, DefaultAgentIds.CodeReviewer, "Code Reviewer",
            "Reviews code for bugs, readability, performance and best practices.",
            "You are an expert code reviewer. Analyze the provided code for bugs, security issues, performance problems, readability and best practices. Give concise, actionable feedback with improved code snippets where helpful.");
        EnsureDefault(store, DefaultAgentIds.SocialMediaWriter, "Social Media Writer",
            "Writes engaging, platform-ready social media posts.",
            "You are a creative social media writer. Produce engaging, concise posts tailored to the requested platform and tone, with relevant hashtags and a clear call to action.");
    }

    private static void EnsureDefault(IAgentStore store, string id, string name, string description, string instructions)
    {
        if (store.Agents.Any(a =>
                !string.IsNullOrEmpty(a.Id) &&
                string.Equals(a.Id, id, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        // Match by name too so a user who somehow already created
        // "Code Reviewer" without our id still won't see a duplicate.
        if (store.IsNameTaken(name))
        {
            return;
        }

        var config = new AgentConfiguration
        {
            Id = id,
            AgentName = name,
            AgentDescription = description,
            Instructions = instructions,
        };
        config.EnsureIdentityFields();
        store.Add(config);
    }

    private void ApplySearchFilter()
    {
        // The Search page lists all chats by default; the query
        // narrows the list. Opening the page should not show an empty
        // list — that's the "haven't searched yet" feel the user
        // complained about. We still skip empty sessions so demo
        // "New chat" stubs don't clutter the results.
        if (FilteredRecentChats is null)
        {
            FilteredRecentChats = new ObservableCollection<RecentChatItem>();
        }

        FilteredRecentChats.Clear();
        var query = (SearchText ?? string.Empty).Trim();

        foreach (var item in RecentChats)
        {
            if (query.Length == 0 ||
                (item.Title?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false))
            {
                FilteredRecentChats.Add(item);
            }
        }
        OnPropertyChanged(nameof(FilteredRecentChats));
    }

    private IEnumerable<RecentChatItem> BuildRecentChatItems()
    {
        if (_chatDataService is null) yield break;
        foreach (var session in _chatDataService.ChatSessions)
        {
            // Only surface sessions that have at least one user prompt.
            // Just opening New chat / an agent creates a session, but
            // those haven't been used yet and shouldn't appear in
            // Recent chats or the Search page.
            if (!HasUserPrompt(session)) continue;
            yield return new RecentChatItem
            {
                Title = ResolveChatTitle(session),
                Time = FormatRelative(session.LastActiveDate),
                Glyph = "\ue711",
                Tag = session,
            };
        }
    }

    /// <summary>True if the session contains at least one user (requested) message.</summary>
    private static bool HasUserPrompt(ChatSession session)
    {
        if (session is null) return false;
        foreach (var message in session.Messages)
        {
            if (message is AssistItem ai && ai.IsRequested && !string.IsNullOrWhiteSpace(ai.Text))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Returns the title to surface in Recent chats / Search. The session
    /// is promoted to the user's first prompt when the stored title is
    /// still the "New chat" placeholder or the agent's name, so the
    /// search list always reflects what was actually asked.
    /// </summary>
    private static string ResolveChatTitle(ChatSession session)
    {
        if (session is null) return "New chat";
        var t = session.Title;
        var isDefault =
            string.IsNullOrWhiteSpace(t) ||
            string.Equals(t, "New chat", StringComparison.OrdinalIgnoreCase) ||
            (session.Agent is not null &&
             string.Equals(t, session.Agent.AgentName, StringComparison.OrdinalIgnoreCase));
        if (!isDefault) return t!;
        foreach (var message in session.Messages)
        {
            if (message is AssistItem ai && ai.IsRequested && !string.IsNullOrWhiteSpace(ai.Text))
            {
                return ai.Text.Length > 80 ? ai.Text.Substring(0, 80) + "…" : ai.Text;
            }
        }
        return "New chat";
    }

    private static string FormatRelative(DateTime utc)
    {
        var delta = DateTime.UtcNow - utc.ToUniversalTime();
        if (delta.TotalSeconds < 60) return "now";
        if (delta.TotalMinutes < 60) return $"{(int)delta.TotalMinutes}m";
        if (delta.TotalHours < 24) return $"{(int)delta.TotalHours}h";
        if (delta.TotalDays < 7) return $"{(int)delta.TotalDays}d";
        return utc.ToLocalTime().ToString("MMM d", CultureInfo.InvariantCulture);
    }

    #endregion

    #region INotifyPropertyChanged (kept for backward compat — ObservableObject handles dispatch)

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        base.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));

    protected void SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(storage, value)) return;
        storage = value;
        this.OnPropertyChanged(propertyName!);
    }

    #endregion
}