using System.Text.Json.Serialization;

namespace AIProcessDiagramDrafting.Models;

public sealed class WorkflowWarning
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}
