using System.Globalization;
using Microsoft.Maui.Controls;
using Syncfusion.Maui.Chat;

namespace SupportReplyAIDraft.Models;

public enum SupportConversationRole
{
    Customer,
    SupportAgent
}

public class SupportConversationMessage : TextMessage
{
    public static readonly BindableProperty IsSelectedProperty = BindableProperty.Create(
        nameof(IsSelected),
        typeof(bool),
        typeof(SupportConversationMessage),
        false);

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
        Author = new Author { Name = authorName };
        Text = text;
        DateTime = DateTime.ParseExact(timestamp, "h:mm tt", CultureInfo.InvariantCulture);
        DeliveryState = DeliveryStates.Delivered;
    }

    public string Id { get; }
    public string AuthorName { get; }
    public string Initials { get; }
    public SupportConversationRole Role { get; }
    public string Timestamp { get; }

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public bool CanSelect => Role == SupportConversationRole.Customer;
    public bool IsAgent => Role == SupportConversationRole.SupportAgent;
}
