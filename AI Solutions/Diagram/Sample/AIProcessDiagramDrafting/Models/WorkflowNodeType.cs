using System.Text.Json.Serialization;

namespace AIProcessDiagramDrafting.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WorkflowNodeType
{
    Start,
    Process,
    Decision,
    End
}
