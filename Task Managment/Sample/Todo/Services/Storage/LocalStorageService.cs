using Microsoft.Extensions.Logging;
using Todo.Services.Interfaces;
using Todo.Utilities;

namespace Todo.Services.Storage;

/// <summary>
/// Placeholder implementation of <see cref="ILocalStorageService"/>.  The
/// SQLite-backed implementation is added in Task 1-07 (Database Setup).
/// This skeleton simply returns "not initialised" results so the DI container
/// can resolve the service today.
/// </summary>
public sealed class LocalStorageService : ILocalStorageService
{
    private readonly ILogger<LocalStorageService> _logger;

    /// <summary>
    /// Initialises a new instance of the <see cref="LocalStorageService"/>
    /// class.
    /// </summary>
    /// <param name="logger">Logger used for structured diagnostics.</param>
    public LocalStorageService(ILogger<LocalStorageService> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<Result> InitializeAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogWarning(
            "LocalStorageService.InitializeAsync is not implemented yet (Task 1-07).");
        throw new NotImplementedException(
            "Local storage is implemented in Task 1-07 (Database Setup).");
    }

    /// <inheritdoc />
    public Task<Result<string?>> GetAsync(
        string key,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException("Implemented in Task 1-07.");

    /// <inheritdoc />
    public Task<Result> SetAsync(
        string key,
        string? value,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException("Implemented in Task 1-07.");

    /// <inheritdoc />
    public Task<Result> RemoveAsync(
        string key,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException("Implemented in Task 1-07.");
}
