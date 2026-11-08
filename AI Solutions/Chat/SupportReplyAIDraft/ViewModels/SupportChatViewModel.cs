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
    private const string InitialPlaceholder = "Select a customer message to begin.";
    private const string DraftingPlaceholder = "Drafting reply...";
    private const string GenerationFailurePlaceholder = "Unable to generate a reply.";
    private const string ReviewAndEditMessage = "Review and edit before sending.";
    private const string GenerationFailureMessage = "We couldn’t generate a reply. Please try again.";
    private const string DraftReplySuggestion = "Draft Reply";
    private const string RegenerateSuggestion = "Regenerate";
    private const string DiscardSuggestion = "Discard";
    private const string RetrySuggestion = "Retry";

    private static readonly ImageSource RefreshSuggestionImage = CreateSuggestionImage("↻", "#004ac6");
    private static readonly ImageSource DiscardSuggestionImage = CreateSuggestionImage("✕", "#737686");

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

    public SupportChatViewModel(ConversationService conversationService, ISupportReplyService supportReplyService)
    {
        _conversationService = conversationService;
        _supportReplyService = supportReplyService;

        CurrentUser = _conversationService.SupportAuthor;
        Messages = _conversationService.CreateConversation();

        TypingIndicator = new ChatTypingIndicator
        {
            Text = DraftingPlaceholder
        };
        TypingIndicator.Authors.Add(_conversationService.SupportAuthor);

        _selectMessageCommand = new AsyncRelayCommand<TextMessage?>(SelectMessageAsync);
        SelectMessageCommand = _selectMessageCommand;

        _draftReplyCommand = new AsyncRelayCommand<TextMessage?>(DraftReplyAsync, CanActivateDraftReply);
        DraftReplyCommand = _draftReplyCommand;

        _nativeSendCommand = new Command<object?>(SendNativeMessage, _ => CanSendMessage);
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

    public IAsyncRelayCommand<TextMessage?> SelectMessageCommand { get; }
    public IAsyncRelayCommand<TextMessage?> DraftReplyCommand { get; }
    public ICommand NativeSendCommand { get; }

    public Func<Task<bool>>? ConfirmRegenerateAsync { get; set; }
    public Func<Task<bool>>? ConfirmSelectionChangeAsync { get; set; }

    public bool CanDraftReply => _workflowState == WorkflowState.Selected && SelectedMessage is not null;

    public bool IsDraftReplyVisible => _workflowState == WorkflowState.Selected && SelectedMessage is not null;

    public string DraftReplyButtonText => "✦ Draft Reply";

    public bool CanSendMessage => IsComposerEnabled && !IsComposerReadOnly && !string.IsNullOrWhiteSpace(EditorText);

    public event Action<TextMessage>? OutgoingMessageSent;

    partial void OnSelectedMessageChanged(TextMessage? value)
    {
        if (value is null)
        {
            TransitionTo(WorkflowState.Initial);
        }
        else if (_workflowState == WorkflowState.Initial || _workflowState == WorkflowState.Sent || _workflowState == WorkflowState.Discarded)
        {
            TransitionTo(WorkflowState.Selected);
        }

        NotifyStateBindings();
    }

    partial void OnIsComposerEnabledChanged(bool value)
    {
        NotifySendBindings();
    }

    partial void OnIsComposerReadOnlyChanged(bool value)
    {
        NotifySendBindings();
    }

    partial void OnEditorTextChanged(string value)
    {
        NotifySendBindings();
    }

    public void UpdateEditorText(string? text)
    {
        if (_isHandlingSuggestionAction || !IsComposerEnabled || IsComposerReadOnly)
        {
            return;
        }

        var updatedText = text ?? string.Empty;
        if (ShouldIgnoreWorkflowActionEditorEcho(updatedText))
        {
            return;
        }

        EditorText = updatedText;

        if (HasDraft)
        {
            _lastStableDraftText = EditorText;
        }
    }

    public async Task HandleSuggestionActionAsync(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        _isHandlingSuggestionAction = true;
        try
        {
            if (text.Equals(DiscardSuggestion, StringComparison.OrdinalIgnoreCase))
            {
                TransitionTo(WorkflowState.Discarded);
                return;
            }

            if (text.Equals(RegenerateSuggestion, StringComparison.OrdinalIgnoreCase))
            {
                if (SelectedMessage is null)
                {
                    return;
                }

                if (HasDraft && HasDraftBeenEdited())
                {
                    var confirmed = ConfirmRegenerateAsync is null || await ConfirmRegenerateAsync();
                    if (!confirmed)
                    {
                        return;
                    }
                }

                await GenerateDraftAsync(preserveExistingDraftOnFailure: HasDraft);
                return;
            }

            if (text.Equals(RetrySuggestion, StringComparison.OrdinalIgnoreCase))
            {
                await GenerateDraftAsync(preserveExistingDraftOnFailure: HasDraft);
            }
        }
        finally
        {
            _isHandlingSuggestionAction = false;
        }
    }

    private async Task SelectMessageAsync(TextMessage? message)
    {
        if (message is null || !_conversationService.IsCustomerMessage(message) || IsDrafting)
        {
            return;
        }

        if (ReferenceEquals(SelectedMessage, message))
        {
            return;
        }

        if (HasDraft)
        {
            var confirmed = ConfirmSelectionChangeAsync is null || await ConfirmSelectionChangeAsync();
            if (!confirmed)
            {
                return;
            }

            _lastGeneratedDraft = string.Empty;
            TransitionTo(WorkflowState.Discarded);
        }

        SelectedMessage = message;
        TransitionTo(WorkflowState.Selected);
    }

    private bool CanActivateDraftReply(TextMessage? message)
    {
        return _workflowState == WorkflowState.Selected
            && SelectedMessage is not null
            && message is not null
            && ReferenceEquals(message, SelectedMessage)
            && _conversationService.IsCustomerMessage(message)
            && !IsDrafting
            && !HasDraft;
    }

    private async Task DraftReplyAsync(TextMessage? message)
    {
        if (!CanActivateDraftReply(message))
        {
            return;
        }

        await GenerateDraftAsync(preserveExistingDraftOnFailure: false);
    }

    private async Task GenerateDraftAsync(bool preserveExistingDraftOnFailure)
    {
        if (SelectedMessage is null)
        {
            return;
        }

        var preservedDraft = preserveExistingDraftOnFailure
            ? GetDraftPreservationSnapshot()
            : string.Empty;

        _generationCancellationTokenSource?.Cancel();
        _generationCancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _generationCancellationTokenSource.Token;
        var generationVersion = ++_generationVersion;

        TransitionTo(WorkflowState.Drafting);

        var request = new SupportReplyRequest(
            CreateMessageId(SelectedMessage),
            SelectedMessage.Text ?? string.Empty,
            _conversationService.BuildRecentContext(Messages, SelectedMessage));

        try
        {
            var validation = await _supportReplyService.DraftReplyAsync(request, cancellationToken);

            if (generationVersion != _generationVersion)
            {
                return;
            }

            if (validation.IsValid && validation.Response is not null)
            {
                TransitionTo(WorkflowState.Generated, generatedReply: validation.Response.Reply);
                return;
            }

            TransitionTo(WorkflowState.Error, preserveDraft: preserveExistingDraftOnFailure, preservedDraft: preservedDraft);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Cancelled request is intentionally ignored.
        }
        catch
        {
            if (generationVersion != _generationVersion)
            {
                return;
            }

            TransitionTo(WorkflowState.Error, preserveDraft: preserveExistingDraftOnFailure, preservedDraft: preservedDraft);
        }
    }

    private void SendNativeMessage(object? args)
    {
        if (args is not SendMessageEventArgs sendArgs)
        {
            return;
        }

        sendArgs.Handled = true;

        if (_isHandlingSuggestionAction || !IsComposerEnabled || IsComposerReadOnly)
        {
            return;
        }

        var submittedText = sendArgs.Message?.Text?.Trim() ?? string.Empty;
        if (submittedText.Length == 0)
        {
            return;
        }

        if (IsWorkflowActionText(submittedText))
        {
            return;
        }

        var editorText = EditorText.Trim();
        if (!string.Equals(submittedText, editorText, StringComparison.Ordinal))
        {
            return;
        }

        var outgoingMessage = new TextMessage
        {
            Author = _conversationService.SupportAuthor,
            DateTime = DateTime.Now,
            Text = submittedText
        };

        Messages.Add(outgoingMessage);
        OutgoingMessageSent?.Invoke(outgoingMessage);

        SelectedMessage = null;
        _lastGeneratedDraft = string.Empty;
        TransitionTo(WorkflowState.Sent);
    }

    private bool HasDraftBeenEdited()
    {
        return HasDraft && !string.Equals(EditorText.Trim(), _lastGeneratedDraft.Trim(), StringComparison.Ordinal);
    }

    private void TransitionTo(
        WorkflowState state,
        string? generatedReply = null,
        bool preserveDraft = false,
        string preservedDraft = "")
    {
        _workflowState = state;

        switch (state)
        {
            case WorkflowState.Initial:
            case WorkflowState.Sent:
                _generationCancellationTokenSource?.Cancel();
                IsDrafting = false;
                HasDraft = false;
                ShowTypingIndicator = false;
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
                IsDrafting = false;
                HasDraft = false;
                ShowTypingIndicator = false;
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
                IsDrafting = true;
                HasDraft = false;
                ShowTypingIndicator = true;
                ShowReviewHint = false;
                ShowErrorMessage = false;
                InlineMessage = string.Empty;
                ChatSuggestions = null;
                IsComposerEnabled = false;
                IsComposerReadOnly = true;
                ComposerPlaceholder = DraftingPlaceholder;
                EditorText = string.Empty;
                break;

            case WorkflowState.Generated:
                IsDrafting = false;
                HasDraft = true;
                ShowTypingIndicator = false;
                ShowReviewHint = true;
                ShowErrorMessage = false;
                InlineMessage = ReviewAndEditMessage;
                ChatSuggestions = BuildSuggestions(
                    (RegenerateSuggestion, RefreshSuggestionImage),
                    (DiscardSuggestion, DiscardSuggestionImage));
                IsComposerEnabled = true;
                IsComposerReadOnly = false;
                ComposerPlaceholder = string.Empty;
                EditorText = generatedReply ?? string.Empty;
                _lastGeneratedDraft = EditorText;
                _lastStableDraftText = EditorText;
                break;

            case WorkflowState.Error:
                IsDrafting = false;
                ShowTypingIndicator = false;
                ShowErrorMessage = true;
                ShowReviewHint = false;
                InlineMessage = GenerationFailureMessage;
                ChatSuggestions = BuildSuggestions((RetrySuggestion, RefreshSuggestionImage));

                if (preserveDraft && !string.IsNullOrWhiteSpace(preservedDraft))
                {
                    HasDraft = true;
                    IsComposerEnabled = true;
                    IsComposerReadOnly = false;
                    ComposerPlaceholder = string.Empty;
                    EditorText = preservedDraft;
                    _lastStableDraftText = EditorText;
                    break;
                }

                HasDraft = false;
                IsComposerEnabled = false;
                IsComposerReadOnly = true;
                ComposerPlaceholder = GenerationFailurePlaceholder;
                EditorText = string.Empty;
                _lastStableDraftText = string.Empty;
                break;
        }

        NotifyStateBindings();
    }

    private void NotifyStateBindings()
    {
        OnPropertyChanged(nameof(CanDraftReply));
        OnPropertyChanged(nameof(IsDraftReplyVisible));
        _draftReplyCommand.NotifyCanExecuteChanged();
        NotifySendBindings();
    }

    private void NotifySendBindings()
    {
        OnPropertyChanged(nameof(CanSendMessage));
        _nativeSendCommand.ChangeCanExecute();
    }

    private bool ShouldIgnoreWorkflowActionEditorEcho(string text)
    {
        var trimmedText = text.Trim();
        return HasDraft
            && ChatSuggestions is not null
            && IsWorkflowActionText(trimmedText)
            && !string.Equals(_lastStableDraftText.Trim(), trimmedText, StringComparison.OrdinalIgnoreCase);
    }

    private string GetDraftPreservationSnapshot()
    {
        if (!string.IsNullOrWhiteSpace(_lastStableDraftText))
        {
            return _lastStableDraftText;
        }

        return EditorText;
    }

    private static ChatSuggestions BuildSuggestions(params (string Text, ImageSource Icon)[] items)
    {
        return new ChatSuggestions
        {
            Items = new ObservableCollection<ISuggestion>(items.Select(item => new Suggestion(item.Text, item.Icon)))
        };
    }

    private static bool IsWorkflowActionText(string text)
    {
        var normalized = NormalizeWorkflowActionText(text);
        return normalized.Equals(DraftReplySuggestion, StringComparison.OrdinalIgnoreCase)
            || normalized.Equals(RegenerateSuggestion, StringComparison.OrdinalIgnoreCase)
            || normalized.Equals(DiscardSuggestion, StringComparison.OrdinalIgnoreCase)
            || normalized.Equals(RetrySuggestion, StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeWorkflowActionText(string text)
    {
        var normalized = text.Trim();
        if (normalized.StartsWith("✦", StringComparison.Ordinal))
        {
            normalized = normalized[1..].TrimStart();
        }

        return normalized;
    }

    private static ImageSource CreateSuggestionImage(string glyph, string color)
    {
        return new FontImageSource
        {
            Glyph = glyph,
            Color = Color.FromArgb(color),
            Size = 14
        };
    }

    private static string CreateMessageId(TextMessage message)
    {
        return $"{message.DateTime:HHmmss}-{message.Author?.Name}-{message.Text}";
    }

    private enum WorkflowState
    {
        Initial,
        Selected,
        Drafting,
        Generated,
        Error,
        Discarded,
        Sent
    }
}
