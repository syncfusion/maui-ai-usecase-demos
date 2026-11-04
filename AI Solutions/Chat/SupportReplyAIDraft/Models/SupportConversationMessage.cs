using CommunityToolkit.Mvvm.ComponentModel;
using Syncfusion.Maui.Chat;

namespace SupportReplyAIDraft.Models;

public enum SupportConversationRole
{
    Customer,
    SupportAgent
}

public partial class SupportConversationMessage : ObservableObject, IMessage
{
    [ObservableProperty]
    private bool isSelected;

    public SupportConversationMessage(
        string id,
        string authorName,
        string initials,
        SupportConversationRole role,
        string timestamp,
        string text)
    {
        Id = id;
        AuthorName = authorName;
        Initials = initials;
        Role = role;
        Timestamp = timestamp;
        Text = text;
        Author = new Author { Name = authorName };
        Data = text;
        DateTime = DateTime.ParseExact(timestamp, "h:mm tt", System.Globalization.CultureInfo.InvariantCulture);
        DeliveryState = DeliveryStates.Delivered;
    }

    public string Id { get; }
    public string AuthorName { get; }
    public string Initials { get; }
    public SupportConversationRole Role { get; }
    public string Timestamp { get; }
    public string Text { get; }

    public Author Author { get; set; } = new Author();
    public object Data { get; set; }
    public DateTime DateTime { get; set; }
    public DeliveryStates DeliveryState { get; set; }
    public bool HideAfterSelection { get; set; }
    public bool IsMessageContentHiddenAfterSelection { get; set; }
    public bool IsPinned { get; set; }
    public bool IsSuggestionViewHiddenAfterSelection { get; set; }
    public IList<string>? Suggestions { get; set; }

    public bool CanSelect => Role == SupportConversationRole.Customer;
    public bool IsAgent => Role == SupportConversationRole.SupportAgent;
}
