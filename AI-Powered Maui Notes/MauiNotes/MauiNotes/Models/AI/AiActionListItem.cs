namespace MauiNotes.Models.AI;

public sealed record AiActionListItem(
    AiEditingAction Action,
    string Title,
    string Icon,
    bool ShowDividerBefore = false);
