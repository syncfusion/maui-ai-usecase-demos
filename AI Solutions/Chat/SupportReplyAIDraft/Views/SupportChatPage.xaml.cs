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
    }

    public SupportChatViewModel ViewModel { get; }

    public ICommand SelectMessageCommand => ViewModel.SelectMessageCommand;
}
