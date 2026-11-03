namespace AIProcessDiagramDrafting.Models;

public sealed class WorkflowResponse
{
    public string Title { get; set; } = string.Empty;

    public IList<WorkflowNode> Nodes { get; set; } = new List<WorkflowNode>();

    public IList<WorkflowConnector> Connectors { get; set; } = new List<WorkflowConnector>();

    public IList<WorkflowWarning> Warnings { get; set; } = new List<WorkflowWarning>();
}
