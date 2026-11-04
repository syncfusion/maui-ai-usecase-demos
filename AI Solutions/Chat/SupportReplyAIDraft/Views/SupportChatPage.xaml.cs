using SupportReplyAIDraft.ViewModels;
using Syncfusion.Maui.Chat;

namespace SupportReplyAIDraft.Views;

public partial class SupportChatPage : ContentPage
{
    public SupportChatPage(SupportChatViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        ConversationChat.MessageTemplate = new SupportReplyAIDraft.Views.MessageTemplates.SupportChatMessageTemplateSelector(ConversationChat);
    }
}
