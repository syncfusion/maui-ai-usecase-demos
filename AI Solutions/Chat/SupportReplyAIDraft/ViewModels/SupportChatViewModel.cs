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
    private const string RegenerateSuggestion = "Regenerate";
    private const string DiscardSuggestion = "Discard";
    private const string RetrySuggestion = "Retry";

    private readonly ConversationService _conversationService;
    private readonly ISupportReplyService _supportReplyService;
    private readonly AsyncRelayCommand<TextMessage?> _draftReplyCommand;
    private readonly AsyncRelayCommand<object?> _suggestionItemSelectedCommand;
    private readonly Command<object?> _nativeSendCommand;

    private CancellationTokenSource? _generationCancellationTokenSource;
    private int _generationVersion;
    private string _lastGeneratedDraft = string.Empty;

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

        SelectMessageCommand = new Command<TextMessage>(SelectMessage);

        _draftReplyCommand = new AsyncRelayCommand<TextMessage?>(DraftReplyAsync, CanActivateDraftReply);
        DraftReplyCommand = _draftReplyCommand;

        _suggestionItemSelectedCommand = new AsyncRelayCommand<object?>(HandleSuggestionSelectionAsync);
        SuggestionItemSelectedCommand = _suggestionItemSelectedCommand;

        _nativeSendCommand = new Command<object?>(SendNativeMessage, _ => CanSendMessage);
        NativeSendCommand = _nativeSendCommand;

        ApplyInitialState();
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

    public ICommand SelectMessageCommand { get; }
    public IAsyncRelayCommand<TextMessage?> DraftReplyCommand { get; }
    public IAsyncRelayCommand<object?> SuggestionItemSelectedCommand { get; }
    public ICommand NativeSendCommand { get; }

    public Func<Task<bool>>? ConfirmRegenerateAsync { get; set; }

    public bool CanDraftReply => CanActivateDraftReply(SelectedMessage);

    public bool IsDraftReplyVisible => SelectedMessage is not null && !ShowErrorMessage && (!HasDraft || IsDrafting);

    public string DraftReplyButtonText => IsDrafting ? DraftingPlaceholder : "✦ Draft Reply";

    public bool CanSendMessage => IsComposerEnabled && !IsComposerReadOnly && !string.IsNullOrWhiteSpace(EditorText);

    public event Action<TextMessage>? OutgoingMessageSent;

    partial void OnSelectedMessageChanged(TextMessage? value)
    {
        if (value is null)
        {
            ApplyInitialState();
        }
        else if (!IsDrafting && !HasDraft)
        {
            ApplySelectedState();
        }

        OnPropertyChanged(nameof(CanDraftReply));
        OnPropertyChanged(nameof(IsDraftReplyVisible));
        _draftReplyCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsDraftingChanged(bool value)
    {
        OnPropertyChanged(nameof(DraftReplyButtonText));
        OnPropertyChanged(nameof(CanDraftReply));
        OnPropertyChanged(nameof(IsDraftReplyVisible));
        _draftReplyCommand.NotifyCanExecuteChanged();
    }

    partial void OnHasDraftChanged(bool value)
    {
        OnPropertyChanged(nameof(IsDraftReplyVisible));
    }

    partial void OnShowErrorMessageChanged(bool value)
    {
        OnPropertyChanged(nameof(IsDraftReplyVisible));
    }

    partial void OnIsComposerEnabledChanged(bool value)
    {
        OnPropertyChanged(nameof(CanSendMessage));
        _nativeSendCommand.ChangeCanExecute();
    }

    partial void OnIsComposerReadOnlyChanged(bool value)
    {
        OnPropertyChanged(nameof(CanSendMessage));
        _nativeSendCommand.ChangeCanExecute();
    }

    partial void OnEditorTextChanged(string value)
    {
        OnPropertyChanged(nameof(CanSendMessage));
        _nativeSendCommand.ChangeCanExecute();
    }

    public void UpdateEditorText(string? text)
    {
        if (!IsComposerEnabled || IsComposerReadOnly)
        {
            return;
        }

        EditorText = text ?? string.Empty;
    }

    private void SelectMessage(TextMessage? message)
    {
        if (message is null || !_conversationService.IsCustomerMessage(message) || IsDrafting)
        {
            return;
        }

        var selectionChanged = !ReferenceEquals(SelectedMessage, message);
        SelectedMessage = message;

        if (selectionChanged && HasDraft)
        {
            DiscardDraft();
        }
    }

    private bool CanActivateDraftReply(TextMessage? message)
    {
        return SelectedMessage is not null
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

    private async Task HandleSuggestionSelectionAsync(object? args)
    {
        if (args is not SuggestionItemSelectedEventArgs suggestionArgs)
        {
            return;
        }

        suggestionArgs.CancelSendMessage = true;
        suggestionArgs.HideAfterSelection = false;

        var text = (suggestionArgs.SelectedItem as Suggestion)?.Text;
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        if (text.Equals(DiscardSuggestion, StringComparison.OrdinalIgnoreCase))
        {
            DiscardDraft();
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

    private async Task GenerateDraftAsync(bool preserveExistingDraftOnFailure)
    {
        if (SelectedMessage is null)
        {
            return;
        }

        var preservedDraft = preserveExistingDraftOnFailure ? EditorText : string.Empty;

        _generationCancellationTokenSource?.Cancel();
        _generationCancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _generationCancellationTokenSource.Token;
        var generationVersion = ++_generationVersion;

        ApplyDraftingState();

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
                ApplyGeneratedState(validation.Response.Reply);
                return;
            }

            ApplyGenerationFailureState(preserveExistingDraftOnFailure, preservedDraft);
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

            ApplyGenerationFailureState(preserveExistingDraftOnFailure, preservedDraft);
        }
    }

    private void SendNativeMessage(object? args)
    {
        if (args is SendMessageEventArgs sendArgs)
        {
            sendArgs.Handled = true;
        }

        if (!CanSendMessage)
        {
            return;
        }

        var text = EditorText.Trim();
        if (text.Length == 0)
        {
            return;
        }

        var outgoingMessage = new TextMessage
        {
            Author = _conversationService.SupportAuthor,
            DateTime = DateTime.Now,
            Text = text
        };

        Messages.Add(outgoingMessage);
        OutgoingMessageSent?.Invoke(outgoingMessage);

        SelectedMessage = null;
        _lastGeneratedDraft = string.Empty;
        ApplyInitialState();
    }

    private void DiscardDraft()
    {
        _lastGeneratedDraft = string.Empty;

        if (SelectedMessage is null)
        {
            ApplyInitialState();
            return;
        }

        ApplySelectedState();
    }

    private bool HasDraftBeenEdited()
    {
        return HasDraft && !string.Equals(EditorText.Trim(), _lastGeneratedDraft.Trim(), StringComparison.Ordinal);
    }

    private void ApplyInitialState()
    {
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
    }

    private void ApplySelectedState()
    {
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
    }

    private void ApplyDraftingState()
    {
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
    }

    private void ApplyGeneratedState(string reply)
    {
        IsDrafting = false;
        HasDraft = true;
        ShowTypingIndicator = false;
        ShowReviewHint = true;
        ShowErrorMessage = false;
        InlineMessage = ReviewAndEditMessage;
        ChatSuggestions = BuildSuggestions(RegenerateSuggestion, DiscardSuggestion);
        IsComposerEnabled = true;
        IsComposerReadOnly = false;
        ComposerPlaceholder = string.Empty;
        EditorText = reply;
        _lastGeneratedDraft = reply;
    }

    private void ApplyGenerationFailureState(bool preserveExistingDraftOnFailure, string preservedDraft)
    {
        IsDrafting = false;
        ShowTypingIndicator = false;
        ShowErrorMessage = true;
        InlineMessage = GenerationFailureMessage;
        ChatSuggestions = BuildSuggestions(RetrySuggestion);

        if (preserveExistingDraftOnFailure && !string.IsNullOrWhiteSpace(preservedDraft))
        {
            HasDraft = true;
            ShowReviewHint = false;
            IsComposerEnabled = true;
            IsComposerReadOnly = false;
            ComposerPlaceholder = string.Empty;
            EditorText = preservedDraft;
            return;
        }

        HasDraft = false;
        ShowReviewHint = false;
        IsComposerEnabled = false;
        IsComposerReadOnly = true;
        ComposerPlaceholder = GenerationFailurePlaceholder;
        EditorText = string.Empty;
    }

    private static ChatSuggestions BuildSuggestions(params string[] items)
    {
        return new ChatSuggestions
        {
            Items = new ObservableCollection<ISuggestion>(items.Select(text => new Suggestion(text)))
        };
    }

    private static string CreateMessageId(TextMessage message)
    {
        return $"{message.DateTime:HHmmss}-{message.Author?.Name}-{message.Text}";
    }
}
