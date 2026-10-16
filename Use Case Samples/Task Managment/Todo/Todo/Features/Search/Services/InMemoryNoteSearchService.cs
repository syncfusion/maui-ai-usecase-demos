using Todo.Features.Notes.Models;
using Todo.Features.Notes.Services;

namespace Todo.Features.Search.Services;

/// <summary>
/// In-memory implementation of <see cref="INoteSearchService"/>. The
/// service observes the shared <see cref="INoteService.Notes"/>
/// collection so that any create / edit / delete flows performed while
/// the Search page is open are reflected on the next query.
/// </summary>
public sealed class InMemoryNoteSearchService : INoteSearchService
{
    private readonly INoteService _noteService;

    public InMemoryNoteSearchService(INoteService noteService)
    {
        _noteService = noteService;
    }

    public IReadOnlyList<Note> Search(string? query)
    {
        var trimmed = (query ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(trimmed))
            return Array.Empty<Note>();

        return _noteService.Notes
            .Where(n => n.Title is not null
                        && n.Title.Contains(trimmed, StringComparison.OrdinalIgnoreCase))
            .OrderBy(n => n.IsCompleted)
            .ThenBy(n => n.DisplayOrder)
            .ToList();
    }
}
