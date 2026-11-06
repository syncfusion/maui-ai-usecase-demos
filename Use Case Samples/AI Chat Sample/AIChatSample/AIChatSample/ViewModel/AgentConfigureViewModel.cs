using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;



namespace AIChatSample;

/// <summary>
/// View-model that backs ONLY the right-pane of AgentConfigurePage:
/// Configure / Preview tabs, form fields, agent avatar placeholder,
/// and Create / Reset commands. Pure MVVM — no UI types referenced.
/// </summary>
public partial class AgentConfigurationViewModel : ObservableObject
{
    private readonly IAgentService _agentService;
    private readonly IAgentStore _agentStore;

    /// <summary>
    /// Set to true by the Create command after a successful save so the
    /// page knows it can pop back (unit-testable, no UI dependency).
    /// </summary>
    public bool CreatedSuccessfully { get; private set; }

    public AgentConfigurationViewModel(IAgentService agentService, IAgentStore agentStore)
    {
        _agentService = agentService;
        _agentStore = agentStore;

        ModeTabs = new[] { "Configure", "Preview" };
        SelectedTab = 0;                            // start on "Configure"

        // Pre-fill the long-form Instructions text so the field matches the
        // screenshot on first render.
        Instructions =
            "You are a Senior Data Analyst AI. Your role is to help users interpret complex datasets, generate SQL queries, and provide actionable insights. Always structure your responses with clear headings and use code blocks for any technical outputs. Maintain a professional, concise tone.";
    }

    // Prefill from a template card tapped in the AIAssistView suggestions.
    public void InitializeFromTemplate(TemplateCardSuggestion? template)
    {
        if (template is null)
        {
            return;
        }

        AgentName = template.TitleText ?? template.Text;
        Description = template.DescriptionText;
    }

    /// <summary>
    /// Pre-fills the form from an AI-drafted agent proposal (the new
    /// prompt-driven Copilot flow on the CreateAgent page). The user can
    /// still tweak any field before pressing Create.
    /// </summary>
    public void InitializeFromDraft(AgentDraft? draft)
    {
        if (draft is null)
        {
            return;
        }

        AgentName = string.IsNullOrWhiteSpace(draft.Name) ? AgentName : draft.Name.Trim();
        Description = string.IsNullOrWhiteSpace(draft.Description) ? Description : draft.Description.Trim();
        if (!string.IsNullOrWhiteSpace(draft.Instructions))
        {
            Instructions = draft.Instructions.Trim();
        }
    }

    // ---------- Tabs ----------
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsConfigureMode))]
    [NotifyPropertyChangedFor(nameof(IsPreviewMode))]
    [NotifyPropertyChangedFor(nameof(SelectedChipText))]
    private int selectedTab;

    public string[] ModeTabs { get; }
    public bool IsConfigureMode => SelectedTab == 0;
    public bool IsPreviewMode => SelectedTab == 1;

    /// <summary>Text of the active mode chip. Mirrors <see cref="SelectedTab"/> for the SfChipGroup binding.</summary>
    public string? SelectedChipText
    {
        get => (SelectedTab >= 0 && SelectedTab < ModeTabs.Length) ? ModeTabs[SelectedTab] : null;
        set
        {
            if (string.IsNullOrEmpty(value)) return;
            var idx = Array.IndexOf(ModeTabs, value);
            if (idx >= 0) SelectedTab = idx;
        }
    }

    // ---------- Form fields (all TwoWay-bound) ----------
    [ObservableProperty] private string? agentName;
    [ObservableProperty] private string? description;
    [ObservableProperty] private string? instructions;
    [ObservableProperty] private string? companyName;

    // ---------- Avatar placeholder ----------
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasAvatar))]
    private string? agentAvatar;

    public bool HasAvatar => !string.IsNullOrEmpty(AgentAvatar);

    // ---------- Validation ----------
    public bool IsCreateEnabled =>
        !IsBusy &&
        !string.IsNullOrWhiteSpace(AgentName) &&
        !string.IsNullOrWhiteSpace(Instructions);

    /// <summary>
    /// True when the user has interacted with (or attempted to submit)
    /// the name field and left it empty / whitespace. Drives the inline
    /// "Agent name is required" error label under the field.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNameValidationError))]
    private bool hasInteractedWithName;

    /// <summary>True when the agent-name field is currently empty / whitespace AND the user has touched it. Drives the inline error label.</summary>
    public bool HasNameValidationError =>
        HasInteractedWithName && string.IsNullOrWhiteSpace(AgentName);

    /// <summary>Static copy of the inline error; bound to the label under the name field so screen readers and unit tests can pin the message.</summary>
    public string NameValidationMessage => "Agent name is required.";

    /// <summary>
    /// Called by the view (code-behind) when the name <c>Entry</c> is
    /// focused for the first time so the inline error can be shown from
    /// that point on. Also called by <see cref="CreateAgentAsync"/>
    /// when the user clicks Create with an empty name.
    /// </summary>
    public void NotifyNameInteracted()
    {
        if (!HasInteractedWithName)
        {
            HasInteractedWithName = true;
        }
    }

    partial void OnAgentNameChanged(string? value)
    {
        OnPropertyChanged(nameof(IsCreateEnabled));
        OnPropertyChanged(nameof(HasNameValidationError));
    }
    partial void OnInstructionsChanged(string? value) => OnPropertyChanged(nameof(IsCreateEnabled));
    partial void OnIsBusyChanged(bool value) => OnPropertyChanged(nameof(IsCreateEnabled));

    // ---------- Busy / error state ----------
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCreateEnabled))]
    private bool isBusy;

    [ObservableProperty]
    private string? errorMessage;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    partial void OnErrorMessageChanged(string? value) => OnPropertyChanged(nameof(HasError));

    // ---------- Commands ----------
    [RelayCommand] private void SwitchToConfigure() => SelectedTab = 0;
    [RelayCommand] private void SwitchToPreview() => SelectedTab = 1;

    [RelayCommand(CanExecute = nameof(IsCreateEnabled))]
    private async Task CreateAgentAsync(CancellationToken ct)
    {
        // Surface the inline name-required error the first time the user
        // clicks Create with an empty name, instead of silently doing
        // nothing. The Create button is already disabled in this case,
        // but we also want the error to appear if the user hit Enter /
        // an accelerator / etc.
        if (string.IsNullOrWhiteSpace(AgentName))
        {
            HasInteractedWithName = true;
            ErrorMessage = NameValidationMessage;
            return;
        }

        if (!IsCreateEnabled)
        {
            return;
        }

        CreatedSuccessfully = false;
        ErrorMessage = null;
        IsBusy = true;
        CreateAgentCommand.NotifyCanExecuteChanged();

        try
        {
            if (!await _agentService.ValidateNameNotTakenAsync(AgentName!.Trim(), ct))
            {
                ErrorMessage = $"An agent named '{AgentName}' already exists. Choose a different name.";
                return;
            }

            var cfg = new AgentConfiguration
            {
                AgentName = AgentName!.Trim(),
                AgentDescription = Description?.Trim(),
                Instructions = Instructions?.Trim(),
                CompanyName = CompanyName?.Trim(),
                CreatedUtc = DateTime.UtcNow,
                AvatarSource = AgentAvatar is { } avatar && !avatar.Contains(';') ? avatar : null,
                AvatarGlyph = AgentAvatar is { } encoded && encoded.Contains(';') ? encoded : null,
            };

            // Every agent gets a unique Id, a default avatar
            // color and an initial derived from its name. The store
            // already enforces name-uniqueness; the Id is a fresh Guid
            // so chats / nav rows can bind back to a specific agent
            // without re-using the (mutable) display name.
            cfg.EnsureIdentityFields();

            // Preserve a custom Color the user picked in the logo editor
            // when present (AgentAvatar starts with "Font;Glyph;Color" in
            // Create mode; EnsureIdentityFields would otherwise overwrite it
            // with a deterministic palette pick).
            if (AgentAvatar is { } custom && custom.Contains(';'))
            {
                var parts = custom.Split(';');
                if (parts.Length >= 3 && !string.IsNullOrEmpty(parts[2]))
                {
                    cfg.Color = parts[2];
                }
            }

            var created = await _agentService.CreateAgentAsync(cfg, ct);
            _agentStore.Add(created);
            CreatedSuccessfully = true;
        }
        catch (OperationCanceledException)
        {
            // User cancelled (e.g., navigated away) — silently ignore.
            ErrorMessage = null;
        }
        catch (HttpRequestException)
        {
            ErrorMessage = "Network error. The agent was not created. Check your connection and retry.";
        }
        catch (InvalidOperationException)
        {
            // Race: name taken between validation and create.
            ErrorMessage = $"An agent named '{AgentName}' already exists. Choose a different name.";
        }
        finally
        {
            IsBusy = false;
            CreateAgentCommand.NotifyCanExecuteChanged();
        }
    }

    [RelayCommand]
    private void Reset()
    {
        AgentName = string.Empty;
        Description = string.Empty;
        Instructions = string.Empty;
        CompanyName = string.Empty;
        AgentAvatar = null;
        SelectedTab = 0;
        ErrorMessage = null;
        CreatedSuccessfully = false;
    }
}