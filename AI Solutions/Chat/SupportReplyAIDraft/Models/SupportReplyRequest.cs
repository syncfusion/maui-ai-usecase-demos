namespace SupportReplyAIDraft.Models;

public sealed record SupportReplyRequest(
    string SelectedMessageId,
    string SelectedMessageText,
    IReadOnlyList<string> RecentConversationContext);
