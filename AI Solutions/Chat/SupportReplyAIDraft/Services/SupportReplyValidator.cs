using System.Linq;
using SupportReplyAIDraft.Models;

namespace SupportReplyAIDraft.Services;

public sealed class SupportReplyValidator
{
    private const int MaximumReplyLength = 1000;

    public static SupportReplyValidationResult Validate(string? rawResponse)
    {
        if (string.IsNullOrWhiteSpace(rawResponse))
        {
            return SupportReplyValidationResult.Failure(
                "The AI returned an empty reply. Please try again.");
        }

        var reply = NormalizeReply(RemoveMarkdownFence(rawResponse));

        if (string.IsNullOrWhiteSpace(reply))
        {
            return SupportReplyValidationResult.Failure(
                "The generated reply was empty. Please try again.");
        }

        if (reply.Length > MaximumReplyLength)
        {
            return SupportReplyValidationResult.Failure(
                $"The generated reply exceeded the {MaximumReplyLength:N0} character limit.");
        }

        if (ContainsUnsupportedContent(reply))
        {
            return SupportReplyValidationResult.Failure(
                "The generated reply contained unsupported content.");
        }

        if (ContainsUnsupportedClaim(reply))
        {
            return SupportReplyValidationResult.Failure(
                "The generated reply made an unsupported customer-account claim.");
        }

        return SupportReplyValidationResult.Success(
            new SupportReplyResponse(
                reply,
                false,
                [],
                []));
    }

    private static string NormalizeReply(string value)
    {
        var lines = value
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n')
            .Select(line => line.TrimEnd())
            .ToList();

        while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[0]))
        {
            lines.RemoveAt(0);
        }

        while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[^1]))
        {
            lines.RemoveAt(lines.Count - 1);
        }

        return string.Join(Environment.NewLine, lines).Trim();
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

    private static bool ContainsUnsupportedContent(string value)
    {
        return value.Contains("http://", StringComparison.OrdinalIgnoreCase)
            || value.Contains("https://", StringComparison.OrdinalIgnoreCase)
            || value.Contains("sourceId", StringComparison.OrdinalIgnoreCase)
            || value.Contains("knowledge base id", StringComparison.OrdinalIgnoreCase)
            || value.Contains("webview", StringComparison.OrdinalIgnoreCase)
            || value.Contains("api key", StringComparison.OrdinalIgnoreCase)
            || value.Contains("password", StringComparison.OrdinalIgnoreCase)
            || value.Contains("authentication code", StringComparison.OrdinalIgnoreCase);
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

    private static bool ContainsAny(string value, params string[] phrases)
    {
        return phrases.Any(
            phrase => value.Contains(
                phrase,
                StringComparison.OrdinalIgnoreCase));
    }
}
