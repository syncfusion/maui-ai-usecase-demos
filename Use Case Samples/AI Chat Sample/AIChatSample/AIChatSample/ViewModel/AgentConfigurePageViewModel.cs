using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AIChatSample;

/// <summary>
/// Composite view-model for the right-pane of <c>AgentConfigurePage</c>:
/// exposes the agent-configuration form fields directly so the XAML can
/// bind without a "Wrapper.AgentVM.AgentName" path, while still surfacing
/// the shared sidebar / recent-chats data the page needs.
/// </summary>
public partial class AgentConfigurePageViewModel : ObservableObject
{
    public AgentConfigurePageViewModel(IAgentService agentService, IAgentStore agentStore)
    {
        AgentVM = new AgentConfigurationViewModel(agentService, agentStore);
        ChatLayout = new AIMainLayoutViewModel();
        // Reuse the existing logo popup VM so the avatar Border's
        // HasAvatar binding stays live after the popup applies.
        AgentPopUpVM = ChatLayout.AgentPopUpVM;

        // Microsoft Copilot-style assistant that lives in the chat at the
        // bottom of the Configure Agent page. When the user types a prompt,
        // the AI fills the Agent Name / Description / Behaviour fields in
        // place — no navigation.
        Assistant = new AgentConfigAssistantViewModel(AgentVM);

        // The popup writes its Apply result into AgentPopUpVM.AgentAvatar;
        // mirror that onto AgentVM.AgentAvatar so the form's Create flow
        // (and the configure/preview avatar circle) see the customized logo.
        AgentPopUpVM.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(AgentLogoPopupViewModel.AgentAvatar))
            {
                AgentVM.AgentAvatar = AgentPopUpVM.AgentAvatar;
            }
        };

        // Forward agent-form property changes so IsCreateEnabled stays live
        // on this VM too (the Create Agent button binds here).
        AgentVM.PropertyChanged += (s, e) =>
        {
            OnPropertyChanged(e.PropertyName);
        };
    }

    /// <summary>Underlying agent-configuration view-model (form + tabs).</summary>
    public AgentConfigurationViewModel AgentVM { get; }

    /// <summary>Sidebar / drawer / recent-chats state shared with the other pages.</summary>
    public AIMainLayoutViewModel ChatLayout { get; }

    /// <summary>Shared agent-logo popup state. Lives on the layout VM so the avatar tap can push <c>AgentLogoPage</c> against the same instance the configure form binds to.</summary>
    public AgentLogoPopupViewModel AgentPopUpVM { get; }

    /// <summary>
    /// Microsoft Copilot-style assistant bound to the SfAIAssistView at
    /// the bottom of the page. Sending a message here fills the
    /// <see cref="AgentConfigurationViewModel"/> form fields in place.
    /// </summary>
    public AgentConfigAssistantViewModel Assistant { get; }

    // ---- Flattened properties so XAML can bind "AgentName" / "Instructions"
    //      / "SelectedTab" / "IsConfigureMode" / "IsPreviewMode" / etc. directly. ----

    /// <summary>TwoWay-bound wrapper over <see cref="AgentConfigurationViewModel.AgentName"/>.</summary>
    public string? AgentName
    {
        get => AgentVM.AgentName;
        set => AgentVM.AgentName = value;
    }

    /// <summary>TwoWay-bound wrapper over <see cref="AgentConfigurationViewModel.Description"/>.</summary>
    public string? Description
    {
        get => AgentVM.Description;
        set => AgentVM.Description = value;
    }

    /// <summary>TwoWay-bound wrapper over <see cref="AgentConfigurationViewModel.Instructions"/>.</summary>
    public string? Instructions
    {
        get => AgentVM.Instructions;
        set => AgentVM.Instructions = value;
    }

    /// <summary>TwoWay-bound wrapper over <see cref="AgentConfigurationViewModel.CompanyName"/>.</summary>
    public string? CompanyName
    {
        get => AgentVM.CompanyName;
        set => AgentVM.CompanyName = value;
    }

    /// <summary>TwoWay-bound wrapper over <see cref="AgentConfigurationViewModel.AgentAvatar"/>.</summary>
    public string? AgentAvatar
    {
        get => AgentVM.AgentAvatar;
        set => AgentVM.AgentAvatar = value;
    }

    /// <summary>TwoWay-bound wrapper over <see cref="AgentConfigurationViewModel.SelectedTab"/>.</summary>
    public int SelectedTab
    {
        get => AgentVM.SelectedTab;
        set => AgentVM.SelectedTab = value;
    }

    /// <summary>Configure / Preview mode labels rendered as chips at the top of the right pane.</summary>
    public string[] ModeTabs => AgentVM.ModeTabs;

    /// <summary>True while the right pane is in "Configure" mode.</summary>
    public bool IsConfigureMode => AgentVM.IsConfigureMode;

    /// <summary>True while the right pane is in "Preview" mode.</summary>
    public bool IsPreviewMode => AgentVM.IsPreviewMode;

    /// <summary>TwoWay-bound text of the active mode chip.</summary>
    public string? SelectedChipText
    {
        get => AgentVM.SelectedChipText;
        set => AgentVM.SelectedChipText = value;
    }

    /// <summary>True when the agent has a custom avatar configured.</summary>
    public bool HasAvatar => AgentVM.HasAvatar;

    /// <summary>True when the Create button can be pressed (name + instructions valid, not busy).</summary>
    public bool IsCreateEnabled => AgentVM.IsCreateEnabled;

    /// <summary>True while a Create request is in flight.</summary>
    public bool IsBusy => AgentVM.IsBusy;

    /// <summary>True when the Create flow surfaced an error.</summary>
    public bool HasError => AgentVM.HasError;

    /// <summary>Last error surfaced by the Create flow.</summary>
    public string? ErrorMessage => AgentVM.ErrorMessage;

    /// <summary>True after a successful Create so the page can pop back.</summary>
    public bool CreatedSuccessfully => AgentVM.CreatedSuccessfully;

    /// <summary>InputCommand for the AIChatInputEditor to bind in the EditorViewTemplate. Accepts string text and creates RequestEventArgs.</summary>
    public ICommand InputCommand => new Command<string>(OnEditorInput);

    private void OnEditorInput(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        // Create an AssistItem from the typed text
        var userItem = new Syncfusion.Maui.AIAssistView.AssistItem 
        { 
            Text = text, 
            IsRequested = true 
        };

        // Create RequestEventArgs and invoke the Assistant's RequestCommand
        var args = new Syncfusion.Maui.AIAssistView.RequestEventArgs(userItem);
        if (Assistant.RequestCommand.CanExecute(args))
        {
            Assistant.RequestCommand.Execute(args);
        }
    }

    // ---- Inline name-required validation (FR-1.2 / AC-1.2) ----
    public bool HasNameValidationError => AgentVM.HasNameValidationError;
    public string NameValidationMessage => AgentVM.NameValidationMessage;

    /// <summary>Called by the page when the name <c>Entry</c> gains focus; promotes the inline error (FR-1.2).</summary>
    public void NotifyNameInteracted() => AgentVM.NotifyNameInteracted();

    /// <summary>Pre-fills the form fields from an AI-generated proposal.</summary>
    public void InitializeFromDraft(AgentDraft? draft) => AgentVM.InitializeFromDraft(draft);
}
