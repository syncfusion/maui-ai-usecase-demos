using Todo.Features.Notes.Models;

namespace Todo.Common;

/// <summary>
/// Provides the in-memory seed notes used by the sample at first launch.
/// The list is intentionally small so the layout and item template are
/// easy to inspect during design review.
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

            var now = DateTime.Now;
            _seeded = new List<Note>
            {
                new()
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = "Review today's emails and respond to priority items",
                    DueDate = now.Date,
                    ReminderDate = now.Date.AddHours(9),
                    IsImportant = true,
                    DisplayOrder = 0,
                    CreatedAt = now.AddDays(-2),
                    UpdatedAt = now.AddDays(-1)
                },
                new()
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = "Review project requirements with the design team",
                    DueDate = now.Date.AddDays(1),
                    ReminderDate = now.Date.AddDays(1).AddHours(10),
                    IsImportant = false,
                    DisplayOrder = 1,
                    CreatedAt = now.AddDays(-3),
                    UpdatedAt = now.AddDays(-1)
                },
                new()
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = "Pay electricity and water bills",
                    DueDate = now.Date.AddDays(2),
                    IsImportant = true,
                    DisplayOrder = 2,
                    CreatedAt = now.AddDays(-5),
                    UpdatedAt = now.AddDays(-2)
                },
                new()
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = "Plan weekend trip itinerary",
                    DueDate = now.Date.AddDays(4),
                    ReminderDate = now.Date.AddDays(4).AddHours(8),
                    IsImportant = false,
                    DisplayOrder = 3,
                    CreatedAt = now.AddDays(-1),
                    UpdatedAt = now.AddDays(-1)
                },
                new()
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = "Call dentist to reschedule next appointment",
                    DueDate = now.Date.AddDays(3),
                    IsImportant = false,
                    DisplayOrder = 4,
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new()
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = "Read chapter 4 of the architecture book",
                    IsImportant = false,
                    DisplayOrder = 5,
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new()
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = "Refactor the sample data service to be DI-friendly",
                    DueDate = now.Date.AddDays(-1),
                    IsImportant = true,
                    IsCompleted = true,
                    DisplayOrder = 6,
                    CreatedAt = now.AddDays(-7),
                    UpdatedAt = now.AddDays(-1)
                }
            };

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