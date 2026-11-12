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

        ViewModel.PropertyChanged +=
            OnViewModelPropertyChanged;

        ViewModel.OutgoingMessageSent +=
            OnOutgoingMessageSent;

        Loaded += OnLoaded;
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
        AttachEditorHandlers();
        UpdateComposerState();
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

    private async Task<bool> ConfirmRegenerateAsync()
    {
        return await DisplayAlertAsync(
            "Replace draft?",
            "Regenerating will replace your current edited draft.",
            "Regenerate",
            "Cancel");
    }

    private async Task<bool> ConfirmSelectionChangeAsync()
    {
        return await DisplayAlertAsync(
            "Discard current draft?",
            "Selecting another customer message will discard the current draft.",
            "Discard draft",
            "Keep editing");
    }

    private void OnOutgoingMessageSent(
        TextMessage message)
    {
        ConversationChat.ScrollToMessage(message);
    }
}