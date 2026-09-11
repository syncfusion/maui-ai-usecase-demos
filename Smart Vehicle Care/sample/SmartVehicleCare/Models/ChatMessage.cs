namespace SmartVehicleCare.Models;

/// <summary>
/// Represents a chat message in the AI AssistView.
/// Used for both user requests and AI responses.
/// </summary>
public class ChatMessage
{
    /// <summary>
    /// The author of the message (user or AI assistant).
    /// </summary>
    public Author Author { get; set; } = new();

    /// <summary>
    /// The text content of the message.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// The timestamp when the message was created.
    /// </summary>
    public DateTime TimeStamp { get; set; } = DateTime.Now;

    /// <summary>
    /// Indicates if this is an AI response (true) or user request (false).
    /// </summary>
    public bool IsResponse { get; set; }
}

/// <summary>
/// Represents the author of a chat message.
/// </summary>
public class Author
{
    /// <summary>
    /// The name of the author (e.g., "You", "Vehicle Mate AI").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The path to the author's avatar image.
    /// </summary>
    public string Avatar { get; set; } = string.Empty;
}
