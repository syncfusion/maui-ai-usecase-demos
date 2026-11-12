using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SupportReplyAIDraft.Models;
using SupportReplyAIDraft.Services;
using Syncfusion.Maui.Chat;

namespace SupportReplyAIDraft.ViewModels;

public sealed partial class SupportChatViewModel : ObservableObject
{
    private const string InitialPlaceholder =
        "Select a customer message to begin.";

    private const string GenerationFailurePlaceholder =
        "Unable to generate a reply.";

    private const string ReviewAndEditMessage =
        "Review and edit before sending.";

    private const string GenerationFailureMessage =
        "We couldn’t generate a reply. Please try again.";

    private const string RegenerationFailureMessage =
        "We couldn’t regenerate the reply. Your previous draft has been preserved. Please try again.";

    private const string MissingCredentialsMessage =
        "Azure OpenAI credentials are unavailable or invalid. Configure the Azure settings and retry.";

    private const string InvalidAiResponseMessage =
        "We couldn’t process the generated reply. Please try again.";

    private const string DraftReplySuggestion = "Draft Reply";
    private const string RegenerateSuggestion = "Regenerate";
    private const string DiscardSuggestion = "Discard";
    private const string RetrySuggestion = "Retry";

    private static readonly ImageSource RegenerateSuggestionImage =
        ImageSource.FromFile("support_reply_regenerate.svg");

    private static readonly ImageSource RetrySuggestionImage =
        ImageSource.FromFile("support_reply_retry.svg");

    private static readonly ImageSource DiscardSuggestionImage =
        ImageSource.FromFile("support_reply_discard.svg");

    private readonly ConversationService _conversationService;
    private readonly ISupportReplyService _supportReplyService;
    private readonly AsyncRelayCommand<TextMessage?> _selectMessageCommand;
    private readonly AsyncRelayCommand<TextMessage?> _draftReplyCommand;
    private readonly Command<object?> _nativeSendCommand;

    private CancellationTokenSource? _generationCancellationTokenSource;
    private int _generationVersion;
    private string _lastGeneratedDraft = string.Empty;
    private string _lastStableDraftText = string.Empty;
    private WorkflowState _workflowState = WorkflowState.Initial;
    private bool _isHandlingSuggestionAction;

    public SupportChatViewModel(
        ConversationService conversationService,
        ISupportReplyService supportReplyService)
    {
        _conversationService = conversationService;
        _supportReplyService = supportReplyService;

        CurrentUser = _conversationService.SupportAuthor;
        Messages = _conversationService.CreateConversation();

        TypingIndicator = new ChatTypingIndicator
        {
            Text = "Drafting reply..."
        };

        TypingIndicator.Authors.Add(
            _conversationService.SupportAuthor);

        _selectMessageCommand =
            new AsyncRelayCommand<TextMessage?>(
                SelectMessageAsync);

        SelectMessageCommand = _selectMessageCommand;

        _draftReplyCommand =
            new AsyncRelayCommand<TextMessage?>(
                DraftReplyAsync,
                CanActivateDraftReply);

        DraftReplyCommand = _draftReplyCommand;

        _nativeSendCommand =
            new Command<object?>(
                SendNativeMessage,
                _ => CanSendMessage);

        NativeSendCommand = _nativeSendCommand;

        TransitionTo(WorkflowState.Initial);
    }

    public ObservableCollection<object> Messages { get; }

    public Author CurrentUser { get; }

    public ChatTypingIndicator TypingIndicator { get; }

    [ObservableProperty]
    private TextMessage? selectedMessage;

    [ObservableProperty]
    private ChatSuggestions? chatSuggestions;

    [ObservableProperty]
    private bool showTypingIndicator;

    [ObservableProperty]
    private bool isComposerEnabled;

    [ObservableProperty]
    private bool isComposerReadOnly;

    [ObservableProperty]
    private string composerPlaceholder = InitialPlaceholder;

    [ObservableProperty]
    private string editorText = string.Empty;

    [ObservableProperty]
    private bool isDrafting;

    [ObservableProperty]
    private bool hasDraft;

    [ObservableProperty]
    private bool showReviewHint;

    [ObservableProperty]
    private bool showErrorMessage;

    [ObservableProperty]
    private string inlineMessage = string.Empty;

    [ObservableProperty]
    private string progressMessage = string.Empty;

    public IAsyncRelayCommand<TextMessage?> SelectMessageCommand { get; }

    public IAsyncRelayCommand<TextMessage?> DraftReplyCommand { get; }

    public ICommand NativeSendCommand { get; }

    public Func<Task<bool>>? ConfirmRegenerateAsync { get; set; }

    public Func<Task<bool>>? ConfirmSelectionChangeAsync { get; set; }

    public bool CanDraftReply =>
        _workflowState == WorkflowState.Selected
        && SelectedMessage is not null;

    public bool IsDraftReplyVisible =>
        _workflowState == WorkflowState.Selected
        && SelectedMessage is not null;

    public string DraftReplyButtonText =>
        "✦ Draft Reply";

    public bool CanSendMessage =>
        IsComposerEnabled
        && !IsComposerReadOnly
        && !IsDrafting
        && !string.IsNullOrWhiteSpace(EditorText);

    public event Action<TextMessage>? OutgoingMessageSent;

    partial void OnSelectedMessageChanged(
        TextMessage? value)
    {
        if (value is null)
        {
            TransitionTo(WorkflowState.Initial);
        }
        else if (_workflowState is
                 WorkflowState.Initial
                 or WorkflowState.Sent
                 or WorkflowState.Discarded)
        {
            TransitionTo(WorkflowState.Selected);
        }

        NotifyStateBindings();
    }

    partial void OnIsComposerEnabledChanged(
        bool value)
    {
        NotifySendBindings();
    }

    partial void OnIsComposerReadOnlyChanged(
        bool value)
    {
        NotifySendBindings();
    }

    partial void OnEditorTextChanged(
        string value)
    {
        NotifySendBindings();
    }

    public void BeginSuggestionAction()
    {
        _isHandlingSuggestionAction = true;
    }

    public void EndSuggestionAction()
    {
        _isHandlingSuggestionAction = false;
    }

    public void UpdateEditorText(
        string? text)
    {
        if (_isHandlingSuggestionAction
            || !IsComposerEnabled
            || IsComposerReadOnly)
        {
            return;
        }

        var updatedText =
            text ?? string.Empty;

        if (ShouldIgnoreWorkflowActionEditorEcho(
                updatedText))
        {
            return;
        }

        EditorText = updatedText;

        if (HasDraft)
        {
            _lastStableDraftText = EditorText;
        }
    }

    public async Task HandleSuggestionActionAsync(
        string? text)
    {
        if (string.IsNullOrWhiteSpace(text)
            || IsDrafting)
        {
            return;
        }

        _isHandlingSuggestionAction = true;

        try
        {
            var action =
                NormalizeWorkflowActionText(text);

            if (action.Equals(
                    DiscardSuggestion,
                    StringComparison.OrdinalIgnoreCase))
            {
                _generationCancellationTokenSource?.Cancel();

                _lastGeneratedDraft = string.Empty;
                _lastStableDraftText = string.Empty;

                /*
                 * Clearing SelectedMessage invokes
                 * OnSelectedMessageChanged and restores the initial
                 * composer and customer-message appearance.
                 */
                SelectedMessage = null;

                return;
            }

            if (action.Equals(
                    RegenerateSuggestion,
                    StringComparison.OrdinalIgnoreCase))
            {
                if (SelectedMessage is null
                    || !HasDraft)
                {
                    return;
                }

                if (HasDraftBeenEdited())
                {
                    var confirmed =
                        ConfirmRegenerateAsync is null
                        || await ConfirmRegenerateAsync();

                    if (!confirmed)
                    {
                        return;
                    }
                }

                await GenerateDraftAsync(
                    GenerationOperation.Regenerate,
                    preserveExistingDraftOnFailure: true);

                return;
            }

            if (action.Equals(
                    RetrySuggestion,
                    StringComparison.OrdinalIgnoreCase))
            {
                if (SelectedMessage is null)
                {
                    return;
                }

                await GenerateDraftAsync(
                    HasDraft
                        ? GenerationOperation.Regenerate
                        : GenerationOperation.Retry,
                    preserveExistingDraftOnFailure: HasDraft);
            }
        }
        finally
        {
            _isHandlingSuggestionAction = false;
        }
    }

    private async Task SelectMessageAsync(
        TextMessage? message)
    {
        if (message is null
            || !_conversationService.IsCustomerMessage(message)
            || IsDrafting)
        {
            return;
        }

        /*
         * Tapping the selected customer message again toggles
         * the selection off and restores the default state.
         */
        if (ReferenceEquals(
                SelectedMessage,
                message))
        {
            _generationCancellationTokenSource?.Cancel();

            _lastGeneratedDraft = string.Empty;
            _lastStableDraftText = string.Empty;

            SelectedMessage = null;

            return;
        }

        if (HasDraft)
        {
            var confirmed =
                ConfirmSelectionChangeAsync is null
                || await ConfirmSelectionChangeAsync();

            if (!confirmed)
            {
                return;
            }

            _lastGeneratedDraft = string.Empty;
            _lastStableDraftText = string.Empty;

            TransitionTo(WorkflowState.Discarded);
        }

        SelectedMessage = message;

        /*
         * OnSelectedMessageChanged normally transitions to Selected.
         * Keep this explicit transition for selection changes that
         * follow a non-initial workflow state.
         */
        TransitionTo(WorkflowState.Selected);
    }

    private bool CanActivateDraftReply(
        TextMessage? message)
    {
        return _workflowState == WorkflowState.Selected
            && SelectedMessage is not null
            && message is not null
            && ReferenceEquals(
                message,
                SelectedMessage)
            && _conversationService.IsCustomerMessage(message)
            && !IsDrafting
            && !HasDraft;
    }

    private async Task DraftReplyAsync(
        TextMessage? message)
    {
        if (!CanActivateDraftReply(message))
        {
            return;
        }

        await GenerateDraftAsync(
            GenerationOperation.Initial,
            preserveExistingDraftOnFailure: false);
    }

    private async Task GenerateDraftAsync(
        GenerationOperation operation,
        bool preserveExistingDraftOnFailure)
    {
        if (SelectedMessage is null
            || IsDrafting)
        {
            return;
        }

        var preservedDraft =
            preserveExistingDraftOnFailure
                ? GetDraftPreservationSnapshot()
                : string.Empty;

        _generationCancellationTokenSource?.Cancel();

        _generationCancellationTokenSource =
            new CancellationTokenSource();

        var cancellationToken =
            _generationCancellationTokenSource.Token;

        var generationVersion =
            ++_generationVersion;

        TransitionTo(
            operation switch
            {
                GenerationOperation.Regenerate =>
                    WorkflowState.Regenerating,

                GenerationOperation.Retry =>
                    WorkflowState.Retrying,

                _ =>
                    WorkflowState.Drafting
            },
            preservedDraft: preservedDraft);

        var request =
            new SupportReplyRequest(
                CreateMessageId(SelectedMessage),
                SelectedMessage.Text ?? string.Empty,
                _conversationService.BuildRecentContext(
                    Messages,
                    SelectedMessage));

        try
        {
            var validation =
                await _supportReplyService.DraftReplyAsync(
                    request,
                    cancellationToken);

            if (generationVersion
                != _generationVersion)
            {
                return;
            }

            if (validation.IsValid
                && validation.Response is not null)
            {
                TransitionTo(
                    WorkflowState.Generated,
                    generatedReply:
                        validation.Response.Reply);

                return;
            }

            TransitionTo(
                WorkflowState.Error,
                preserveDraft:
                    preserveExistingDraftOnFailure,
                preservedDraft:
                    preservedDraft,
                errorMessage:
                    validation.ErrorMessage);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            /*
             * Cancellation is expected when the workflow is reset,
             * the selected message is cleared, or a request is replaced.
             */
        }
        catch
        {
            if (generationVersion
                != _generationVersion)
            {
                return;
            }

            TransitionTo(
                WorkflowState.Error,
                preserveDraft:
                    preserveExistingDraftOnFailure,
                preservedDraft:
                    preservedDraft,
                errorMessage:
                    InvalidAiResponseMessage);
        }
    }

    private void SendNativeMessage(
        object? args)
    {
        if (args is not SendMessageEventArgs sendArgs)
        {
            return;
        }

        sendArgs.Handled = true;

        if (_isHandlingSuggestionAction
            || IsDrafting
            || !IsComposerEnabled
            || IsComposerReadOnly)
        {
            return;
        }

        var submittedText =
            sendArgs.Message?.Text?.Trim()
            ?? string.Empty;

        if (submittedText.Length == 0
            || IsWorkflowActionText(submittedText))
        {
            return;
        }

        var currentEditorText =
            EditorText.Trim();

        if (!string.Equals(
                submittedText,
                currentEditorText,
                StringComparison.Ordinal))
        {
            return;
        }

        var outgoingMessage =
            new TextMessage
            {
                Author =
                    _conversationService.SupportAuthor,

                DateTime =
                    DateTime.Now,

                Text =
                    submittedText
            };

        Messages.Add(outgoingMessage);

        OutgoingMessageSent?.Invoke(
            outgoingMessage);

        SelectedMessage = null;

        _lastGeneratedDraft = string.Empty;
        _lastStableDraftText = string.Empty;

        TransitionTo(WorkflowState.Sent);
    }

    private bool HasDraftBeenEdited()
    {
        return HasDraft
            && !string.Equals(
                EditorText.Trim(),
                _lastGeneratedDraft.Trim(),
                StringComparison.Ordinal);
    }

    private void TransitionTo(
        WorkflowState state,
        string? generatedReply = null,
        bool preserveDraft = false,
        string preservedDraft = "",
        string? errorMessage = null)
    {
        _workflowState = state;

        switch (state)
        {
            case WorkflowState.Initial:
            case WorkflowState.Sent:
                ResetGenerationState();

                HasDraft = false;
                ShowReviewHint = false;
                ShowErrorMessage = false;
                InlineMessage = string.Empty;
                ChatSuggestions = null;
                IsComposerEnabled = false;
                IsComposerReadOnly = true;
                ComposerPlaceholder = InitialPlaceholder;
                EditorText = string.Empty;
                _lastStableDraftText = string.Empty;

                break;

            case WorkflowState.Selected:
            case WorkflowState.Discarded:
                ResetGenerationState();

                HasDraft = false;
                ShowReviewHint = false;
                ShowErrorMessage = false;
                InlineMessage = string.Empty;
                ChatSuggestions = null;
                IsComposerEnabled = false;
                IsComposerReadOnly = true;
                ComposerPlaceholder = InitialPlaceholder;
                EditorText = string.Empty;
                _lastStableDraftText = string.Empty;

                break;

            case WorkflowState.Drafting:
                StartGenerationState(
                    "Drafting reply...");

                HasDraft = false;
                IsComposerEnabled = false;
                IsComposerReadOnly = true;
                ComposerPlaceholder = InitialPlaceholder;
                EditorText = string.Empty;

                break;

            case WorkflowState.Retrying:
                StartGenerationState(
                    "Retrying draft...");

                HasDraft = false;
                IsComposerEnabled = false;
                IsComposerReadOnly = true;
                ComposerPlaceholder = InitialPlaceholder;
                EditorText = string.Empty;

                break;

            case WorkflowState.Regenerating:
                StartGenerationState(
                    "Regenerating reply...");

                HasDraft = true;
                IsComposerEnabled = true;
                IsComposerReadOnly = true;
                ComposerPlaceholder = string.Empty;
                EditorText = preservedDraft;
                _lastStableDraftText = preservedDraft;

                break;

            case WorkflowState.Generated:
                ResetGenerationState();

                HasDraft = true;
                ShowReviewHint = true;
                ShowErrorMessage = false;
                InlineMessage = ReviewAndEditMessage;

                ChatSuggestions =
                    BuildSuggestions(
                        RegenerateSuggestion,
                        DiscardSuggestion);

                IsComposerEnabled = true;
                IsComposerReadOnly = false;
                ComposerPlaceholder = string.Empty;
                EditorText =
                    generatedReply ?? string.Empty;

                _lastGeneratedDraft = EditorText;
                _lastStableDraftText = EditorText;

                break;

            case WorkflowState.Error:
                ResetGenerationState();

                ShowErrorMessage = true;
                ShowReviewHint = false;

                InlineMessage =
                    CreateUserFacingError(
                        errorMessage,
                        preserveDraft);

                if (preserveDraft
                    && !string.IsNullOrWhiteSpace(
                        preservedDraft))
                {
                    HasDraft = true;

                    ChatSuggestions =
                        BuildSuggestions(
                            RegenerateSuggestion,
                            DiscardSuggestion);

                    IsComposerEnabled = true;
                    IsComposerReadOnly = false;
                    ComposerPlaceholder = string.Empty;
                    EditorText = preservedDraft;
                    _lastStableDraftText = EditorText;

                    break;
                }

                HasDraft = false;

                ChatSuggestions =
                    BuildSuggestions(
                        RetrySuggestion);

                IsComposerEnabled = false;
                IsComposerReadOnly = true;
                ComposerPlaceholder =
                    GenerationFailurePlaceholder;
                EditorText = string.Empty;
                _lastStableDraftText = string.Empty;

                break;
        }

        NotifyStateBindings();
    }

    private void StartGenerationState(
        string message)
    {
        IsDrafting = true;

        /*
         * The selected customer message remains the single
         * progress location. Disable the native SfChat typing
         * indicator to avoid duplicate progress displays.
         */
        ShowTypingIndicator = false;

        ProgressMessage = message;
        ShowReviewHint = false;
        ShowErrorMessage = false;
        InlineMessage = string.Empty;
        ChatSuggestions = null;
    }

    private void ResetGenerationState()
    {
        IsDrafting = false;
        ShowTypingIndicator = false;
        ProgressMessage = string.Empty;
    }

    private string CreateUserFacingError(
        string? errorMessage,
        bool preservedDraft)
    {
        if (preservedDraft)
        {
            return RegenerationFailureMessage;
        }

        if (string.IsNullOrWhiteSpace(errorMessage)
            || errorMessage.Equals(
                "The AI service is currently unavailable.",
                StringComparison.OrdinalIgnoreCase)
            || errorMessage.Contains(
                "valid JSON",
                StringComparison.OrdinalIgnoreCase)
            || errorMessage.Contains(
                "process the generated reply",
                StringComparison.OrdinalIgnoreCase))
        {
            return GenerationFailureMessage;
        }

        if (errorMessage.Contains(
                "credential",
                StringComparison.OrdinalIgnoreCase)
            || errorMessage.Contains(
                "authentication",
                StringComparison.OrdinalIgnoreCase)
            || errorMessage.Contains(
                "endpoint",
                StringComparison.OrdinalIgnoreCase)
            || errorMessage.Contains(
                "deployment",
                StringComparison.OrdinalIgnoreCase))
        {
            return MissingCredentialsMessage;
        }

        return errorMessage;
    }

    private void NotifyStateBindings()
    {
        OnPropertyChanged(
            nameof(CanDraftReply));

        OnPropertyChanged(
            nameof(IsDraftReplyVisible));

        _draftReplyCommand.NotifyCanExecuteChanged();

        NotifySendBindings();
    }

    private void NotifySendBindings()
    {
        OnPropertyChanged(
            nameof(CanSendMessage));

        _nativeSendCommand.ChangeCanExecute();
    }

    private bool ShouldIgnoreWorkflowActionEditorEcho(
        string text)
    {
        return IsWorkflowActionText(
            NormalizeWorkflowActionText(text));
    }

    private string GetDraftPreservationSnapshot()
    {
        if (!string.IsNullOrWhiteSpace(
                _lastStableDraftText))
        {
            return _lastStableDraftText;
        }

        return EditorText;
    }

    private static ChatSuggestions BuildSuggestions(
        params string[] items)
    {
        return new ChatSuggestions
        {
            Items =
                new ObservableCollection<ISuggestion>(
                    items.Select(
                        item =>
                            new Suggestion(
                                item,
                                GetSuggestionImage(item))))
        };
    }

    private static ImageSource GetSuggestionImage(
        string suggestionText)
    {
        var normalizedSuggestion =
            NormalizeWorkflowActionText(suggestionText);

        if (normalizedSuggestion.Equals(
                DiscardSuggestion,
                StringComparison.OrdinalIgnoreCase))
        {
            return DiscardSuggestionImage;
        }

        if (normalizedSuggestion.Equals(
                RetrySuggestion,
                StringComparison.OrdinalIgnoreCase))
        {
            return RetrySuggestionImage;
        }

        return RegenerateSuggestionImage;
    }

    private static bool IsWorkflowActionText(
        string text)
    {
        var normalized =
            NormalizeWorkflowActionText(text);

        return normalized.Equals(
                   DraftReplySuggestion,
                   StringComparison.OrdinalIgnoreCase)
            || normalized.Equals(
                   RegenerateSuggestion,
                   StringComparison.OrdinalIgnoreCase)
            || normalized.Equals(
                   DiscardSuggestion,
                   StringComparison.OrdinalIgnoreCase)
            || normalized.Equals(
                   RetrySuggestion,
                   StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeWorkflowActionText(
        string text)
    {
        var normalized =
            text.Trim();

        if (normalized.StartsWith(
                "✦",
                StringComparison.Ordinal))
        {
            normalized =
                normalized[1..].TrimStart();
        }

        return normalized;
    }

    private static string CreateMessageId(
        TextMessage message)
    {
        return
            $"{message.DateTime:HHmmss}-" +
            $"{message.Author?.Name}-" +
            $"{message.Text}";
    }

    private enum GenerationOperation
    {
        Initial,
        Retry,
        Regenerate
    }

    private enum WorkflowState
    {
        Initial,
        Selected,
        Drafting,
        Retrying,
        Regenerating,
        Generated,
        Error,
        Discarded,
        Sent
    }
}