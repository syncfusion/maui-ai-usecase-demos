using Todo.Utilities;

namespace Todo.Services.Storage;

/// <summary>
/// Service contract for persisting small key-value pairs (preferences,
/// cached tokens, "last opened list", etc.) on the device.  Backed by
/// the SQLite <c>Preferences</c> table that is managed by
/// <see cref="Data.AppDbContext"/>.
/// </summary>
/// <remarks>
/// <para>
/// The interface is intentionally generic so callers can stash a typed
/// value and receive it back without manual JSON conversion.  The
/// implementation uses <see cref="System.Text.Json"/> for serialisation.
/// </para>
/// <para>
/// All methods are asynchronous and return a
/// <see cref="Result{T}"/> / <see cref="Utilities.Result{T}"/> so that
/// failures (corrupt JSON, missing key, etc.) surface to the caller as a
/// structured result instead of a thrown exception.
/// </para>
/// </remarks>
public interface ILocalStorageService
{
    /// <summary>
    /// Retrieves the value stored under <paramref name="key"/> and
    /// deserialises it as <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Expected value type.</typeparam>
    /// <param name="key">The key to look up.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> containing the value when the
    /// key exists, or a failed <see cref="Result{T}"/> with
    /// <see cref="Result{T}.IsSuccess"/> set to <see langword="false"/>
    /// when the key is unknown or the payload cannot be deserialised.
    /// </returns>
    Task<Result<T>> GetAsync<T>(string key);

    /// <summary>
    /// Serialises <paramref name="value"/> and stores it under
    /// <paramref name="key"/>, overwriting any existing entry.
    /// </summary>
    /// <typeparam name="T">Type of the value being stored.</typeparam>
    /// <param name="key">The key to write to.</param>
    /// <param name="value">The value to persist (may be <see langword="null"/>).</param>
    /// <returns>
    /// A <see cref="Utilities.Result{T}"/> describing the outcome of the
    /// write operation.
    /// </returns>
    Task<Result<T>> SetAsync<T>(string key, T value);

    /// <summary>
    /// Removes the value stored under <paramref name="key"/>, if any.
    /// </summary>
    /// <param name="key">The key to delete.</param>
    /// <returns>
    /// A <see cref="Utilities.Result{T}"/> whose <c>IsSuccess</c> flag
    /// reports whether the row was found and deleted.
    /// </returns>
    Task<Result<bool>> DeleteAsync(string key);

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="key"/> exists.
    /// </summary>
    /// <param name="key">The key to test.</param>
    Task<Result<bool>> ContainsAsync(string key);

    /// <summary>
    /// Removes every key/value pair from the preferences table.
    /// </summary>
    Task<Result<bool>> ClearAsync();
}
