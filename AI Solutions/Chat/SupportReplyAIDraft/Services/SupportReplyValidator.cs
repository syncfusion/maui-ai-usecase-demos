using System.Text.Json;
using SupportReplyAIDraft.Models;

namespace SupportReplyAIDraft.Services;

public sealed class SupportReplyValidator
{
    private static readonly HashSet<string> RequiredKeys = new(StringComparer.Ordinal)
    {
        "reply",
        "requiresMoreInformation",
        "requestedInformation",
        "warnings"
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
            using var document = JsonDocument.Parse(json, new JsonDocumentOptions
            {
                CommentHandling = JsonCommentHandling.Disallow,
                AllowTrailingCommas = false
            });

            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return SupportReplyValidationResult.Failure("The AI response was not valid JSON.");
            }

            var root = document.RootElement;
            var keys = root.EnumerateObject().Select(property => property.Name).ToList();
            if (keys.Count != RequiredKeys.Count
                || keys.Any(key => !RequiredKeys.Contains(key)))
            {
                return SupportReplyValidationResult.Failure("The AI response was not valid JSON.");
            }

            if (!root.TryGetProperty("reply", out var replyElement)
                || replyElement.ValueKind != JsonValueKind.String)
            {
                return SupportReplyValidationResult.Failure("Reply is required.");
            }

            var reply = (replyElement.GetString() ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(reply))
            {
                return SupportReplyValidationResult.Failure("Reply is required.");
            }

            if (reply.Length > 1000)
            {
                return SupportReplyValidationResult.Failure("Reply exceeds the 1,000 character limit.");
            }

            if (!root.TryGetProperty("requiresMoreInformation", out var requiresMoreInformationElement)
                || (requiresMoreInformationElement.ValueKind != JsonValueKind.False
                    && requiresMoreInformationElement.ValueKind != JsonValueKind.True))
            {
                return SupportReplyValidationResult.Failure("The AI response was not valid JSON.");
            }

            if (!root.TryGetProperty("requestedInformation", out var requestedInformationElement)
                || requestedInformationElement.ValueKind != JsonValueKind.Array)
            {
                return SupportReplyValidationResult.Failure("The AI response was not valid JSON.");
            }

            if (!root.TryGetProperty("warnings", out var warningsElement)
                || warningsElement.ValueKind != JsonValueKind.Array)
            {
                return SupportReplyValidationResult.Failure("The AI response was not valid JSON.");
            }

            var requestedInformation = ParseStringArray(requestedInformationElement);
            if (requestedInformation is null)
            {
                return SupportReplyValidationResult.Failure("The AI response was not valid JSON.");
            }

            var warnings = ParseStringArray(warningsElement);
            if (warnings is null)
            {
                return SupportReplyValidationResult.Failure("The AI response was not valid JSON.");
            }

            if (ContainsUnsupportedContent(reply)
                || ContainsUnsupportedClaim(reply)
                || ContainsUnsupportedContent(requestedInformation)
                || ContainsUnsupportedContent(warnings)
                || HasDuplicateRequestedInformation(requestedInformation))
            {
                return SupportReplyValidationResult.Failure("The AI response contains unsupported content.");
            }

            return SupportReplyValidationResult.Success(new SupportReplyResponse(
                reply,
                requiresMoreInformationElement.GetBoolean(),
                requestedInformation,
                warnings));
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

    private static List<string>? ParseStringArray(JsonElement element)
    {
        var values = new List<string>();
        foreach (var item in element.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.String)
            {
                return null;
            }

            values.Add(item.GetString() ?? string.Empty);
        }

        return values;
    }
}
