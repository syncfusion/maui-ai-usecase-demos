using Microsoft.Extensions.Logging;
using Todo.Models.DTOs;
using Todo.Services.Interfaces;
using Todo.Utilities;

namespace Todo.Services.Task;

/// <summary>
/// Placeholder implementation of <see cref="ITaskService"/>.  The real
/// implementation is added in Module 4 (Task 4-xx).  This skeleton exists so
/// the DI container can resolve the service in Task 1-05 and downstream
/// modules can compile against the full interface surface.
/// </summary>
public sealed class TaskService : ITaskService
{
    private readonly ILogger<TaskService> _logger;

    /// <summary>
    /// Initialises a new instance of the <see cref="TaskService"/> class.
    /// </summary>
    /// <param name="logger">Logger used for structured diagnostics.</param>
    public TaskService(ILogger<TaskService> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<Result<IReadOnlyList<TaskDto>>> GetTasksAsync(
        string? listId = null,
        Enums.ListCategory? category = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning(
            "TaskService.GetTasksAsync is not implemented yet (Module 4).");
        throw new NotImplementedException(
            "Task CRUD is implemented in Module 4 (Task 4-xx).");
    }

    /// <inheritdoc />
    public Task<Result<TaskDto>> GetTaskByIdAsync(
        string taskId,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException("Implemented in Module 4.");

    /// <inheritdoc />
    public Task<Result<TaskDto>> CreateTaskAsync(
        TaskDto task,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException("Implemented in Module 4.");

    /// <inheritdoc />
    public Task<Result<TaskDto>> UpdateTaskAsync(
        TaskDto task,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException("Implemented in Module 4.");

    /// <inheritdoc />
    public Task<Result> DeleteTaskAsync(
        string taskId,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException("Implemented in Module 4.");

    /// <inheritdoc />
    public Task<Result<TaskDto>> SetTaskCompletionAsync(
        string taskId,
        bool isCompleted,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException("Implemented in Module 4.");
}
