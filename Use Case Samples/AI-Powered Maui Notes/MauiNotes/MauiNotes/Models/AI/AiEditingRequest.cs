namespace MauiNotes.Models.AI;

public sealed record AiEditingRequest(
    AiEditingAction Action,
    string Text,
    string Html,
    string CustomInstruction);
