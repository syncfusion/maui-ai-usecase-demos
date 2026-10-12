using Microsoft.Extensions.Logging;
using Todo.Services.Interfaces;
using System.Text.Json;
using SQLite;
using Todo.Data;
using Todo.Utilities;

namespace Todo.Services.Storage;

/// <summary>
/// SQLite-backed implementation of <see cref="ILocalStorageService"/>.  The
/// service manages a dedicated <c>Preferences</c> table with two columns
/// (key, value) and serialises every value as JSON so the column can hold
/// arbitrary payloads without a schema migration.
/// </summary>
/// <remarks>
/// <para>
/// The <c>Preferences</c> table is created lazily the first time the
/// service is asked to read or write a value, so the cost is paid only on
/// devices that actually use local storage.  The table definition is
/// pinned to a primary key on the <c>Key</c> column so duplicate inserts
/// fail fast.
/// </para>
/// <para>
/// All public methods catch exceptions and translate them to
/// <see cref="Result{T}"/> values.  This makes the contract easy to
/// consume from <c>[RelayCommand]</c> methods on the ViewModels.
/// </para>
/// </remarks>
public sealed class LocalStorageService : ILocalStorageService
{
    /// <summary>
    /// Name of the SQLite table that stores key/value preferences.
    /// </summary>
    public const string PreferencesTableName = "Preferences";

    private readonly AppDbContext _context;
    private readonly ILogger<LocalStorageService> _logger;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _initialised;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    /// <summary>
    /// Initialises the service with the application-wide
    /// <see cref="AppDbContext"/>.
    /// </summary>
    /// <param name="context">The shared database context.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    public LocalStorageService(AppDbContext context, ILogger<LocalStorageService> logger)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(logger);

        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<T>> GetAsync<T>(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return Result<T>.Failure(
                new ArgumentException("Key must be a non-empty string.", nameof(key)),
                "Invalid key.");
        }

        try
        {
            await EnsureInitialisedAsync().ConfigureAwait(false);

            PreferenceEntry? row = await _context.Connection
                .Table<PreferenceEntry>()
                .Where(p => p.Key == key)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            if (row is null)
            {
                return Result<T>.Failure(
                    new KeyNotFoundException($"No preference stored under key '{key}'."),
                    $"Key '{key}' was not found.");
            }

            if (string.IsNullOrEmpty(row.Value))
            {
                return Result<T>.Failure(
                    new InvalidDataException($"Preference '{key}' is stored as an empty payload."),
                    $"Preference '{key}' has no value.");
            }

            T? value = JsonSerializer.Deserialize<T>(row.Value, SerializerOptions);
            if (value is null)
            {
                return Result<T>.Failure(
                    new InvalidDataException($"Preference '{key}' deserialised to null."),
                    $"Preference '{key}' has no value.");
            }

            return Result<T>.Success(value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read preference {Key}", key);
            return Result<T>.Failure(ex);
        }
    }

    /// <inheritdoc />
    public async Task<Result<T>> SetAsync<T>(string key, T value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return Result<T>.Failure(
                new ArgumentException("Key must be a non-empty string.", nameof(key)),
                "Invalid key.");
        }

        try
        {
            await EnsureInitialisedAsync().ConfigureAwait(false);

            string payload = JsonSerializer.Serialize(value, SerializerOptions);
            DateTime now = DateTime.UtcNow;

            PreferenceEntry? existing = await _context.Connection
                .Table<PreferenceEntry>()
                .Where(p => p.Key == key)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            if (existing is null)
            {
                var entry = new PreferenceEntry
                {
                    Key = key,
                    Value = payload,
                    UpdatedAt = now,
                };
                await _context.Connection.InsertAsync(entry).ConfigureAwait(false);
            }
            else
            {
                existing.Value = payload;
                existing.UpdatedAt = now;
                await _context.Connection.UpdateAsync(existing).ConfigureAwait(false);
            }

            _logger.LogDebug("Stored preference {Key} ({Length} bytes)", key, payload.Length);
            return Result<T>.Success(value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write preference {Key}", key);
            return Result<T>.Failure(ex);
        }
    }

    /// <inheritdoc />
    public async Task<Result<bool>> DeleteAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return Result<bool>.Failure(
                new ArgumentException("Key must be a non-empty string.", nameof(key)),
                "Invalid key.");
        }

        try
        {
            await EnsureInitialisedAsync().ConfigureAwait(false);

            int rows = await _context.Connection
                .Table<PreferenceEntry>()
                .Where(p => p.Key == key)
                .DeleteAsync()
                .ConfigureAwait(false);

            _logger.LogDebug("Deleted preference {Key} (rows={Rows})", key, rows);
            return Result<bool>.Success(rows > 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete preference {Key}", key);
            return Result<bool>.Failure(ex);
        }
    }

    /// <inheritdoc />
    public async Task<Result<bool>> ContainsAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return Result<bool>.Failure(
                new ArgumentException("Key must be a non-empty string.", nameof(key)),
                "Invalid key.");
        }

        try
        {
            await EnsureInitialisedAsync().ConfigureAwait(false);

            int count = await _context.Connection
                .Table<PreferenceEntry>()
                .Where(p => p.Key == key)
                .CountAsync()
                .ConfigureAwait(false);

            return Result<bool>.Success(count > 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to test for preference {Key}", key);
            return Result<bool>.Failure(ex);
        }
    }

    /// <inheritdoc />
    public async Task<Result<bool>> ClearAsync()
    {
        try
        {
            await EnsureInitialisedAsync().ConfigureAwait(false);

            int rows = await _context.Connection
                .Table<PreferenceEntry>()
                .DeleteAsync()
                .ConfigureAwait(false);

            _logger.LogInformation("Cleared {Rows} preferences", rows);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear preferences");
            return Result<bool>.Failure(ex);
        }
    }

    /// <summary>
    /// Ensures the <c>Preferences</c> table exists.  The check is
    /// serialised through a <see cref="SemaphoreSlim"/> so two callers
    /// racing on cold-start cannot both create the table.
    /// </summary>
    private async Task EnsureInitialisedAsync()
    {
        if (_initialised)
        {
            return;
        }

        await _initLock.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_initialised)
            {
                return;
            }

            await _context.Connection
                .CreateTableAsync<PreferenceEntry>()
                .ConfigureAwait(false);

            _initialised = true;
            _logger.LogDebug("Preferences table is ready");
        }
        finally
        {
            _initLock.Release();
        }
    }

    /// <summary>
    /// Internal row representation for the <c>Preferences</c> table.  The
    /// <c>Value</c> column stores a JSON blob.
    /// </summary>
    [Table(PreferencesTableName)]
    private sealed class PreferenceEntry
    {
        /// <summary>Primary key — the preference key.</summary>
        [PrimaryKey]
        [Column("Key")]
        public string Key { get; set; } = string.Empty;

        /// <summary>JSON-serialised payload.</summary>
        [Column("Value")]
        public string Value { get; set; } = string.Empty;

        /// <summary>UTC timestamp of the most recent write.</summary>
        [Column("UpdatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
