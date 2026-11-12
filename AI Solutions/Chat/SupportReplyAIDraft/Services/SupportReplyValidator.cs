using System.Text.Json;
using SupportReplyAIDraft.Models;

namespace SupportReplyAIDraft.Services;

public sealed class SupportReplyValidator
{
    private const int MaximumReplyLength = 1000;

    private static readonly HashSet<string> AllowedKeys =
        new(StringComparer.Ordinal)
        {
            "reply",
            "requiresMoreInformation",
            "requestedInformation",
            "warnings"
        };
    private static readonly string[] Warnings =
    [
        "The model returned plain text instead of structured JSON."
    ];

    public static SupportReplyValidationResult Validate(string? rawResponse)
    {
        if (string.IsNullOrWhiteSpace(rawResponse))
        {
            return SupportReplyValidationResult.Failure(
                "The AI returned an empty reply. Please try again.");
        }

        var normalizedResponse = rawResponse.Trim();
        var json = ExtractJson(normalizedResponse);

        if (json is not null)
        {
            var jsonValidation = ValidateJson(json);
            if (jsonValidation.IsValid)
            {
                return jsonValidation;
            }

            if (LooksLikeJson(normalizedResponse))
            {
                return SupportReplyValidationResult.Failure(
                    "We couldn’t process the generated reply. Please try again.");
            }
        }

        return ValidatePlainTextFallback(normalizedResponse);
    }

    private static SupportReplyValidationResult ValidateJson(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(
                json,
                new JsonDocumentOptions
                {
                    CommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                });

            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return SupportReplyValidationResult.Failure(
                    "We couldn’t process the generated reply. Please try again.");
            }

            var root = document.RootElement;

            var suppliedKeys = root
                .EnumerateObject()
                .Select(property => property.Name)
                .ToList();

            if (suppliedKeys.Any(key => !AllowedKeys.Contains(key)))
            {
                return SupportReplyValidationResult.Failure(
                    "The generated reply contained unsupported data.");
            }

            if (!TryGetString(root, "reply", out var reply))
            {
                return SupportReplyValidationResult.Failure(
                    "The generated reply was empty. Please try again.");
            }

            reply = NormalizeReply(reply);

            var replyValidation = ValidateReplyText(reply);
            if (replyValidation is not null)
            {
                return SupportReplyValidationResult.Failure(replyValidation);
            }

            var requiresMoreInformation =
                TryGetBoolean(root, "requiresMoreInformation", out var requires) && requires;

            var requestedInformation =
                TryGetStringArray(root, "requestedInformation", out var requested)
                    ? requested
                    : [];

            var warnings =
                TryGetStringArray(root, "warnings", out var parsedWarnings)
                    ? parsedWarnings
                    : [];

            requestedInformation = NormalizeList(requestedInformation);
            warnings = NormalizeList(warnings);

            if (requestedInformation.Any(ContainsUnsupportedContent)
                || warnings.Any(ContainsUnsupportedContent))
            {
                return SupportReplyValidationResult.Failure(
                    "The generated reply contained unsupported data.");
            }

            if (HasDuplicates(requestedInformation))
            {
                requestedInformation = [.. requestedInformation.Distinct(StringComparer.OrdinalIgnoreCase)];
            }

            if (!requiresMoreInformation)
            {
                requestedInformation.Clear();
            }
            else if (requestedInformation.Count == 0)
            {
                requiresMoreInformation = false;
            }

            return SupportReplyValidationResult.Success(
                new SupportReplyResponse(
                    reply,
                    requiresMoreInformation,
                    requestedInformation,
                    warnings));
        }
        catch (JsonException)
        {
            return SupportReplyValidationResult.Failure(
                "We couldn’t process the generated reply. Please try again.");
        }
    }

    private static SupportReplyValidationResult ValidatePlainTextFallback(
        string rawResponse)
    {
        var plainText = RemoveMarkdownFence(rawResponse);
        plainText = NormalizeReply(plainText);

        var validationError = ValidateReplyText(plainText);
        if (validationError is not null)
        {
            return SupportReplyValidationResult.Failure(validationError);
        }

        return SupportReplyValidationResult.Success(
            new SupportReplyResponse(
                plainText,
                false,
                [],
                Warnings));
    }

    private static string? ValidateReplyText(string reply)
    {
        if (string.IsNullOrWhiteSpace(reply))
        {
            return "The generated reply was empty. Please try again.";
        }

        if (reply.Length > MaximumReplyLength)
        {
            return
                $"The generated reply exceeded the {MaximumReplyLength:N0} character limit.";
        }

        if (ContainsUnsupportedContent(reply))
        {
            return "The generated reply contained unsupported content.";
        }

        if (ContainsUnsupportedClaim(reply))
        {
            return
                "The generated reply made an unsupported customer-account claim.";
        }

        return null;
    }

    private static string NormalizeReply(string value)
    {
        var lines = value
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n')
            .Select(line => line.TrimEnd())
            .ToList();

        while (lines.Count > 0
               && string.IsNullOrWhiteSpace(lines[0]))
        {
            lines.RemoveAt(0);
        }

        while (lines.Count > 0
               && string.IsNullOrWhiteSpace(lines[^1]))
        {
            lines.RemoveAt(lines.Count - 1);
        }

        return string.Join(Environment.NewLine, lines).Trim();
    }

    private static List<string> NormalizeList(
        IEnumerable<string> values)
    {
        return [.. values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)];
    }

    private static string? ExtractJson(string rawResponse)
    {
        var trimmed = RemoveMarkdownFence(rawResponse);

        if (trimmed.StartsWith('{')
            && trimmed.EndsWith('}'))
        {
            return trimmed;
        }

        var start = trimmed.IndexOf('{');
        if (start < 0)
        {
            return null;
        }

        var depth = 0;
        var insideString = false;
        var escaped = false;

        for (var index = start; index < trimmed.Length; index++)
        {
            var character = trimmed[index];

            if (insideString)
            {
                if (escaped)
                {
                    escaped = false;
                    continue;
                }

                if (character == '\\')
                {
                    escaped = true;
                    continue;
                }

                if (character == '"')
                {
                    insideString = false;
                }

                continue;
            }

            if (character == '"')
            {
                insideString = true;
                continue;
            }

            if (character == '{')
            {
                depth++;
                continue;
            }

            if (character != '}')
            {
                continue;
            }

            depth--;

            if (depth == 0)
            {
                return trimmed[start..(index + 1)];
            }
        }

        return null;
    }

    private static string RemoveMarkdownFence(string value)
    {
        var trimmed = value.Trim();

        if (!trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            return trimmed;
        }

        var firstNewLine = trimmed.IndexOf('\n');
        if (firstNewLine >= 0)
        {
            trimmed = trimmed[(firstNewLine + 1)..];
        }

        if (trimmed.EndsWith("```", StringComparison.Ordinal))
        {
            trimmed = trimmed[..^3];
        }

        return trimmed.Trim();
    }

    private static bool LooksLikeJson(string value)
    {
        var trimmed = RemoveMarkdownFence(value);
        return trimmed.StartsWith('{')
            || trimmed.StartsWith('[');
    }

    private static bool TryGetString(
        JsonElement root,
        string propertyName,
        out string value)
    {
        value = string.Empty;

        if (!root.TryGetProperty(propertyName, out var element)
            || element.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        value = element.GetString() ?? string.Empty;
        return true;
    }

    private static bool TryGetBoolean(
        JsonElement root,
        string propertyName,
        out bool value)
    {
        value = false;

        if (!root.TryGetProperty(propertyName, out var element)
            || element.ValueKind is not (
                JsonValueKind.True or JsonValueKind.False))
        {
            return false;
        }

        value = element.GetBoolean();
        return true;
    }

    private static bool TryGetStringArray(
        JsonElement root,
        string propertyName,
        out List<string> values)
    {
        values = [];

        if (!root.TryGetProperty(propertyName, out var element))
        {
            return true;
        }

        if (element.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        foreach (var item in element.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.String)
            {
                return false;
            }

            values.Add(item.GetString() ?? string.Empty);
        }

        return true;
    }

    private static bool ContainsUnsupportedContent(string value)
    {
        return value.Contains("http://", StringComparison.OrdinalIgnoreCase)
            || value.Contains("https://", StringComparison.OrdinalIgnoreCase)
            || value.Contains("sourceId", StringComparison.OrdinalIgnoreCase)
            || value.Contains(
                "knowledge base id",
                StringComparison.OrdinalIgnoreCase)
            || value.Contains("webview", StringComparison.OrdinalIgnoreCase)
            || value.Contains(
                "api key",
                StringComparison.OrdinalIgnoreCase)
            || value.Contains(
                "password",
                StringComparison.OrdinalIgnoreCase)
            || value.Contains(
                "authentication code",
                StringComparison.OrdinalIgnoreCase);
    }

    private static bool ContainsUnsupportedClaim(string value)
    {
        return ContainsAny(
            value,
            "we have verified",
            "i have verified",
            "confirmed on our end",
            "your account has been updated",
            "your issue is resolved",
            "guarantee",
            "we promise",
            "will definitely be resolved",
            "refund has been issued",
            "license has been reset",
            "we reviewed your account",
            "we checked your account",
            "we have escalated",
            "i have escalated");
    }

    private static bool ContainsAny(
        string value,
        params string[] phrases)
    {
        return phrases.Any(
            phrase => value.Contains(
                phrase,
                StringComparison.OrdinalIgnoreCase));
    }

    private static bool HasDuplicates(IEnumerable<string> values)
    {
        var items = values.ToList();

        return items.Count != items
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();
    }
}