using System.Text.Json.Serialization;

namespace NutriLens.Models;

/// <summary>One persisted daily-insight entry stored in Preferences.</summary>
public sealed class DailyInsightCacheEntry
{
    [JsonPropertyName("insightText")]
    public string InsightText { get; set; } = string.Empty;

    [JsonPropertyName("generatedOnUtc")]
    public DateTime GeneratedOnUtc { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("profileKey")]
    public string ProfileKey { get; set; } = "default";

    [JsonPropertyName("source")]
    public string Source { get; set; } = "ai";
}