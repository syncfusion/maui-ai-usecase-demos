namespace Todo.Features.Notes.Models;

/// <summary>
/// Recurrence options for a note. The default is
/// <see cref="None"/>. Other values drive the "Repeat" picker in the
/// Edit Note dialog and the repeat chip on the note card.
/// </summary>
public enum RepeatType
{
    None,
    Daily,
    Weekly,
    Monthly,
    Yearly
}
