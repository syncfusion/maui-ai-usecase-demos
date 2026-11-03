namespace AIProcessDiagramDrafting.Models;

public sealed class WorkflowNode
{
    public string Id { get; set; } = string.Empty;

    public WorkflowNodeType Type { get; set; }

    public string Label { get; set; } = string.Empty;
}
