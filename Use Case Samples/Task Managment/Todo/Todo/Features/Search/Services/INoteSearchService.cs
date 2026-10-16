using Todo.Features.Notes.Models;

namespace Todo.Features.Search.Services;

/// <summary>
/// Abstraction over the in-memory title search used by the Search
/// feature (Module-06). The implementation is intentionally minimal:
/// a case-insensitive substring match against the note title. The
/// service never mutates the underlying store.
/// </summary>
public interface INoteSearchService
{
    /// <summary>
    /// Returns every note whose <see cref="Note.Title"/> contains the
    /// supplied <paramref name="query"/> as a case-insensitive
    /// substring. A blank or whitespace-only query returns an empty
    /// list (the caller should treat that as the empty state per
    /// <c>search.png</c>).
    /// </summary>
    IReadOnlyList<Note> Search(string? query);
}
