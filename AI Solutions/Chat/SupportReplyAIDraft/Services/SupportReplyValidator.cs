using System.Text.Json;
using System.Text.Json.Serialization;
using SupportReplyAIDraft.Models;

namespace SupportReplyAIDraft.Services;

public sealed class SupportReplyValidator
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Disallow,
        AllowTrailingCommas = false
    };

    public SupportReplyValidationResult Validate(string? rawJson)
    {
        if (string.IsNullOrWhiteSpace(rawJson))
        {
            return SupportReplyValidationResult.Failure("The AI response was empty.");
        }

        var json = ExtractJson(rawJson);
        if (json is null)
        {
            return SupportReplyValidationResult.Failure("The AI response was not valid JSON.");
        }

        try
        {
            var response = JsonSerializer.Deserialize<SupportReplyEnvelope>(json, JsonOptions);
            if (response is null)
            {
                return SupportReplyValidationResult.Failure("The AI response was not valid JSON.");
            }

            if (string.IsNullOrWhiteSpace(response.Reply))
            {
                return SupportReplyValidationResult.Failure("Reply is required.");
            }

            var reply = response.Reply.Trim();

            if (reply.Length > 1000)
            {
                return SupportReplyValidationResult.Failure("Reply exceeds the 1,000 character limit.");
            }

            if (ContainsUnsupportedContent(reply)
                || ContainsUnsupportedClaim(reply)
                || ContainsUnsupportedContent(response.RequestedInformation)
                || ContainsUnsupportedContent(response.Warnings)
                || HasDuplicateRequestedInformation(response.RequestedInformation))
            {
                return SupportReplyValidationResult.Failure("The AI response contains unsupported content.");
            }

            return SupportReplyValidationResult.Success(new SupportReplyResponse(
                reply,
                response.RequiresMoreInformation,
                response.RequestedInformation ?? new List<string>(),
                response.Warnings ?? new List<string>()));
        }
        catch (JsonException)
        {
            return SupportReplyValidationResult.Failure("The AI response was not valid JSON.");
        }
    }

    private static string? ExtractJson(string rawJson)
    {
        var trimmed = rawJson.Trim();
        if (trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            var firstLineEnd = trimmed.IndexOf('\n');
            if (firstLineEnd >= 0)
            {
                trimmed = trimmed[(firstLineEnd + 1)..];
            }

            if (trimmed.EndsWith("```", StringComparison.Ordinal))
            {
                trimmed = trimmed[..^3];
            }

            trimmed = trimmed.Trim();
        }

        return trimmed.StartsWith('{') && trimmed.EndsWith('}') ? trimmed : null;
    }

    private static bool ContainsUnsupportedContent(string value)
        => value.Contains("http://", StringComparison.OrdinalIgnoreCase)
        || value.Contains("https://", StringComparison.OrdinalIgnoreCase)
        || value.Contains("sourceId", StringComparison.OrdinalIgnoreCase)
        || value.Contains("knowledge", StringComparison.OrdinalIgnoreCase)
        || value.Contains("webview", StringComparison.OrdinalIgnoreCase);

    private static bool ContainsUnsupportedClaim(string value)
        => ContainsAny(value,
            "we have verified",
            "i have verified",
            "confirmed on our end",
            "your account has been updated",
            "your issue is resolved",
            "guarantee",
            "promise",
            "will be resolved",
            "refund has been issued",
            "license has been reset",
            "we reviewed your account");

    private static bool ContainsUnsupportedContent(IEnumerable<string>? values)
        => values is not null && values.Any(ContainsUnsupportedContent);

    private static bool HasDuplicateRequestedInformation(IEnumerable<string>? values)
    {
        if (values is null)
        {
            return false;
        }

        var items = values.Where(item => !string.IsNullOrWhiteSpace(item))
            .Select(item => item.Trim())
            .ToList();
        return items.Count != items.Distinct(StringComparer.OrdinalIgnoreCase).Count();
    }

    private static bool ContainsAny(string value, params string[] phrases)
        => phrases.Any(phrase => value.Contains(phrase, StringComparison.OrdinalIgnoreCase));

    private sealed class SupportReplyEnvelope
    {
        [JsonPropertyName("reply")]
        public string? Reply { get; set; }

        [JsonPropertyName("requiresMoreInformation")]
        public bool RequiresMoreInformation { get; set; }

        [JsonPropertyName("requestedInformation")]
        public List<string>? RequestedInformation { get; set; }

        [JsonPropertyName("warnings")]
        public List<string>? Warnings { get; set; }
    }
}
