using AIProcessDiagramDrafting.Models;

namespace AIProcessDiagramDrafting.Services;

public sealed class FakeWorkflowExtractionService : IWorkflowExtractionService
{
    private readonly string mode;

    public FakeWorkflowExtractionService(string? mode = null)
    {
        this.mode = string.IsNullOrWhiteSpace(mode) ? "Warning" : mode.Trim();
    }

    public Task<WorkflowResponse> ExtractAsync(string processDescription, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(processDescription);

        if (string.Equals(mode, "Failure", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The fake workflow service is configured to fail.");
        }

        if (string.Equals(mode, "InvalidGraph", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(new WorkflowResponse
            {
                Title = "Invalid draft",
                Nodes = new List<WorkflowNode>
                {
                    new() { Id = "N1", Type = WorkflowNodeType.Start, Label = "Start" },
                    new() { Id = "N1", Type = WorkflowNodeType.Process, Label = "Duplicate node id" },
                    new() { Id = "N2", Type = WorkflowNodeType.End, Label = "End" }
                },
                Connectors = new List<WorkflowConnector>
                {
                    new() { Id = "C1", SourceId = "N1", TargetId = "N2", Label = string.Empty },
                    new() { Id = "C1", SourceId = "N1", TargetId = "N1", Label = string.Empty }
                }
            });
        }

        var warnings = string.Equals(mode, "Warning", StringComparison.OrdinalIgnoreCase)
            ? new List<WorkflowWarning>
            {
                new() { Message = "The finance review branch is incomplete and should be checked before finalizing the workflow." }
            }
            : new List<WorkflowWarning>();

        return Task.FromResult(new WorkflowResponse
        {
            Title = "Purchase Request Approval",
            Nodes = new List<WorkflowNode>
            {
                new() { Id = "N1", Type = WorkflowNodeType.Start, Label = "Start" },
                new() { Id = "N2", Type = WorkflowNodeType.Process, Label = "Submit purchase request" },
                new() { Id = "N3", Type = WorkflowNodeType.Process, Label = "Manager reviews request" },
                new() { Id = "N4", Type = WorkflowNodeType.Decision, Label = "Amount below ₹50,000?" },
                new() { Id = "N5", Type = WorkflowNodeType.Process, Label = "Approve request" },
                new() { Id = "N6", Type = WorkflowNodeType.Process, Label = "Finance reviews request" },
                new() { Id = "N7", Type = WorkflowNodeType.Process, Label = "Create purchase order" },
                new() { Id = "N8", Type = WorkflowNodeType.Process, Label = "Notify employee" },
                new() { Id = "N9", Type = WorkflowNodeType.End, Label = "End" }
            },
            Connectors = new List<WorkflowConnector>
            {
                new() { Id = "C1", SourceId = "N1", TargetId = "N2", Label = string.Empty },
                new() { Id = "C2", SourceId = "N2", TargetId = "N3", Label = string.Empty },
                new() { Id = "C3", SourceId = "N3", TargetId = "N4", Label = string.Empty },
                new() { Id = "C4", SourceId = "N4", TargetId = "N5", Label = "Yes" },
                new() { Id = "C5", SourceId = "N4", TargetId = "N6", Label = "No" },
                new() { Id = "C6", SourceId = "N5", TargetId = "N7", Label = string.Empty },
                new() { Id = "C7", SourceId = "N6", TargetId = "N8", Label = string.Empty },
                new() { Id = "C8", SourceId = "N7", TargetId = "N9", Label = string.Empty },
                new() { Id = "C9", SourceId = "N8", TargetId = "N9", Label = string.Empty }
            },
            Warnings = warnings
        });
    }
}
