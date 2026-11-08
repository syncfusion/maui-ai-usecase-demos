using System.ComponentModel;
using System.Windows.Input;
using SupportReplyAIDraft.ViewModels;
using Syncfusion.Maui.Chat;

namespace SupportReplyAIDraft.Views;

public partial class SupportChatPage : ContentPage
{
    private bool _isUpdatingEditorText;

    public SupportChatPage(SupportChatViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
        BindingContext = ViewModel;

        ConversationChat.MessageTemplate = new SupportReplyAIDraft.Views.MessageTemplates.SupportChatMessageTemplateSelector(ConversationChat);

        ViewModel.ConfirmRegenerateAsync = ConfirmRegenerateAsync;
        ViewModel.PropertyChanged += OnViewModelPropertyChanged;
        ViewModel.OutgoingMessageSent += OnOutgoingMessageSent;

        Loaded += OnLoaded;
    }

    public SupportChatViewModel ViewModel { get; }

    public ICommand SelectMessageCommand => ViewModel.SelectMessageCommand;
    public ICommand DraftReplyCommand => ViewModel.DraftReplyCommand;

    private void OnLoaded(object? sender, EventArgs e)
    {
        AttachEditorHandlers();
        UpdateComposerState();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(SupportChatViewModel.SelectedMessage)
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

        ConversationChat.Editor.TextChanged -= OnEditorTextChanged;
        ConversationChat.Editor.TextChanged += OnEditorTextChanged;
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
            ConversationChat.Editor.IsEnabled = ViewModel.IsComposerEnabled;
            ConversationChat.Editor.IsReadOnly = ViewModel.IsComposerReadOnly;
            ConversationChat.Editor.Placeholder = ViewModel.ComposerPlaceholder;

            if (!string.Equals(ConversationChat.Editor.Text, ViewModel.EditorText, StringComparison.Ordinal))
            {
                ConversationChat.Editor.Text = ViewModel.EditorText;
            }
        }
        finally
        {
            _isUpdatingEditorText = false;
        }
    }

    private void OnEditorTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_isUpdatingEditorText)
        {
            return;
        }

        ViewModel.UpdateEditorText(e.NewTextValue);
    }

    private async Task<bool> ConfirmRegenerateAsync()
    {
        return await DisplayAlert(
            "Replace draft?",
            "Regenerating will replace your current edited draft.",
            "Regenerate",
            "Cancel");
    }

    private void OnOutgoingMessageSent(TextMessage message)
    {
        ConversationChat.ScrollToMessage(message);
    }
}
