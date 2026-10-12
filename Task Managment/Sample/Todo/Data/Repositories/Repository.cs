using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.Logging;
using SQLite;

namespace Todo.Data.Repositories;

/// <summary>
/// Default generic <see cref="IRepository{T}"/> implementation backed by
/// <see cref="SQLiteAsyncConnection"/>.  The repository is deliberately
/// thin: it forwards calls to SQLite-net and adds the soft-delete
/// filtering that every aggregate root in <c>Todo.Models</c> expects.
/// </summary>
/// <remarks>
/// <para>
/// Soft-delete filtering works by reflecting on the entity's
/// <c>DeletedAt</c> property (a nullable <see cref="DateTime"/>).  When the
/// property exists, the repository appends a <c>DeletedAt IS NULL</c>
/// clause to the generated SQL.  Entities that do not declare
/// <c>DeletedAt</c> are treated as non-soft-deletable.
/// </para>
/// <para>
/// Every method logs at <c>Debug</c> level; <c>Information</c> is reserved
/// for inserts / updates / deletes so the application log can audit
/// changes.
/// </para>
/// </remarks>
/// <typeparam name="T">
/// The entity type handled by the repository.  Must expose a string
/// <c>Id</c> property.
/// </typeparam>
public class Repository<T> : IRepository<T>
    where T : class, new()
{
    private readonly ILogger<Repository<T>> _logger;
    private readonly PropertyInfo? _deletedAtProperty;
    private readonly PropertyInfo? _idProperty;
    private readonly bool _supportsSoftDelete;

    /// <summary>
    /// Initialises a new repository that operates on the supplied
    /// connection.
    /// </summary>
    /// <param name="context">The application-wide <see cref="AppDbContext"/>.</param>
    /// <param name="logger">Logger used for diagnostics.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="context"/> or <paramref name="logger"/>
    /// is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <typeparamref name="T"/> does not expose a public
    /// <c>Id</c> property.
    /// </exception>
    public Repository(AppDbContext context, ILogger<Repository<T>> logger)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(logger);

        Context = context;
        Connection = context.Connection;
        _logger = logger;

        Type type = typeof(T);

        _idProperty = type.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance)
            ?? throw new InvalidOperationException(
                $"{type.FullName} must expose a public 'Id' property to be used with {nameof(Repository<T>)}.");

        _deletedAtProperty = type.GetProperty(
            "DeletedAt",
            BindingFlags.Public | BindingFlags.Instance);

        _supportsSoftDelete = _deletedAtProperty is { PropertyType: var t } &&
            Nullable.GetUnderlyingType(t) == typeof(DateTime);

        _logger.LogDebug(
            "Repository<{Type}> initialised (softDelete={SupportsSoftDelete})",
            type.Name,
            _supportsSoftDelete);
    }

    /// <summary>
    /// The <see cref="AppDbContext"/> this repository is bound to.  Useful
    /// when callers need access to the underlying connection without
    /// going through <see cref="Connection"/>.
    /// </summary>
    protected AppDbContext Context { get; }

    /// <inheritdoc />
    public SQLiteAsyncConnection Connection { get; }

    /// <inheritdoc />
    public virtual async Task<T?> GetByIdAsync(string id, bool includeSoftDeleted = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        T? entity = await Connection.FindAsync<T>(id).ConfigureAwait(false);

        if (entity is null)
        {
            return null;
        }

        if (!includeSoftDeleted && IsSoftDeleted(entity))
        {
            return null;
        }

        return entity;
    }

    /// <inheritdoc />
    public virtual async Task<IReadOnlyList<T>> GetAllAsync(bool includeSoftDeleted = false)
    {
        List<T> rows = await Connection.Table<T>().ToListAsync().ConfigureAwait(false);

        if (includeSoftDeleted || !_supportsSoftDelete)
        {
            return rows;
        }

        return rows.Where(row => !IsSoftDeleted(row)).ToList();
    }

    /// <inheritdoc />
    public virtual async Task<IReadOnlyList<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        bool includeSoftDeleted = false)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        List<T> rows = await Connection.Table<T>()
            .Where(predicate)
            .ToListAsync()
            .ConfigureAwait(false);

        if (includeSoftDeleted || !_supportsSoftDelete)
        {
            return rows;
        }

        return rows.Where(row => !IsSoftDeleted(row)).ToList();
    }

    /// <inheritdoc />
    public virtual async Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        bool includeSoftDeleted = false)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        T? row = await Connection.Table<T>()
            .Where(predicate)
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);

        if (row is null)
        {
            return null;
        }

        if (!includeSoftDeleted && IsSoftDeleted(row))
        {
            return null;
        }

        return row;
    }

    /// <inheritdoc />
    public virtual async Task<int> InsertAsync(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        StampTimestamps(entity, creating: true);
        int rows = await Connection.InsertAsync(entity).ConfigureAwait(false);
        _logger.LogInformation(
            "Inserted {Type} (id={Id}, rows={Rows})",
            typeof(T).Name,
            GetId(entity),
            rows);
        return rows;
    }

    /// <inheritdoc />
    public virtual async Task<int> UpdateAsync(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        StampTimestamps(entity, creating: false);
        int rows = await Connection.UpdateAsync(entity).ConfigureAwait(false);
        _logger.LogInformation(
            "Updated {Type} (id={Id}, rows={Rows})",
            typeof(T).Name,
            GetId(entity),
            rows);
        return rows;
    }

    /// <inheritdoc />
    public virtual async Task<int> SoftDeleteAsync(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        if (!_supportsSoftDelete)
        {
            throw new InvalidOperationException(
                $"{typeof(T).Name} does not support soft delete (no nullable DateTime 'DeletedAt' property).");
        }

        T? entity = await Connection.FindAsync<T>(id).ConfigureAwait(false);
        if (entity is null)
        {
            return 0;
        }

        DateTime now = DateTime.UtcNow;
        _deletedAtProperty!.SetValue(entity, now);

        // Also bump UpdatedAt so consumers can see the entity changed.
        PropertyInfo? updatedAt = typeof(T).GetProperty(
            "UpdatedAt",
            BindingFlags.Public | BindingFlags.Instance);
        updatedAt?.SetValue(entity, now);

        int rows = await Connection.UpdateAsync(entity).ConfigureAwait(false);
        _logger.LogInformation(
            "Soft-deleted {Type} (id={Id}, rows={Rows})",
            typeof(T).Name,
            id,
            rows);
        return rows;
    }

    /// <inheritdoc />
    public virtual async Task<int> DeleteAsync(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        int rows = await Connection.DeleteAsync<T>(id).ConfigureAwait(false);
        _logger.LogInformation(
            "Hard-deleted {Type} (id={Id}, rows={Rows})",
            typeof(T).Name,
            id,
            rows);
        return rows;
    }

    /// <inheritdoc />
    public virtual async Task<int> CountAsync(bool includeSoftDeleted = false)
    {
        if (includeSoftDeleted || !_supportsSoftDelete)
        {
            return await Connection.Table<T>().CountAsync().ConfigureAwait(false);
        }

        // SQLite-net cannot express "IS NULL" on a reflected property in a
        // typed query, so we fetch the rows and filter in-memory.  Acceptable
        // for the dataset sizes this app deals with.
        List<T> rows = await Connection.Table<T>().ToListAsync().ConfigureAwait(false);
        return rows.Count(row => !IsSoftDeleted(row));
    }

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="entity"/>'s
    /// <c>DeletedAt</c> column is a non-null timestamp.
    /// </summary>
    private bool IsSoftDeleted(T entity)
    {
        if (!_supportsSoftDelete)
        {
            return false;
        }

        object? value = _deletedAtProperty!.GetValue(entity);
        return value is DateTime dt && dt != default;
    }

    /// <summary>
    /// Sets the entity's <c>CreatedAt</c> / <c>UpdatedAt</c> columns to the
    /// current UTC timestamp.  No-op when the properties are missing.
    /// </summary>
    /// <param name="entity">The entity to stamp.</param>
    /// <param name="creating">
    /// When <see langword="true"/>, both columns are set; otherwise only
    /// <c>UpdatedAt</c> is updated.
    /// </param>
    private void StampTimestamps(T entity, bool creating)
    {
        DateTime now = DateTime.UtcNow;

        if (creating)
        {
            typeof(T).GetProperty("CreatedAt", BindingFlags.Public | BindingFlags.Instance)
                ?.SetValue(entity, now);
        }

        typeof(T).GetProperty("UpdatedAt", BindingFlags.Public | BindingFlags.Instance)
            ?.SetValue(entity, now);
    }

    /// <summary>
    /// Reads the <c>Id</c> property for logging purposes.
    /// </summary>
    private string? GetId(T entity) => _idProperty!.GetValue(entity) as string;
}
