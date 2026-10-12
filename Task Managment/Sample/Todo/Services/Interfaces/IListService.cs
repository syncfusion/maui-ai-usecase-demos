using Todo.Models.DTOs;
using Todo.Utilities;

namespace Todo.Services.Interfaces;

/// <summary>
/// Contract for the list management service.  Implementations are added in
/// Module 5 (Task 5-xx).
/// </summary>
public interface IListService
{
    /// <summary>
    /// Returns every list owned by the current user.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> whose <see cref="Result{T}.Value"/> is a
    /// read-only list of <see cref="ListDto"/> objects.
    /// </returns>
    Task<Result<IReadOnlyList<ListDto>>> GetListsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new list.
    /// </summary>
    /// <param name="list">The list to create.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> whose <see cref="Result{T}.Value"/> is the
    /// created <see cref="ListDto"/> (with assigned identifier).
    /// </returns>
    Task<Result<ListDto>> CreateListAsync(
        ListDto list,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing list.
    /// </summary>
    /// <param name="list">The list to update (must have an existing identifier).</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A <see cref="Result{T}"/> whose <see cref="Result{T}.Value"/> is the updated <see cref="ListDto"/>.</returns>
    Task<Result<ListDto>> UpdateListAsync(
        ListDto list,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a list.  Tasks that belong to the list are kept but unlinked.
    /// </summary>
    /// <param name="listId">The unique list identifier.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A <see cref="Result"/> describing the outcome.</returns>
    Task<Result> DeleteListAsync(
        string listId,
        CancellationToken cancellationToken = default);
}
