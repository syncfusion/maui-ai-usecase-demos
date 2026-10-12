using Todo.Utilities;

namespace Todo.Services.Interfaces;

/// <summary>
/// Contract for the local key-value storage service backed by the SQLite
/// database created in Task 1-07.
/// </summary>
public interface ILocalStorageService
{
    /// <summary>
    /// Initialises the underlying database, creating the schema if it does
    /// not already exist.  Must be called once at application startup.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A <see cref="Result"/> describing the outcome.</returns>
    Task<Result> InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the value associated with <paramref name="key"/>, or
    /// <see langword="null"/> when no value is stored.
    /// </summary>
    /// <param name="key">The key to look up.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> whose <see cref="Result{T}.Value"/> is the
    /// stored string value, or <see langword="null"/> if the key is missing.
    /// </returns>
    Task<Result<string?>> GetAsync(
        string key,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores <paramref name="value"/> under <paramref name="key"/>,
    /// overwriting any existing value.
    /// </summary>
    /// <param name="key">The key to store the value under.</param>
    /// <param name="value">The string value to store.  Pass <see langword="null"/> to remove the entry.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A <see cref="Result"/> describing the outcome.</returns>
    Task<Result> SetAsync(
        string key,
        string? value,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the entry for <paramref name="key"/>, if any.
    /// </summary>
    /// <param name="key">The key to remove.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A <see cref="Result"/> describing the outcome.</returns>
    Task<Result> RemoveAsync(
        string key,
        CancellationToken cancellationToken = default);
}
