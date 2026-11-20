namespace SupportReplyAIDraft.Models;

public sealed record SupportReplyRequest(
    string SelectedMessageText,
    IReadOnlyList<string> RecentConversationContext);
