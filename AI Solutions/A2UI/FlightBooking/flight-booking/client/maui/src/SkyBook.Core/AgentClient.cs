using System.Text.Json.Nodes;

namespace SkyBook.Core;

public sealed record AgentReply(JsonArray? A2uiMessages, string AgentText, string? ContextId);

/// <summary>A2A JSON-RPC <c>message/send</c> client (same wire format as the React client).</summary>
public sealed class AgentClient(HttpClient http, Uri agentUrl) : IAgentClient
{
    public static readonly Uri DefaultUrl = new("http://localhost:10006");

    public async Task<AgentReply> SendAsync(string text, string? contextId, CancellationToken ct = default)
    {
        var message = new JsonObject
        {
            ["kind"] = "message",
            ["messageId"] = NewId("msg"),
            ["role"] = "user",
            ["parts"] = new JsonArray(new JsonObject { ["text"] = text }),
        };
        if (contextId is not null) message["contextId"] = contextId;

        var body = new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["id"] = NewId("req"),
            ["method"] = "message/send",
            ["params"] = new JsonObject { ["message"] = message },
        };

        // Buffered string content => Content-Length is sent (no chunked encoding), which every A2A server accepts.
        using var content = new StringContent(body.ToJsonString(), System.Text.Encoding.UTF8, "application/json");
        using var res = await http.PostAsync(agentUrl, content, ct);
        return Parse(JsonNode.Parse(await res.Content.ReadAsStringAsync(ct)));
    }

    public static AgentReply Parse(JsonNode? envelope)
    {
        var result = envelope?["result"];
        var ctxId = result?["contextId"] is JsonValue v && v.TryGetValue<string>(out var s) ? s : null;

        var parts = (result?["artifacts"] as JsonArray ?? [])
            .SelectMany(a => a?["parts"] as JsonArray ?? [])
            .ToList();
        var a2ui = parts.Select(p => p?["data"]?["a2uiEnvelope"]).OfType<JsonArray>().FirstOrDefault();

        var statusParts = result?["status"]?["message"]?["parts"] as JsonArray ?? [];
        var agentText = string.Join(" ",
            statusParts.Concat(parts)
                .Select(p => p?["text"] is JsonValue t && t.TryGetValue<string>(out var x) ? x : null)
                .Where(t => !string.IsNullOrWhiteSpace(t))).Trim();

        return new AgentReply(a2ui?.DeepClone() as JsonArray, agentText, ctxId);
    }

    public static string NewId(string prefix) =>
        $"{prefix}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}-{Guid.NewGuid().ToString("N")[..5]}";
}
