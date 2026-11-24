using MauiNotes.Models.AI;

namespace MauiNotes.Services.AI;

public interface IAiEditingService
{
    Task<string> GenerateAsync(
        AiEditingRequest request,
        CancellationToken cancellationToken);
}
