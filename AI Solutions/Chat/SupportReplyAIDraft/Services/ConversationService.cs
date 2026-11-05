using System.Collections.ObjectModel;
using System.Linq;
using Syncfusion.Maui.Chat;
using SupportReplyAIDraft.Models;

namespace SupportReplyAIDraft.Services;

public sealed class ConversationService
{
    private readonly Author _customerAuthor = new() { Name = "Alex Morgan" };
    private readonly Author _supportAuthor = new() { Name = "Jordan Lee" };

    public Author CustomerAuthor => _customerAuthor;
    public Author SupportAuthor => _supportAuthor;

    public ObservableCollection<object> CreateConversation()
    {
        return new ObservableCollection<object>
        {
            new SupportConversationMessage("MSG-001", "Alex Morgan", "AM", SupportConversationRole.Customer, "9:12 AM", "Hello, I renewed my Desktop Analytics Suite subscription yesterday, but the application still shows that my license has expired."),
            new SupportConversationMessage("MSG-002", "Jordan Lee", "JL", SupportConversationRole.SupportAgent, "9:15 AM", "Thanks for contacting support. Did the renewal complete successfully in your account?"),
            new SupportConversationMessage("MSG-003", "Alex Morgan", "AM", SupportConversationRole.Customer, "9:17 AM", "Yes, the renewal was successful and I received a confirmation email."),
            new SupportConversationMessage("MSG-004", "Alex Morgan", "AM", SupportConversationRole.Customer, "9:18 AM", "I restarted the application, but the expired-license message is still displayed. What should I do next?")
        };
    }

    public IReadOnlyList<string> BuildRecentContext(IEnumerable<object> conversation, string selectedMessageId)
    {
        var list = conversation.OfType<SupportConversationMessage>().ToList();
        var index = list.FindIndex(item => item.Id == selectedMessageId);
        if (index < 0)
        {
            return Array.Empty<string>();
        }

        return list.Take(index + 1)
            .Select(item => $"{item.AuthorName} ({item.Timestamp}): {item.Text}")
            .ToList();
    }
}
