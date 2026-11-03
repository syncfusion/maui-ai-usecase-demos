using AIProcessDiagramDrafting.Models;

namespace AIProcessDiagramDrafting.Services;

public sealed class WorkflowExtractionService : IWorkflowExtractionService
{
    public Task<WorkflowResponse> ExtractAsync(string processDescription, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new WorkflowResponse());
    }
}
