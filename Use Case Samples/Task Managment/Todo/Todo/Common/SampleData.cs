using Todo.Features.Notes.Models;

namespace Todo.Common;

/// <summary>
/// Provides the in-memory seed notes used by the sample at first launch.
///
/// <para>
/// The data set is laid out so that each built-in filter view
/// (<b>Important</b>, <b>Reminder</b>, <b>Bin</b>) shows a distinct
/// list of notes whose size and content match the design images
/// <c>images/important.png</c>, <c>images/remainder.png</c> and
/// <c>images/bin.png</c>:
/// </para>
/// <list type="bullet">
///   <item><b>Important</b> shows 5 starred notes (no reminder, not soft-deleted).</item>
///   <item><b>Reminder</b> shows 3 notes that have a reminder date (not starred, not soft-deleted).</item>
///   <item><b>Bin</b> shows 3 soft-deleted notes (not starred, no reminder).</item>
///   <item><b>My Notes</b> is empty at first launch, matching <c>images/mynotes.png</c>.</item>
/// </list>
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
                // ---------------------------------------------------
                //  Important (5 starred notes — match important.png)
                // ---------------------------------------------------
                new()
                {
                    Title = "Prepare project report",
                    DueDate = new DateTime(2026, 2, 18),
                    IsImportant = true,
                    DisplayOrder = 0,
                    CreatedAt = now.AddDays(-7),
                    UpdatedAt = now.AddDays(-1)
                },
                new()
                {
                    Title = "Review client requirements",
                    IsImportant = true,
                    DisplayOrder = 1,
                    CreatedAt = now.AddDays(-5),
                    UpdatedAt = now.AddDays(-1)
                },
                new()
                {
                    Title = "Complete pending design feedback",
                    DueDate = new DateTime(2026, 2, 18),
                    IsImportant = true,
                    DisplayOrder = 2,
                    CreatedAt = now.AddDays(-4),
                    UpdatedAt = now.AddDays(-1)
                },
                new()
                {
                    Title = "Attend project stand-up",
                    DueDate = new DateTime(2026, 2, 18),
                    IsImportant = true,
                    DisplayOrder = 3,
                    CreatedAt = now.AddDays(-3),
                    UpdatedAt = now.AddDays(-1)
                },
                new()
                {
                    Title = "Pay electricity and internet bills",
                    DueDate = new DateTime(2026, 2, 18),
                    IsImportant = true,
                    DisplayOrder = 4,
                    CreatedAt = now.AddDays(-3),
                    UpdatedAt = now.AddDays(-1)
                },

                // ---------------------------------------------------
                //  Reminder (3 notes with a reminder date — match remainder.png)
                // ---------------------------------------------------
                new()
                {
                    Title = "Go for a 30-minute walk",
                    DueDate = new DateTime(2026, 2, 18),
                    ReminderDate = new DateTime(2026, 2, 18, 10, 0, 0),
                    DisplayOrder = 5,
                    CreatedAt = now.AddDays(-2),
                    UpdatedAt = now.AddDays(-1)
                },
                new()
                {
                    Title = "Update project status report",
                    DueDate = new DateTime(2026, 2, 18),
                    ReminderDate = new DateTime(2026, 2, 18, 10, 0, 0),
                    DisplayOrder = 6,
                    CreatedAt = now.AddDays(-2),
                    UpdatedAt = now.AddDays(-1)
                },
                new()
                {
                    Title = "Schedule doctor or dentist appointment",
                    DueDate = new DateTime(2026, 2, 18),
                    ReminderDate = new DateTime(2026, 2, 18, 10, 0, 0),
                    DisplayOrder = 7,
                    CreatedAt = now.AddDays(-2),
                    UpdatedAt = now.AddDays(-1)
                },

                // ---------------------------------------------------
                //  Bin (3 soft-deleted notes — match bin.png)
                // ---------------------------------------------------
                new()
                {
                    Title = "Read 20 pages of a book",
                    DisplayOrder = 8,
                    CreatedAt = now.AddDays(-3),
                    UpdatedAt = now.AddDays(-2)
                },
                new()
                {
                    Title = "Buy groceries for the week",
                    DisplayOrder = 9,
                    CreatedAt = now.AddDays(-3),
                    UpdatedAt = now.AddDays(-2)
                },
                new()
                {
                    Title = "Water indoor plants",
                    DisplayOrder = 10,
                    CreatedAt = now.AddDays(-4),
                    UpdatedAt = now.AddDays(-2)
                }
            };

            // Each note receives a fresh GUID so the in-memory store
            // is independent for every consumer.
            var softDeleteTimestamp = now.AddDays(-1);
            foreach (var note in _seeded)
            {
                note.Id = Guid.NewGuid().ToString();
                // Only the Bin bucket is soft-deleted; Important and
                // Reminder notes stay live so they are not visible in
                // the Bin filter (which only shows DeletedAt != null).
                if (note.DisplayOrder >= 8)
                {
                    note.DeletedAt = softDeleteTimestamp;
                }
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
