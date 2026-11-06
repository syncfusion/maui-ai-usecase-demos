namespace AIChatSample;

/// <summary>
/// Lightweight proposal returned by the AI for the agent-creation
/// copilot. Captures the three fields the configure page needs to be
/// pre-filled with so the user only has to pick a logo / color and
/// press Create.
/// </summary>
public sealed class AgentDraft
{
    /// <summary>Short display name for the new agent (≤ 60 chars).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>One-sentence description that surfaces on the agent card.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>System-prompt style instructions the configure page seeds the long-form field with.</summary>
    public string Instructions { get; set; } = string.Empty;
}
