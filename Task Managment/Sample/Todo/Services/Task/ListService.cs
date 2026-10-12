using Microsoft.Extensions.Logging;
using Todo.Models.DTOs;
using Todo.Services.Interfaces;
using Todo.Utilities;

namespace Todo.Services.TaskService;

// ListService lives in the same Services/Task folder as TaskService because
// lists and tasks are managed together.  This is purely a folder convention
// — it is resolved through the IListService interface in the DI container.

/// <summary>
/// Placeholder implementation of <see cref="IListService"/>.  The real
/// implementation is added in Module 5 (Task 5-xx).
/// </summary>
public sealed class ListService : IListService
{
    private readonly ILogger<ListService> _logger;

    /// <summary>
    /// Initialises a new instance of the <see cref="ListService"/> class.
    /// </summary>
    /// <param name="logger">Logger used for structured diagnostics.</param>
    public ListService(ILogger<ListService> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<Result<IReadOnlyList<ListDto>>> GetListsAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning(
            "ListService.GetListsAsync is not implemented yet (Module 5).");
        throw new NotImplementedException(
            "List CRUD is implemented in Module 5 (Task 5-xx).");
    }

    /// <inheritdoc />
    public Task<Result<ListDto>> CreateListAsync(
        ListDto list,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException("Implemented in Module 5.");

    /// <inheritdoc />
    public Task<Result<ListDto>> UpdateListAsync(
        ListDto list,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException("Implemented in Module 5.");

    /// <inheritdoc />
    public Task<Result> DeleteListAsync(
        string listId,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException("Implemented in Module 5.");
}
