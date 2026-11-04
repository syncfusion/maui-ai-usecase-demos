using AIDiagramDrafting.Models;

namespace AIDiagramDrafting.Services;

public sealed class WorkflowExtractionService : IWorkflowExtractionService
{
    public Task<WorkflowResponse> ExtractAsync(string processDescription, CancellationToken cancellationToken = default)
    {
        var response = new WorkflowResponse
        {
            Title = "Purchase Request Approval",
            Nodes =
            [
                new WorkflowNode { Id = "N1", Type = WorkflowNodeType.Start, Label = "Start" },
                new WorkflowNode { Id = "N2", Type = WorkflowNodeType.Process, Label = "Submit purchase request" },
                new WorkflowNode { Id = "N3", Type = WorkflowNodeType.Process, Label = "Manager reviews request" },
                new WorkflowNode { Id = "N4", Type = WorkflowNodeType.Decision, Label = "Amount below ₹50,000?" },
                new WorkflowNode { Id = "N5", Type = WorkflowNodeType.Process, Label = "Approve request" },
                new WorkflowNode { Id = "N6", Type = WorkflowNodeType.Process, Label = "Finance reviews request" },
                new WorkflowNode { Id = "N7", Type = WorkflowNodeType.Process, Label = "Create purchase order" },
                new WorkflowNode { Id = "N8", Type = WorkflowNodeType.Process, Label = "Notify employee" },
                new WorkflowNode { Id = "N9", Type = WorkflowNodeType.End, Label = "End" }
            ],
            Connectors =
            [
                new WorkflowConnector { Id = "C1", SourceId = "N1", TargetId = "N2", Label = string.Empty },
                new WorkflowConnector { Id = "C2", SourceId = "N2", TargetId = "N3", Label = string.Empty },
                new WorkflowConnector { Id = "C3", SourceId = "N3", TargetId = "N4", Label = string.Empty },
                new WorkflowConnector { Id = "C4", SourceId = "N4", TargetId = "N5", Label = "Yes" },
                new WorkflowConnector { Id = "C5", SourceId = "N4", TargetId = "N6", Label = "No" },
                new WorkflowConnector { Id = "C6", SourceId = "N5", TargetId = "N7", Label = string.Empty },
                new WorkflowConnector { Id = "C7", SourceId = "N6", TargetId = "N8", Label = string.Empty },
                new WorkflowConnector { Id = "C8", SourceId = "N7", TargetId = "N9", Label = string.Empty },
                new WorkflowConnector { Id = "C9", SourceId = "N8", TargetId = "N9", Label = string.Empty }
            ],
            Warnings =
            [
                new WorkflowWarning { Message = "The finance branch outcome is not fully specified." }
            ]
        };

        return Task.FromResult(response);
    }
}
