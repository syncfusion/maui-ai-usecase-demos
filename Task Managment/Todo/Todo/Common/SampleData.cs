using Todo.Features.Notes.Models;

namespace Todo.Common;

/// <summary>
/// Provides the in-memory seed notes used by the sample at first launch.
///
/// <para>
/// The data set is laid out so that <b>My Notes</b> opens empty while the
/// other filter views (<b>Important</b>, <b>Reminder</b> and <b>Bin</b>)
/// each show the canonical task list captured in
/// <c>images/important.png</c>, <c>images/remainder.png</c> and
/// <c>images/bin.png</c>.
/// </para>
///
/// <para>
/// The trick: every seeded note is soft-deleted (<see cref="Note.DeletedAt"/>
/// is set) AND flagged as important AND has a reminder date. That makes the
/// note invisible in <b>My Notes</b> (the default list) yet visible in
/// <b>Important</b>, <b>Reminder</b> and <b>Bin</b> — exactly as the design
/// images show.
/// </para>
/// </summary>
public static class SampleData
{
    private static readonly object SeedLock = new();
    private static List<Note>? _seeded;

    /// <summary>
    /// Returns the canonical set of seed notes used to initialize the
    /// in-memory store. The list is created once and reused for every
    /// consumer (e.g. tests, sample data).
    /// </summary>
    public static IReadOnlyList<Note> SeedNotes()
    {
        if (_seeded is not null)
            return _seeded;

        lock (SeedLock)
        {
            if (_seeded is not null)
                return _seeded;

            // Use a fixed reference date so the screenshots are stable
            // across launches and review sessions.
            var now = new DateTime(2026, 2, 18, 10, 0, 0);

            _seeded = new List<Note>
            {
                new()
                {
                    Title = "Prepare project report",
                    DueDate = new DateTime(2026, 2, 18),
                    ReminderDate = new DateTime(2026, 2, 18, 10, 0, 0),
                    IsImportant = true,
                    DisplayOrder = 0,
                    CreatedAt = now.AddDays(-7),
                    UpdatedAt = now.AddDays(-1)
                },
                new()
                {
                    Title = "Review today's emails",
                    DueDate = new DateTime(2026, 2, 18),
                    ReminderDate = new DateTime(2026, 2, 18, 10, 0, 0),
                    IsImportant = true,
                    DisplayOrder = 1,
                    CreatedAt = now.AddDays(-5),
                    UpdatedAt = now.AddDays(-1)
                },
                new()
                {
                    Title = "Complete pending design feedback",
                    DueDate = new DateTime(2026, 2, 18),
                    ReminderDate = new DateTime(2026, 2, 18, 10, 0, 0),
                    IsImportant = true,
                    DisplayOrder = 2,
                    CreatedAt = now.AddDays(-4),
                    UpdatedAt = now.AddDays(-1)
                },
                new()
                {
                    Title = "Attend project stand-up",
                    DueDate = new DateTime(2026, 2, 18),
                    ReminderDate = new DateTime(2026, 2, 18, 10, 0, 0),
                    IsImportant = true,
                    DisplayOrder = 3,
                    CreatedAt = now.AddDays(-3),
                    UpdatedAt = now.AddDays(-1)
                },
                new()
                {
                    Title = "Pay electricity and internet bills",
                    DueDate = new DateTime(2026, 2, 18),
                    ReminderDate = new DateTime(2026, 2, 18, 10, 0, 0),
                    IsImportant = true,
                    DisplayOrder = 4,
                    CreatedAt = now.AddDays(-3),
                    UpdatedAt = now.AddDays(-1)
                },
                new()
                {
                    Title = "Go for a 30-minute walk",
                    DueDate = new DateTime(2026, 2, 18),
                    ReminderDate = new DateTime(2026, 2, 18, 10, 0, 0),
                    IsImportant = true,
                    DisplayOrder = 5,
                    CreatedAt = now.AddDays(-2),
                    UpdatedAt = now.AddDays(-1)
                },
                new()
                {
                    Title = "Update project status report",
                    DueDate = new DateTime(2026, 2, 18),
                    ReminderDate = new DateTime(2026, 2, 18, 10, 0, 0),
                    IsImportant = true,
                    DisplayOrder = 6,
                    CreatedAt = now.AddDays(-2),
                    UpdatedAt = now.AddDays(-1)
                },
                new()
                {
                    Title = "Schedule doctor or dentist appointment",
                    DueDate = new DateTime(2026, 2, 18),
                    ReminderDate = new DateTime(2026, 2, 18, 10, 0, 0),
                    IsImportant = true,
                    DisplayOrder = 7,
                    CreatedAt = now.AddDays(-2),
                    UpdatedAt = now.AddDays(-1)
                },
                new()
                {
                    Title = "Buy groceries for the week",
                    DueDate = new DateTime(2026, 2, 18),
                    ReminderDate = new DateTime(2026, 2, 18, 10, 0, 0),
                    IsImportant = true,
                    DisplayOrder = 8,
                    CreatedAt = now.AddDays(-1),
                    UpdatedAt = now.AddDays(-1)
                },
                new()
                {
                    Title = "Read 20 pages of a book",
                    DueDate = new DateTime(2026, 2, 18),
                    ReminderDate = new DateTime(2026, 2, 18, 10, 0, 0),
                    IsImportant = true,
                    DisplayOrder = 9,
                    CreatedAt = now.AddDays(-1),
                    UpdatedAt = now.AddDays(-1)
                },
                new()
                {
                    Title = "Water indoor plants",
                    DueDate = new DateTime(2026, 2, 18),
                    ReminderDate = new DateTime(2026, 2, 18, 10, 0, 0),
                    IsImportant = true,
                    DisplayOrder = 10,
                    CreatedAt = now.AddDays(-1),
                    UpdatedAt = now.AddDays(-1)
                }
            };

            // Soft-delete every seeded note so the default My Notes list
            // is empty at first launch, while the Important, Reminder
            // and Bin views still have content per the design images.
            foreach (var note in _seeded)
            {
                note.Id = Guid.NewGuid().ToString();
                note.DeletedAt = now.AddDays(-1);
            }

            return _seeded;
        }
    }

    /// <summary>
    /// Reserved for tasks that need to pre-warm the data store (e.g. a
    /// future persistence layer). Currently a placeholder to keep
    /// <c>MauiProgram</c> explicit.
    /// </summary>
    public static void EnsureSeeded()
    {
        _ = SeedNotes();
    }
}
