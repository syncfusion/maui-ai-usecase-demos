using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Syncfusion.Maui.Chat;

namespace SupportReplyAIDraft.Services;

public sealed class ConversationService
{
    private readonly Author _customerAuthor = new() { Name = "Alex Morgan" };
    private readonly Author _supportAuthor = new() { Name = "Jordan Lee" };

    public Author SupportAuthor => _supportAuthor;

    public ObservableCollection<object> CreateConversation()
    {
        return new ObservableCollection<object>
        {
            CreateTextMessage(_customerAuthor, "9:12 AM", "Hello, I renewed my Desktop Analytics Suite subscription yesterday, but the application still shows that my license has expired."),
            CreateTextMessage(_supportAuthor, "9:15 AM", "Thanks for contacting support. Did the renewal complete successfully in your account?"),
            CreateTextMessage(_customerAuthor, "9:17 AM", "Yes, the renewal was successful and I received a confirmation email."),
            CreateTextMessage(_supportAuthor, "9:17 AM", "Thanks for confirming. Please restart the application and check whether the renewed license is recognized."),
            CreateTextMessage(_customerAuthor, "9:18 AM", "I restarted the application, but the expired-license message is still displayed. What should I do next?")
        };
    }

    private static TextMessage CreateTextMessage(Author author, string timestamp, string text)
    {
        return new TextMessage
        {
            Author = author,
            DateTime = DateTime.ParseExact(timestamp, "h:mm tt", CultureInfo.InvariantCulture),
            Text = text
        };
    }

    public bool IsCustomerMessage(TextMessage message)
    {
        return message.Author?.Name == _customerAuthor.Name;
    }

    public IReadOnlyList<string> BuildRecentContext(IEnumerable<object> conversation, TextMessage selectedMessage)
    {
        var list = conversation.OfType<TextMessage>().ToList();
        var index = list.FindIndex(item => ReferenceEquals(item, selectedMessage));
        if (index < 0)
        {
            return Array.Empty<string>();
        }

        return list.Take(index + 1)
            .Select(item => $"{item.Author?.Name ?? "Unknown"} ({item.DateTime:h:mm tt}): {item.Text}")
            .ToList();
    }
}
