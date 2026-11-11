using System.Text.Json.Nodes;

namespace SkyBook.Core;

/// <summary>Ports of <c>sanitizeEnvelope</c> and <c>extractExplanation</c> from App.tsx.</summary>
public static class EnvelopeSanitizer
{
    private static readonly HashSet<string> Containers = ["Row", "Column"];

    /// <summary>
    /// Removes children that reference components the agent never defined (they would render as "[Loading x...]"),
    /// then drops Row/Column containers (other than "root") that became empty.
    /// </summary>
    public static JsonArray Sanitize(JsonArray messages)
    {
        var defined = new Dictionary<string, HashSet<string>>();
        foreach (var m in messages)
        {
            if (m?["updateComponents"] is not JsonObject uc) continue;
            var surface = uc["surfaceId"]?.GetValue<string>() ?? "";
            if (!defined.TryGetValue(surface, out var set)) defined[surface] = set = [];
            foreach (var c in uc["components"] as JsonArray ?? [])
                if (c?["id"]?.GetValue<string>() is { Length: > 0 } id) set.Add(id);
        }

        var cleaned = new JsonArray();
        foreach (var m in messages)
        {
            var copy = m?.DeepClone();
            if (copy?["updateComponents"] is JsonObject uc)
            {
                var ids = defined[uc["surfaceId"]?.GetValue<string>() ?? ""];
                foreach (var c in (uc["components"] as JsonArray ?? []).OfType<JsonObject>())
                    FilterChildren(c, id => !IsString(id) || ids.Contains(id!.GetValue<string>()));
            }
            cleaned.Add(copy);
        }

        foreach (var m in cleaned)
        {
            if (m?["updateComponents"] is not JsonObject uc || uc["components"] is not JsonArray comps) continue;
            var empty = comps.OfType<JsonObject>()
                .Where(c => c["children"] is JsonArray { Count: 0 }
                            && c["id"]?.GetValue<string>() != "root"
                            && Containers.Contains(c["component"]?.GetValue<string>() ?? ""))
                .Select(c => c["id"]!.GetValue<string>()).ToHashSet();
            if (empty.Count == 0) continue;

            var kept = new JsonArray();
            foreach (var c in comps.OfType<JsonObject>().Where(c => !empty.Contains(c["id"]?.GetValue<string>() ?? "")))
            {
                var clone = (JsonObject)c.DeepClone();
                FilterChildren(clone, id => !IsString(id) || !empty.Contains(id!.GetValue<string>()));
                kept.Add(clone);
            }
            uc["components"] = kept;
        }
        return cleaned;
    }

    /// <summary>Reads the agent's reply text from the last <c>/_explanation</c> data-model update.</summary>
    public static string ExtractExplanation(JsonArray messages)
    {
        for (var i = messages.Count - 1; i >= 0; i--)
        {
            if (messages[i]?["updateDataModel"] is JsonObject u
                && u["path"]?.GetValue<string>() == "/_explanation"
                && u["value"] is JsonValue v && v.TryGetValue<string>(out var s) && !string.IsNullOrWhiteSpace(s))
                return s.Trim();
        }
        return "";
    }

    public static bool HasCreateSurface(JsonArray messages) => messages.Any(m => m?["createSurface"] is not null);

    private static bool IsString(JsonNode? n) => n is JsonValue v && v.TryGetValue<string>(out _);

    private static void FilterChildren(JsonObject c, Func<JsonNode?, bool> keep)
    {
        if (c["children"] is not JsonArray children) return;
        var filtered = new JsonArray();
        foreach (var child in children) if (keep(child)) filtered.Add(child?.DeepClone());
        c["children"] = filtered;
    }
}
