namespace SupportReplyAIDraft.Models;

public sealed record SupportReplyResponse(
    string Reply,
    bool RequiresMoreInformation,
    IReadOnlyList<string> RequestedInformation,
    IReadOnlyList<string> Warnings);
