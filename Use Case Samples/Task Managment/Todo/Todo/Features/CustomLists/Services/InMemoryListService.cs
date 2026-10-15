using System.Collections.ObjectModel;
using Todo.Features.CustomLists.Models;
using Todo.Features.Notes.Services;

namespace Todo.Features.CustomLists.Services;

/// <summary>
/// Singleton in-memory implementation of <see cref="IListService"/>.
/// Keeps a single <see cref="ObservableCollection{T}"/> of user-defined
/// lists, ordered by <see cref="NoteList.DisplayOrder"/>. When a list
/// is deleted, any notes that referenced it are returned to
/// <c>My Notes</c> by clearing their <c>ListId</c> through the
/// shared <see cref="INoteService"/>.
/// </summary>
public sealed class InMemoryListService : IListService
{
    private readonly ObservableCollection<NoteList> _lists;
    private readonly INoteService _noteService;

    public InMemoryListService(INoteService noteService)
    {
        _noteService = noteService;
        _lists = new ObservableCollection<NoteList>();
        Lists = new ReadOnlyObservableCollection<NoteList>(_lists);
    }

    public ReadOnlyObservableCollection<NoteList> Lists { get; }

    public Task<NoteList> CreateAsync(string name)
    {
        var trimmed = (name ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(trimmed))
            throw new ArgumentException("List name is required.", nameof(name));
        if (NameExists(trimmed))
            throw new ArgumentException($"A list named '{trimmed}' already exists.", nameof(name));

        var nextOrder = _lists.Count == 0 ? 0 : _lists.Max(l => l.DisplayOrder) + 1;
        var list = new NoteList
        {
            Id = Guid.NewGuid().ToString(),
            Name = trimmed,
            IsSystem = false,
            DisplayOrder = nextOrder,
            CreatedAt = DateTime.Now
        };
        _lists.Add(list);
        return Task.FromResult(list);
    }

    public Task RenameAsync(string id, string newName)
    {
        var trimmed = (newName ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(trimmed))
            throw new ArgumentException("List name is required.", nameof(newName));

        var existing = _lists.FirstOrDefault(l => l.Id == id)
            ?? throw new KeyNotFoundException($"List '{id}' was not found.");

        if (NameExists(trimmed, excludeId: id))
            throw new ArgumentException($"A list named '{trimmed}' already exists.", nameof(newName));

        existing.Name = trimmed;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string id)
    {
        var existing = _lists.FirstOrDefault(l => l.Id == id);
        if (existing is null) return Task.CompletedTask;

        _lists.Remove(existing);

        // Re-home any notes that were in this list back to My Notes so
        // the user doesn't lose them (per spec §7.5: deleting a list
        // removes the list, not its notes).
        foreach (var note in _noteService.Notes)
        {
            if (note.ListId == id)
                note.ListId = null;
        }

        return Task.CompletedTask;
    }

    public bool NameExists(string name, string? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        var comparison = StringComparison.OrdinalIgnoreCase;
        return _lists.Any(l => l.Id != excludeId
                                && string.Equals(l.Name, name.Trim(), comparison));
    }
}