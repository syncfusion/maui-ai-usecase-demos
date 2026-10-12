using Todo.Models.DTOs;
using Todo.Utilities;

namespace Todo.Services.Interfaces;

/// <summary>
/// Contract for the task CRUD service.  Implementations are added in
/// Module 4 (Task 4-xx).
/// </summary>
public interface ITaskService
{
    /// <summary>
    /// Returns every task for the current user, optionally filtered by list
    /// and/or category.
    /// </summary>
    /// <param name="listId">Optional list identifier to filter by.</param>
    /// <param name="category">Optional category to filter by.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> whose <see cref="Result{T}.Value"/> is a
    /// read-only list of <see cref="TaskDto"/> objects.
    /// </returns>
    Task<Result<IReadOnlyList<TaskDto>>> GetTasksAsync(
        string? listId = null,
        Enums.ListCategory? category = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the task with the supplied identifier.
    /// </summary>
    /// <param name="taskId">The unique task identifier.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> whose <see cref="Result{T}.Value"/> is the
    /// matching <see cref="TaskDto"/> on success.
    /// </returns>
    Task<Result<TaskDto>> GetTaskByIdAsync(
        string taskId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new task.
    /// </summary>
    /// <param name="task">The task to create.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> whose <see cref="Result{T}.Value"/> is the
    /// created <see cref="TaskDto"/> (with assigned identifier).
    /// </returns>
    Task<Result<TaskDto>> CreateTaskAsync(
        TaskDto task,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing task.
    /// </summary>
    /// <param name="task">The task to update (must have an existing identifier).</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A <see cref="Result{T}"/> whose <see cref="Result{T}.Value"/> is the updated <see cref="TaskDto"/>.</returns>
    Task<Result<TaskDto>> UpdateTaskAsync(
        TaskDto task,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a task as deleted (soft delete; recoverable from the Bin).
    /// </summary>
    /// <param name="taskId">The unique task identifier.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A <see cref="Result"/> describing the outcome.</returns>
    Task<Result> DeleteTaskAsync(
        string taskId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Toggles the completion state of the supplied task.
    /// </summary>
    /// <param name="taskId">The unique task identifier.</param>
    /// <param name="isCompleted">The new completion state.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A <see cref="Result{T}"/> whose <see cref="Result{T}.Value"/> is the updated <see cref="TaskDto"/>.</returns>
    Task<Result<TaskDto>> SetTaskCompletionAsync(
        string taskId,
        bool isCompleted,
        CancellationToken cancellationToken = default);
}
