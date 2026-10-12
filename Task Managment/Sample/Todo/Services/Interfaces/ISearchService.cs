using Todo.Models.DTOs;
using Todo.Utilities;

namespace Todo.Services.Interfaces;

/// <summary>
/// Contract for the search and filtering service.  Implementations are added
/// in Module 7 (Task 7-xx).
/// </summary>
public interface ISearchService
{
    /// <summary>
    /// Performs a full-text search across the current user's tasks and lists.
    /// </summary>
    /// <param name="query">The user-supplied query string.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> whose <see cref="Result{T}.Value"/> is a
    /// <see cref="SearchResultDto"/> containing the matching tasks and lists.
    /// </returns>
    Task<Result<SearchResultDto>> SearchAsync(
        string query,
        CancellationToken cancellationToken = default);
}
