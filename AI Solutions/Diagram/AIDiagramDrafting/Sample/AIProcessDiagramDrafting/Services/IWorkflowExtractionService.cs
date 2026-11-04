using AIDiagramDrafting.Models;

namespace AIDiagramDrafting.Services;

public interface IWorkflowExtractionService
{
    Task<WorkflowResponse> ExtractAsync(string processDescription, CancellationToken cancellationToken = default);
}
