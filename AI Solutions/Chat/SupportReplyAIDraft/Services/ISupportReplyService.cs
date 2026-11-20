using SupportReplyAIDraft.Models;

namespace SupportReplyAIDraft.Services;

public interface ISupportReplyService
{
    Task<SupportReplyValidationResult> DraftReplyAsync(SupportReplyRequest request, CancellationToken cancellationToken);
}
