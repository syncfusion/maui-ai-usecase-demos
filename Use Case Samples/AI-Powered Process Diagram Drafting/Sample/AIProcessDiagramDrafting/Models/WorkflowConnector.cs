namespace AIProcessDiagramDrafting.Models;

public sealed class WorkflowConnector
{
    public string Id { get; set; } = string.Empty;

    public string SourceId { get; set; } = string.Empty;

    public string TargetId { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;
}
