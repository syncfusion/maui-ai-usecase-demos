using System.ComponentModel;
using System.Windows.Input;
using SupportReplyAIDraft.ViewModels;
using Syncfusion.Maui.Chat;

namespace SupportReplyAIDraft.Views;

public partial class SupportChatPage : ContentPage
{
    private static readonly TimeSpan SuggestionEditorRestoreDelay =
        TimeSpan.FromMilliseconds(100);

    private bool _isUpdatingEditorText;
    private bool _isViewModelEventsAttached;

    public SupportChatPage(
        SupportChatViewModel viewModel)
    {
        ViewModel = viewModel;

        InitializeComponent();

        BindingContext = ViewModel;

        ConversationChat.MessageTemplate =
            new SupportReplyAIDraft.Views.MessageTemplates
                .SupportChatMessageTemplateSelector(
                    ConversationChat);

        ViewModel.ConfirmRegenerateAsync =
            ConfirmRegenerateAsync;

        ViewModel.ConfirmSelectionChangeAsync =
            ConfirmSelectionChangeAsync;

        AttachViewModelEvents();

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    public SupportChatViewModel ViewModel { get; }

    public ICommand SelectMessageCommand =>
        ViewModel.SelectMessageCommand;

    public ICommand DraftReplyCommand =>
        ViewModel.DraftReplyCommand;

    private void OnLoaded(
        object? sender,
        EventArgs e)
    {
        AttachViewModelEvents();
        AttachEditorHandlers();
        UpdateComposerState();
    }

    private void OnUnloaded(
        object? sender,
        EventArgs e)
    {
        if (ConversationChat.Editor is not null)
        {
            ConversationChat.Editor.TextChanged -=
                OnEditorTextChanged;
        }

        DetachViewModelEvents();
    }

    private void AttachViewModelEvents()
    {
        // Keep event wiring idempotent across page load/unload cycles.
        if (_isViewModelEventsAttached)
        {
            return;
        }

        ViewModel.PropertyChanged +=
            OnViewModelPropertyChanged;

        ViewModel.OutgoingMessageSent +=
            OnOutgoingMessageSent;

        _isViewModelEventsAttached = true;
    }

    private void DetachViewModelEvents()
    {
        if (!_isViewModelEventsAttached)
        {
            return;
        }

        ViewModel.PropertyChanged -=
            OnViewModelPropertyChanged;

        ViewModel.OutgoingMessageSent -=
            OnOutgoingMessageSent;

        _isViewModelEventsAttached = false;
    }

    private void OnViewModelPropertyChanged(
        object? sender,
        PropertyChangedEventArgs e)
    {
        if (e.PropertyName is
            nameof(SupportChatViewModel.SelectedMessage)
            or nameof(SupportChatViewModel.ComposerPlaceholder)
            or nameof(SupportChatViewModel.IsComposerEnabled)
            or nameof(SupportChatViewModel.IsComposerReadOnly)
            or nameof(SupportChatViewModel.EditorText))
        {
            UpdateComposerState();
        }
    }

    private void AttachEditorHandlers()
    {
        if (ConversationChat.Editor is null)
        {
            return;
        }

        ConversationChat.Editor.TextChanged -=
            OnEditorTextChanged;

        ConversationChat.Editor.TextChanged +=
            OnEditorTextChanged;
    }

    private void UpdateComposerState()
    {
        if (ConversationChat.Editor is null)
        {
            return;
        }

        _isUpdatingEditorText = true;

        try
        {
            ConversationChat.Editor.IsEnabled =
                ViewModel.IsComposerEnabled;

            ConversationChat.Editor.IsReadOnly =
                ViewModel.IsComposerReadOnly;

            ConversationChat.Editor.Placeholder =
                ViewModel.ComposerPlaceholder;

            if (!string.Equals(
                    ConversationChat.Editor.Text,
                    ViewModel.EditorText,
                    StringComparison.Ordinal))
            {
                ConversationChat.Editor.Text =
                    ViewModel.EditorText;
            }
        }
        finally
        {
            _isUpdatingEditorText = false;
        }
    }

    private void OnEditorTextChanged(
        object? sender,
        TextChangedEventArgs e)
    {
        if (_isUpdatingEditorText)
        {
            return;
        }

        ViewModel.UpdateEditorText(
            e.NewTextValue);
    }

    private async void OnSuggestionItemSelected(
        object? sender,
        SuggestionItemSelectedEventArgs e)
    {
        e.CancelSendMessage = true;
        e.HideAfterSelection = false;

        var selectedText =
            (e.SelectedItem as Suggestion)?.Text;

        ViewModel.BeginSuggestionAction();

        try
        {
            var actionTask =
                ViewModel.HandleSuggestionActionAsync(
                    selectedText);

            Dispatcher.Dispatch(
                UpdateComposerState);

            Dispatcher.DispatchDelayed(
                SuggestionEditorRestoreDelay,
                UpdateComposerState);

            await actionTask;
        }
        finally
        {
            ViewModel.EndSuggestionAction();

            Dispatcher.Dispatch(
                UpdateComposerState);

            Dispatcher.DispatchDelayed(
                SuggestionEditorRestoreDelay,
                UpdateComposerState);
        }
    }

    private Task<bool> ConfirmRegenerateAsync()
    {
        return ConfirmActionAsync(
            "Replace draft?",
            "Regenerating will replace the current draft.",
            "Regenerate",
            "Cancel",
            (Color)Application.Current!.Resources["AccentColor"]);
    }

    private TaskCompletionSource<bool>? _confirmTcs;

    private async Task<bool> ConfirmActionAsync(
        string title,
        string message,
        string accept,
        string cancel,
        Color acceptColor)
    {
        var useOverlay =
            (DeviceInfo.Platform == DevicePlatform.Android
                || DeviceInfo.Platform == DevicePlatform.iOS)
            && DeviceInfo.Idiom != DeviceIdiom.Desktop;

        if (!useOverlay)
        {
            return await DisplayAlertAsync(title, message, accept, cancel);
        }

        ConfirmTitleLabel.Text = title;
        ConfirmMessageLabel.Text = message;
        ConfirmAcceptButton.Text = accept;
        ConfirmAcceptButton.TextColor = acceptColor;
        ConfirmCancelButton.Text = cancel;

        _confirmTcs = new TaskCompletionSource<bool>();
        ConfirmOverlay.IsVisible = true;

        var result = await _confirmTcs.Task;
        ConfirmOverlay.IsVisible = false;
        return result;
    }

    private void OnConfirmAcceptClicked(object? sender, EventArgs e)
        => _confirmTcs?.TrySetResult(true);

    private void OnConfirmCancelClicked(object? sender, EventArgs e)
        => _confirmTcs?.TrySetResult(false);

    private Task<bool> ConfirmSelectionChangeAsync()
    {
        return ConfirmActionAsync(
            "Discard draft?",
            "Selecting another customer message will discard the current draft.",
            "Discard",
            "Cancel",
            (Color)Application.Current!.Resources["DangerColor"]);
    }

    private void OnOutgoingMessageSent(
        TextMessage message)
    {
        ConversationChat.ScrollToMessage(message);
    }
}