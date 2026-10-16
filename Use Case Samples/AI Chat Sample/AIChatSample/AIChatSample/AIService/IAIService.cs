namespace AIChatSample;

/// <summary>
/// Single seam between the UI and any AI provider (NFR-2).
/// </summary>
public interface IAIService
{
    /// <summary>
    /// Sends a user prompt to the AI provider. The optional
    /// <paramref name="systemContext"/> is used as the system prompt
    /// (e.g. an agent's <c>Behaviour</c> — FR-0.3).
    /// </summary>
    /// <returns>
    /// A <see cref="ContextSuggestionResponse"/> with the assistant's
    /// answer and any follow-up suggestions, or <c>null</c> if the
    /// request was cancelled.
    /// </returns>
    Task<ContextSuggestionResponse?> GetResponseAsync(
        string prompt,
        string? systemContext = null,
        CancellationToken ct = default);

    /// <summary>
    /// Drafts a specialist agent from a free-form prompt. The AI is
    /// expected to return a structured <see cref="AgentDraft"/> with a
    /// short name, one-line description, and the long-form system
    /// instructions the agent should run with. Returns <c>null</c> if
    /// cancelled or no useful draft could be produced.
    /// </summary>
    Task<AgentDraft?> DraftAgentAsync(string prompt, CancellationToken ct = default);
}
