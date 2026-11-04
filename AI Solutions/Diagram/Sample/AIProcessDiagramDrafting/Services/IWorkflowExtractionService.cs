using AIProcessDiagramDrafting.Models;

namespace AIProcessDiagramDrafting.Services;

public interface IWorkflowExtractionService
{
    Task<WorkflowResponse> ExtractAsync(string processDescription, CancellationToken cancellationToken = default);
}
