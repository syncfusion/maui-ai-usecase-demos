using Microsoft.Extensions.Logging;
using SQLite;
using Todo.Models;
using Todo.Utilities;

namespace Todo.Data;

/// <summary>
/// Wraps a single <see cref="SQLiteAsyncConnection"/> and is responsible for
/// initialising the local database (creating tables, enabling foreign keys,
/// applying pragmas) on application start.  All repositories and services
/// receive the same context instance through the DI container so the
/// underlying connection is shared.
/// </summary>
/// <remarks>
/// <para>
/// This class is the SQLite-net equivalent of an Entity Framework
/// <c>DbContext</c> but is intentionally minimal — it does not own
/// change-tracking; the repositories do that explicitly through
/// <c>Insert/Update/Delete</c> calls.
/// </para>
/// <para>
/// The connection is created with <see cref="SQLiteOpenFlags.SharedCache"/>
/// so the same file can be opened by multiple repositories without
/// blocking each other, and the default busy timeout
/// (<see cref="Constants.Database.BusyTimeoutSeconds"/>) is applied so
/// short-lived contention resolves automatically.
/// </para>
/// </remarks>
public sealed class AppDbContext : IAsyncDisposable
{
    private readonly ILogger<AppDbContext> _logger;
    private bool _disposed;

    /// <summary>
    /// Creates a new <see cref="AppDbContext"/> pointing at the supplied
    /// database file, creates the schema if necessary, and enables
    /// foreign-key enforcement.
    /// </summary>
    /// <param name="databasePath">
    /// Absolute path of the SQLite file.  On real devices this is typically
    /// <c>Path.Combine(FileSystem.AppDataDirectory, "todo.db3")</c>.
    /// </param>
    /// <param name="logger">Logger for diagnostics.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="databasePath"/> is null or whitespace.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="logger"/> is <see langword="null"/>.
    /// </exception>
    public AppDbContext(string databasePath, ILogger<AppDbContext> logger)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;

        // Ensure the parent directory exists.  On Windows / iOS / Android the
        // platform-provided AppDataDirectory is always created, but a
        // developer-supplied path may not be.
        string? directory = Path.GetDirectoryName(databasePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        DatabasePath = databasePath;

        _logger.LogInformation(
            "Opening SQLite database at {Path} (flags={Flags}, busyTimeout={Timeout}s)",
            DatabasePath,
            Constants.Database.OpenFlags,
            Constants.Database.BusyTimeoutSeconds);

        Connection = new SQLiteAsyncConnection(
            DatabasePath,
            Constants.Database.OpenFlags,
            storeDateTimeAsTicks: true);

        // Enable foreign-key enforcement.  Per https://www.sqlite.org/pragma.html#pragma_foreign_keys
        // FK enforcement is OFF by default for every new connection.
        Connection.ExecuteScalarAsync<int>("PRAGMA foreign_keys = ON;").GetAwaiter().GetResult();

        // Set the default busy timeout so a short-lived contention with the
        // background thread that flushes WAL files resolves itself.
        Connection.SetBusyTimeoutAsync(TimeSpan.FromSeconds(Constants.Database.BusyTimeoutSeconds))
            .GetAwaiter()
            .GetResult();

        // Run schema creation synchronously inside the constructor so that
        // the first repository call can rely on the tables already existing.
        InitializeSchemaAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Absolute path of the SQLite file backing this context.
    /// </summary>
    public string DatabasePath { get; }

    /// <summary>
    /// The shared <see cref="SQLiteAsyncConnection"/> used by every
    /// repository.  Exposed so tests can inspect / dispose of it directly.
    /// </summary>
    public SQLiteAsyncConnection Connection { get; }

    /// <summary>
    /// Creates every table known to the application (idempotent).  Callers
    /// do not need to invoke this directly — the constructor does it.
    /// </summary>
    /// <returns>A task that completes when the schema is in place.</returns>
    public async Task InitializeSchemaAsync()
    {
        _logger.LogDebug("Creating database schema (Users, TaskLists, Tasks)");

        await Connection.CreateTableAsync<User>().ConfigureAwait(false);
        await Connection.CreateTableAsync<TaskList>().ConfigureAwait(false);
        await Connection.CreateTableAsync<TaskItem>().ConfigureAwait(false);

        _logger.LogInformation("Database schema initialised");
    }

    /// <summary>
    /// Closes and disposes the underlying <see cref="SQLiteAsyncConnection"/>.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        try
        {
            await Connection.CloseAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to close SQLite connection cleanly");
        }
    }
}
