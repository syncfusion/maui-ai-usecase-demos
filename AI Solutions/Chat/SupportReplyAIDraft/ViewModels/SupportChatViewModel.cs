using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using SupportReplyAIDraft.Models;
using SupportReplyAIDraft.Services;

namespace SupportReplyAIDraft.ViewModels;

public sealed partial class SupportChatViewModel : ObservableObject
{
    private readonly ConversationService _conversationService;
    private readonly ISupportReplyService _supportReplyService;
    private readonly SemaphoreSlim _generationGate = new(1, 1);
    private string _lastValidatedReply = string.Empty;
    private int _requestVersion;

    public SupportChatViewModel(ConversationService conversationService, ISupportReplyService supportReplyService)
    {
        _conversationService = conversationService;
        _supportReplyService = supportReplyService;

        Messages = _conversationService.CreateConversation();
        CurrentUser = new Syncfusion.Maui.Chat.Author { Name = "Jordan Lee" };

        SelectMessageCommand = new Command<SupportConversationMessage>(SelectMessage);
        DraftReplyCommand = new Command(async () => await DraftReplyAsync());
        RegenerateCommand = new Command(async () => await RegenerateAsync());
        DiscardCommand = new Command(DiscardDraft);
        RetryCommand = new Command(async () => await DraftReplyAsync());
        SendCommand = new Command(SendReply);
    }

    public ObservableCollection<SupportConversationMessage> Messages { get; }
    public Syncfusion.Maui.Chat.Author CurrentUser { get; }

    [ObservableProperty]
    private SupportConversationMessage? selectedMessage;

    [ObservableProperty]
    private string replyText = string.Empty;

    [ObservableProperty]
    private bool isDrafting;

    [ObservableProperty]
    private bool isGenerationError;

    [ObservableProperty]
    private bool isRegenerationError;

    [ObservableProperty]
    private string statusTitle = string.Empty;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private bool showStatus;

    [ObservableProperty]
    private bool showRequestedInformation;

    [ObservableProperty]
    private ObservableCollection<string> requestedInformation = new();

    [ObservableProperty]
    private ObservableCollection<string> warnings = new();

    [ObservableProperty]
    private bool hasValidatedDraft;

    [ObservableProperty]
    private bool hasUserEdits;

    private bool suppressReplyTracking;

    public ICommand SelectMessageCommand { get; }
    public ICommand DraftReplyCommand { get; }
    public ICommand RegenerateCommand { get; }
    public ICommand DiscardCommand { get; }
    public ICommand RetryCommand { get; }
    public ICommand SendCommand { get; }

    public bool CanDraftReply => SelectedMessage is { CanSelect: true } && !IsDrafting && string.IsNullOrWhiteSpace(ReplyText);
    public bool CanRegenerate => SelectedMessage is not null && HasValidatedDraft && !IsDrafting;
    public bool CanDiscard => SelectedMessage is not null && (HasValidatedDraft || !string.IsNullOrWhiteSpace(ReplyText) || ShowStatus);
    public bool CanSend => !IsDrafting && !string.IsNullOrWhiteSpace(ReplyText);
    public bool HasReply => !string.IsNullOrWhiteSpace(ReplyText);
    public bool IsComposerEnabled => HasValidatedDraft && !IsDrafting;
    public string ComposerHint => SelectedMessage is null
        ? "Choose a customer message to draft a reply."
        : IsDrafting
            ? "Drafting a structured reply from the selected message and recent context."
            : HasValidatedDraft
                ? "Review, edit, regenerate, or send the draft reply."
                : "Draft a reply from the selected message and recent context.";

    public bool HasRequestedInformation => RequestedInformation.Count > 0;
    public bool HasWarnings => Warnings.Count > 0;

    partial void OnSelectedMessageChanged(SupportConversationMessage? value)
    {
        foreach (var message in Messages)
        {
            message.IsSelected = message == value;
        }

        ClearDraftState(clearReply: true);
        NotifyAllState();
    }

    partial void OnReplyTextChanged(string value)
    {
        if (suppressReplyTracking)
        {
            return;
        }

        HasUserEdits = HasValidatedDraft && value != _lastValidatedReply;
        NotifyState();
    }

    partial void OnIsDraftingChanged(bool value) => NotifyState();
    partial void OnIsGenerationErrorChanged(bool value) => NotifyState();
    partial void OnIsRegenerationErrorChanged(bool value) => NotifyState();
    partial void OnShowStatusChanged(bool value) => NotifyState();
    partial void OnShowRequestedInformationChanged(bool value) => NotifyState();
    partial void OnHasValidatedDraftChanged(bool value) => NotifyState();
    partial void OnHasUserEditsChanged(bool value) => NotifyState();

    private void SelectMessage(SupportConversationMessage? message)
    {
        if (message is null || !message.CanSelect || IsDrafting)
        {
            return;
        }

        SelectedMessage = message;
    }

    private async Task DraftReplyAsync()
    {
        if (SelectedMessage is null || IsDrafting)
        {
            return;
        }

        await _generationGate.WaitAsync();
        var version = Interlocked.Increment(ref _requestVersion);
        var hadExistingDraft = HasValidatedDraft && !string.IsNullOrWhiteSpace(ReplyText);
        var previousReply = ReplyText;
        var previousRequestedInformation = RequestedInformation.ToList();
        var previousWarnings = Warnings.ToList();
        try
        {
            IsDrafting = true;
            ShowStatus = false;
            IsGenerationError = false;
            IsRegenerationError = false;
            HasValidatedDraft = hadExistingDraft;
            HasUserEdits = false;
            RequestedInformation.Clear();
            Warnings.Clear();

            if (!hadExistingDraft)
            {
                SetReplyText(string.Empty, fromSystem: true);
            }

            NotifyState();

            var request = new SupportReplyRequest(
                SelectedMessage.Id,
                SelectedMessage.Text,
                _conversationService.BuildRecentContext(Messages, SelectedMessage.Id));

            var result = await _supportReplyService.DraftReplyAsync(request, CancellationToken.None);
            if (version != _requestVersion)
            {
                return;
            }

            if (!result.IsValid || result.Response is null)
            {
                if (!hadExistingDraft)
                {
                    SetReplyText(string.Empty, fromSystem: true);
                }
                else
                {
                    SetReplyText(previousReply, fromSystem: true);
                    HasValidatedDraft = true;
                    RequestedInformation = new ObservableCollection<string>(previousRequestedInformation);
                    Warnings = new ObservableCollection<string>(previousWarnings);
                    ShowRequestedInformation = RequestedInformation.Count > 0;
                }

                IsGenerationError = true;
                IsRegenerationError = hadExistingDraft;
                ShowStatus = true;
                StatusTitle = hadExistingDraft ? "Unable to regenerate the reply" : "Unable to draft a reply";
                StatusMessage = hadExistingDraft
                    ? "Your current draft has been preserved."
                    : "The AI service is currently unavailable. Your conversation and selected message have been preserved.";
                return;
            }

            ApplyDraft(result.Response);
        }
        finally
        {
            IsDrafting = false;
            _generationGate.Release();
            NotifyState();
        }
    }

    private async Task RegenerateAsync()
    {
        if (SelectedMessage is null || IsDrafting)
        {
            return;
        }

        if (HasUserEdits)
        {
            var confirmed = await ConfirmRegenerationAsync();
            if (!confirmed)
            {
                return;
            }
        }

        await DraftReplyAsync();
    }

    private async Task<bool> ConfirmRegenerationAsync()
    {
        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page is null)
        {
            return true;
        }

        return await page.DisplayAlertAsync(
            "Replace current draft?",
            "Regenerating will replace your current edits. Do you want to continue?",
            "Regenerate",
            "Cancel");
    }

    private void DiscardDraft()
    {
        SetReplyText(string.Empty, fromSystem: true);
        RequestedInformation.Clear();
        Warnings.Clear();
        ShowStatus = false;
        StatusTitle = string.Empty;
        StatusMessage = string.Empty;
        IsGenerationError = false;
        IsRegenerationError = false;
        HasValidatedDraft = false;
        HasUserEdits = false;
        NotifyState();
    }

    private void SendReply()
    {
        if (SelectedMessage is null || string.IsNullOrWhiteSpace(ReplyText))
        {
            return;
        }

        Messages.Add(new SupportConversationMessage(
            $"AGENT-{Messages.Count + 1:000}",
            "Jordan Lee",
            "JL",
            SupportConversationRole.SupportAgent,
            DateTime.Now.ToString("h:mm tt"),
            ReplyText.Trim()));

        DiscardDraft();
    }

    private void ApplyDraft(SupportReplyResponse response)
    {
        _lastValidatedReply = response.Reply;
        SetReplyText(response.Reply, fromSystem: true);
        HasValidatedDraft = true;
        HasUserEdits = false;
        ShowStatus = false;
        IsGenerationError = false;
        IsRegenerationError = false;
        RequestedInformation = new ObservableCollection<string>(response.RequestedInformation);
        Warnings = new ObservableCollection<string>(response.Warnings);
        ShowRequestedInformation = RequestedInformation.Count > 0;
    }

    private void ClearDraftState(bool clearReply)
    {
        if (clearReply)
        {
            SetReplyText(string.Empty, fromSystem: true);
            _lastValidatedReply = string.Empty;
        }

        RequestedInformation.Clear();
        Warnings.Clear();
        ShowRequestedInformation = false;
        ShowStatus = false;
        StatusTitle = string.Empty;
        StatusMessage = string.Empty;
        IsDrafting = false;
        IsGenerationError = false;
        IsRegenerationError = false;
        HasValidatedDraft = false;
        HasUserEdits = false;
    }

    private void SetReplyText(string value, bool fromSystem)
    {
        suppressReplyTracking = fromSystem;
        ReplyText = value;
        suppressReplyTracking = false;
    }

    private void NotifyState()
    {
        OnPropertyChanged(nameof(CanDraftReply));
        OnPropertyChanged(nameof(CanRegenerate));
        OnPropertyChanged(nameof(CanDiscard));
        OnPropertyChanged(nameof(CanSend));
        OnPropertyChanged(nameof(HasReply));
        OnPropertyChanged(nameof(HasRequestedInformation));
        OnPropertyChanged(nameof(HasWarnings));
        OnPropertyChanged(nameof(IsComposerEnabled));
        OnPropertyChanged(nameof(ComposerHint));
        (DraftReplyCommand as Command)?.ChangeCanExecute();
        (RegenerateCommand as Command)?.ChangeCanExecute();
        (DiscardCommand as Command)?.ChangeCanExecute();
        (RetryCommand as Command)?.ChangeCanExecute();
        (SendCommand as Command)?.ChangeCanExecute();
    }

    private void NotifyAllState()
    {
        NotifyState();
        OnPropertyChanged(nameof(ShowStatus));
        OnPropertyChanged(nameof(ShowRequestedInformation));
    }
}
