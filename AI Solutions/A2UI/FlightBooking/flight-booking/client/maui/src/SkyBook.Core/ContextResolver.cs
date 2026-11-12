using System.Text.Json;
using System.Text.Json.Nodes;

namespace SkyBook.Core;

/// <summary>
/// Turns an A2UI client-action context into plain JSON. Values shaped like <c>{"path": "/x"}</c> are resolved against
/// the surface data model; everything else is passed through. (The React renderer hands the app already-resolved values;
/// this keeps the MAUI client correct whichever shape the renderer delivers.)
/// </summary>
public static class ContextResolver
{
    public static JsonObject Resolve(IDictionary<string, object?>? context, Func<string, object?> lookup)
    {
        var result = new JsonObject();
        if (context is null) return result;
        foreach (var (key, raw) in context)
        {
            var node = ToNode(raw);
            if (node is JsonObject o && o.Count == 1 && o["path"] is JsonValue p && p.TryGetValue<string>(out var path))
                node = ToNode(lookup(path));
            // SfDataGrid writes the tapped row back as a JSON string (selectedRowJson); the agent expects an object.
            if (key == "selectedRow" && node is JsonValue sv && sv.TryGetValue<string>(out var json) && json.TrimStart().StartsWith('{'))
                node = TryParse(json);
            // The stock DateTimeInput writes DateTime.ToString() (culture specific); the agent expects ISO yyyy-MM-dd like React.
            if (key.EndsWith("Date", StringComparison.Ordinal) && node is JsonValue dv && dv.TryGetValue<string>(out var ds)
                && DateTime.TryParse(ds, System.Globalization.CultureInfo.CurrentCulture, System.Globalization.DateTimeStyles.None, out var dt))
                node = JsonValue.Create(dt.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture));
            result[key] = node;
        }
        return result;
    }

    private static JsonNode? TryParse(string json)
    {
        try { return JsonNode.Parse(json); } catch (JsonException) { return null; }
    }

    private static JsonNode? ToNode(object? value) => value switch
    {
        null => null,
        JsonNode n => n.DeepClone(),
        JsonElement e => JsonNode.Parse(e.GetRawText()),
        _ => JsonSerializer.SerializeToNode(value),
    };
}
