namespace AIChatSample;

/// <summary>
/// Stable identifiers for the agents that ship with the app. Used so the
/// navigation-drawer <see cref="AgentItem"/> rows (which only carry a
/// <c>Name</c>) can be matched back to the rich <see cref="AgentConfiguration"/>
/// (or default <see cref="Syncfusion.Maui.AIAssistView.AssistAgent"/>) in
/// the AIAssistView selector.
/// </summary>
public static class DefaultAgentIds
{
    /// <summary>Identifier of the default "Code Reviewer" agent.</summary>
    public const string CodeReviewer = "default-agent-code-reviewer";

    /// <summary>Identifier of the default "Social Media Writer" agent.</summary>
    public const string SocialMediaWriter = "default-agent-social-media-writer";

    /// <summary>Identifier of the default "Meeting Notes Assistant" agent.</summary>
    public const string MeetingNotesAssistant = "default-agent-meeting-notes-assistant";
}
