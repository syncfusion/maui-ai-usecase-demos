namespace SupportReplyAIDraft.Models;

public sealed record SupportReplyValidationResult(
    bool IsValid,
    string? ErrorMessage,
    SupportReplyResponse? Response)
{
    public static SupportReplyValidationResult Success(SupportReplyResponse response)
        => new(true, null, response);

    public static SupportReplyValidationResult Failure(string message)
        => new(false, message, null);
}
