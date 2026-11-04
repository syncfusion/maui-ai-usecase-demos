using System.Text.Json.Serialization;

namespace AIDiagramDrafting.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WorkflowNodeType
{
    Start,
    Process,
    Decision,
    End
}
