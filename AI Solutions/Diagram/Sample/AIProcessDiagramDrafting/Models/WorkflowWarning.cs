using System.Text.Json.Serialization;
using AIProcessDiagramDrafting.Services;

namespace AIProcessDiagramDrafting.Models;

[JsonConverter(typeof(WorkflowWarningJsonConverter))]
public sealed class WorkflowWarning
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}
