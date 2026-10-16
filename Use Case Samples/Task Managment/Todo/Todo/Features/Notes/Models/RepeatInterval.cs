namespace Todo.Features.Notes.Models;

/// <summary>
/// Recurrence cadence for a note's reminder / due date. Mirrors the
/// Microsoft To Do repeat picker (Daily, Weekly, Monthly, Yearly).
/// </summary>
public enum RepeatInterval
{
    None,
    Daily,
    Weekly,
    Monthly,
    Yearly
}
