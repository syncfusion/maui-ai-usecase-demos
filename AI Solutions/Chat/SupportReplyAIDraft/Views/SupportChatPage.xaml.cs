using System.ComponentModel;
using System.Windows.Input;
using SupportReplyAIDraft.ViewModels;
using Syncfusion.Maui.Chat;

namespace SupportReplyAIDraft.Views;

public partial class SupportChatPage : ContentPage
{
    public SupportChatPage(SupportChatViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
        BindingContext = ViewModel;
        ConversationChat.MessageTemplate = new SupportReplyAIDraft.Views.MessageTemplates.SupportChatMessageTemplateSelector(ConversationChat);
        Loaded += OnLoaded;
        ViewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    public SupportChatViewModel ViewModel { get; }

    public ICommand SelectMessageCommand => ViewModel.SelectMessageCommand;
    public ICommand DraftReplyCommand => ViewModel.DraftReplyCommand;

    private void OnLoaded(object? sender, EventArgs e)
    {
        UpdateComposerState();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SupportChatViewModel.SelectedMessage))
        {
            UpdateComposerState();
        }
    }

    private void UpdateComposerState()
    {
        if (ConversationChat.Editor is null)
        {
            return;
        }

        ConversationChat.Editor.IsEnabled = false;
        ConversationChat.Editor.IsReadOnly = true;
        ConversationChat.Editor.Text = string.Empty;
        ConversationChat.Editor.Placeholder = ViewModel.ComposerPlaceholder;
    }
}
