using System.Text.Json.Serialization;

namespace AIDiagramDrafting.Models;

public sealed class WorkflowNode
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public WorkflowNodeType Type { get; set; }

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;
}
