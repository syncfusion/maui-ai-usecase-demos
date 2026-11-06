using System.Text.Json;
using System.Text.Json.Serialization;
using AIProcessDiagramDrafting.Models;

namespace AIProcessDiagramDrafting.Services;

public sealed class WorkflowWarningJsonConverter : JsonConverter<WorkflowWarning>
{
    public override WorkflowWarning Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            return new WorkflowWarning { Message = reader.GetString() ?? string.Empty };
        }

        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected a warning string or object.");
        }

        using var document = JsonDocument.ParseValue(ref reader);
        if (document.RootElement.TryGetProperty("message", out var messageElement))
        {
            return new WorkflowWarning { Message = messageElement.GetString() ?? string.Empty };
        }

        return new WorkflowWarning();
    }

    public override void Write(Utf8JsonWriter writer, WorkflowWarning value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Message);
}
