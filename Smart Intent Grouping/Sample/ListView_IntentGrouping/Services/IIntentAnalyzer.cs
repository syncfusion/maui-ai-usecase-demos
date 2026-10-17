using ListView_IntentGrouping.Models;

namespace ListView_IntentGrouping.Services;

/// <summary>
/// Identifies which AI strategy is currently powering the analyzer.
/// Surfaced in the UI so the user can see whether the analysis came
/// from Azure OpenAI or the bundled rule-based fallback.
/// </summary>
public enum IntentAnalyzerMode
{
    /// <summary>
    /// The bundled local rule-based analyzer. Always available, no
    /// network calls. Used as the fallback when Azure OpenAI
    /// configuration is missing or invalid.
    /// </summary>
    Local,

    /// <summary>
    /// Azure OpenAI chat completion. Requires a valid endpoint, key,
    /// and deployment to be configured in <c>MauiProgram</c>.
    /// </summary>
    AzureOpenAI,
}

/// <summary>
/// Abstraction over the AI analyzer so the view-model can run the
/// same <c>AnalyzeAsync</c> code path regardless of whether the
/// configuration points at Azure OpenAI or the bundled local
/// rule-based fallback.
/// </summary>
public interface IIntentAnalyzer
{
    /// <summary>
    /// The active analyzer strategy. The view-model exposes this
    /// through <c>AnalyzerMode</c> so the UI can show a status
    /// banner.
    /// </summary>
    IntentAnalyzerMode Mode { get; }

    /// <summary>
    /// A short human-readable label for the active strategy
    /// (e.g. "Azure OpenAI (gpt-4o)" or "Local rule-based").
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Run the analyzer on the supplied items and produce an
    /// <see cref="IntentAnalysisResult"/>. Implementations are
    /// expected to be thread-safe and to mutate only the items
    /// handed to them (never the source dataset).
    /// </summary>
    Task<IntentAnalysisResult> AnalyzeAsync(
        string prompt,
        string domain,
        IEnumerable<FeedbackItem> items,
        CancellationToken cancellationToken = default);
}
