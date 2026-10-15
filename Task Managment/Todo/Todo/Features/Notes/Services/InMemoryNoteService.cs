using System.Collections.ObjectModel;
using Todo.Common;
using Todo.Features.Notes.Models;

namespace Todo.Features.Notes.Services;

/// <summary>
/// Singleton in-memory implementation of <see cref="INoteService"/>.
/// Backed by a single <see cref="ObservableCollection{T}"/> so all
/// subscribed UI automatically reflects changes.
/// </summary>
public sealed class InMemoryNoteService : INoteService
{
    private readonly ObservableCollection<Note> _notes;

    public InMemoryNoteService()
    {
        // Per the design (mynotes.png vs important.png / remainder.png /
        // bin.png), the My Notes list opens empty at first launch while the
        // Important, Reminder and Bin filters still display the canonical
        // tasks from the reference images.
        //
        // SampleData.SeedNotes() produces that exact set: every note is
        // soft-deleted, flagged important and has a reminder date set — so
        // it is hidden from My Notes yet visible in the other three views.
        var backing = new ObservableCollection<Note>();
        foreach (var seed in SampleData.SeedNotes())
            backing.Add(seed);

        _notes = backing;
        Notes = new ReadOnlyObservableCollection<Note>(_notes);
    }

    public ReadOnlyObservableCollection<Note> Notes { get; }

    public Task<Note> CreateAsync(string title, DateTime? dueDate = null, DateTime? reminderDate = null)
    {
        var now = DateTime.Now;
        var nextOrder = _notes.Count == 0 ? 0 : _notes.Max(n => n.DisplayOrder) + 1;
        var note = new Note
        {
            Id = Guid.NewGuid().ToString(),
            Title = (title ?? string.Empty).Trim(),
            DueDate = dueDate,
            ReminderDate = reminderDate,
            DisplayOrder = nextOrder,
            CreatedAt = now,
            UpdatedAt = now
        };
        _notes.Insert(0, note);
        return Task.FromResult(note);
    }

    public Task UpdateAsync(Note note)
    {
        note.UpdatedAt = DateTime.Now;
        // ObservableCollection replaces items in-place; binding systems
        // observe property-level notifications on Note itself.
        return Task.CompletedTask;
    }

    public Task SoftDeleteAsync(string id)
    {
        var existing = _notes.FirstOrDefault(n => n.Id == id);
        if (existing is null)
            return Task.CompletedTask;

        existing.DeletedAt = DateTime.Now;
        existing.UpdatedAt = DateTime.Now;
        return Task.CompletedTask;
    }

    public Task RestoreAsync(string id)
    {
        var existing = _notes.FirstOrDefault(n => n.Id == id);
        if (existing is null)
            return Task.CompletedTask;

        existing.DeletedAt = null;
        existing.UpdatedAt = DateTime.Now;
        return Task.CompletedTask;
    }

    public Task DeletePermanentlyAsync(string id)
    {
        var existing = _notes.FirstOrDefault(n => n.Id == id);
        if (existing is not null)
            _notes.Remove(existing);
        return Task.CompletedTask;
    }

    public Task ToggleCompleteAsync(string id)
    {
        var existing = _notes.FirstOrDefault(n => n.Id == id);
        if (existing is null)
            return Task.CompletedTask;

        existing.IsCompleted = !existing.IsCompleted;
        existing.UpdatedAt = DateTime.Now;
        return Task.CompletedTask;
    }

    public Task ToggleImportantAsync(string id)
    {
        var existing = _notes.FirstOrDefault(n => n.Id == id);
        if (existing is null)
            return Task.CompletedTask;

        existing.IsImportant = !existing.IsImportant;
        existing.UpdatedAt = DateTime.Now;
        return Task.CompletedTask;
    }

    public IReadOnlyList<Note> GetForCategory(NotesListCategory category)
    {
        IEnumerable<Note> query = _notes;
        query = category switch
        {
            // Per the design (mynotes.png), My Notes is the active list
            // and only shows notes that are not soft-deleted.
            NotesListCategory.MyNotes => query.Where(n => n.DeletedAt is null),

            // Per the design (important.png), Important shows every note
            // that has been starred – including ones that have since been
            // moved to the Bin so users can still find them via the star.
            NotesListCategory.Important => query.Where(n => n.IsImportant),

            // Per the design (remainder.png), Reminder shows every note
            // that has a reminder date set, regardless of soft-delete.
            NotesListCategory.Reminder => query.Where(n => n.ReminderDate.HasValue),

            // Per the design (bin.png), Bin shows every soft-deleted note.
            NotesListCategory.Bin => query.Where(n => n.DeletedAt is not null),
            _ => query
        };

        return query
            .OrderBy(n => n.IsCompleted)
            .ThenBy(n => n.DisplayOrder)
            .ToList();
    }
}