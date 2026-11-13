using Syncfusion.Maui.Chat;

namespace SupportReplyAIDraft.Views.MessageTemplates;

public sealed class SupportChatMessageTemplateSelector : ChatMessageTemplateSelector
{
    private readonly SfChat _chat;
    private readonly DataTemplate _customerTemplate;
    private readonly DataTemplate _agentTemplate;

    public SupportChatMessageTemplateSelector(SfChat chat) : base(chat)
    {
        _chat = chat;
        _customerTemplate = new DataTemplate(typeof(CustomerMessageTemplateView));
        _agentTemplate = new DataTemplate(typeof(AgentMessageTemplateView));
    }

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        if (item is not TextMessage message)
        {
            return _customerTemplate;
        }

        // Match the current chat user instead of hard-coding an author name.
        var isAgentMessage = ReferenceEquals(message.Author, _chat.CurrentUser)
            || string.Equals(
                message.Author?.Name,
                _chat.CurrentUser?.Name,
                StringComparison.Ordinal);

        return isAgentMessage
            ? _agentTemplate
            : _customerTemplate;
    }
}
