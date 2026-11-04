using System.Text.Json.Serialization;

namespace AIProcessDiagramDrafting.Models;

public sealed class WorkflowResponse
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("nodes")]
    public IList<WorkflowNode> Nodes { get; set; } = new List<WorkflowNode>();

    [JsonPropertyName("connectors")]
    public IList<WorkflowConnector> Connectors { get; set; } = new List<WorkflowConnector>();

    [JsonPropertyName("warnings")]
    public IList<WorkflowWarning> Warnings { get; set; } = new List<WorkflowWarning>();
}
