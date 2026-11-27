namespace MauiNotes.Models.AI;

public sealed record AiEditingRequest(
    AiEditingAction Action,
    string Html,
    string CustomInstruction);
