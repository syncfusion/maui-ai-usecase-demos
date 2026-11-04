using SupportReplyAIDraft.Models;
using Syncfusion.Maui.Chat;

namespace SupportReplyAIDraft.Views.MessageTemplates;

public sealed class SupportChatMessageTemplateSelector : ChatMessageTemplateSelector
{
    private readonly DataTemplate _customerTemplate;
    private readonly DataTemplate _agentTemplate;

    public SupportChatMessageTemplateSelector(SfChat sfChat) : base(sfChat)
    {
        _customerTemplate = new DataTemplate(typeof(CustomerMessageTemplateView));
        _agentTemplate = new DataTemplate(typeof(AgentMessageTemplateView));
    }

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        return item is SupportConversationMessage message && message.IsAgent
            ? _agentTemplate
            : _customerTemplate;
    }
}
