using System.Linq.Expressions;
using SQLite;

namespace Todo.Data.Repositories;

/// <summary>
/// Generic async repository contract that every aggregate root persisted by
/// the application implements.  Methods return <see cref="Task{T}"/> so the
/// underlying SQLite-net call is awaited on the captured context and the
/// caller is never blocked on disk I/O.
/// </summary>
/// <typeparam name="T">
/// A reference type with a public parameterless constructor and a string
/// <c>Id</c> property.  Models in <c>Todo.Models</c> already satisfy this
/// contract.
/// </typeparam>
public interface IRepository<T>
    where T : class, new()
{
    /// <summary>
    /// Returns the entity with the supplied primary key, or <see langword="null"/>
    /// when no row matches.
    /// </summary>
    /// <param name="id">Primary-key value to look up.</param>
    /// <param name="includeSoftDeleted">
    /// When <see langword="true"/>, rows with a non-null
    /// <c>DeletedAt</c> are returned as well.  Defaults to
    /// <see langword="false"/> to honour the soft-delete pattern.
    /// </param>
    Task<T?> GetByIdAsync(string id, bool includeSoftDeleted = false);

    /// <summary>
    /// Returns every row of <typeparamref name="T"/> in the table.
    /// </summary>
    /// <param name="includeSoftDeleted">
    /// When <see langword="true"/>, soft-deleted rows are included.
    /// </param>
    Task<IReadOnlyList<T>> GetAllAsync(bool includeSoftDeleted = false);

    /// <summary>
    /// Returns every row matching <paramref name="predicate"/>.
    /// </summary>
    /// <param name="predicate">A SQL-net compatible expression.</param>
    /// <param name="includeSoftDeleted">
    /// When <see langword="true"/>, soft-deleted rows are included.
    /// </param>
    Task<IReadOnlyList<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        bool includeSoftDeleted = false);

    /// <summary>
    /// Returns the first row matching <paramref name="predicate"/>, or
    /// <see langword="null"/> when no row matches.
    /// </summary>
    Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        bool includeSoftDeleted = false);

    /// <summary>
    /// Inserts <paramref name="entity"/> and returns the number of rows
    /// written (1 on success, 0 on failure).  The entity is updated in
    /// place with the auto-generated primary key when applicable.
    /// </summary>
    Task<int> InsertAsync(T entity);

    /// <summary>
    /// Updates <paramref name="entity"/> in place and returns the number
    /// of rows written.
    /// </summary>
    Task<int> UpdateAsync(T entity);

    /// <summary>
    /// Soft-deletes the row with the supplied primary key by setting its
    /// <c>DeletedAt</c> column to <see cref="DateTime.UtcNow"/>.  Returns
    /// the number of rows written.
    /// </summary>
    Task<int> SoftDeleteAsync(string id);

    /// <summary>
    /// Hard-deletes the row with the supplied primary key.  Use with care:
    /// the row cannot be recovered.
    /// </summary>
    Task<int> DeleteAsync(string id);

    /// <summary>
    /// Returns the number of rows currently stored in the table.
    /// </summary>
    /// <param name="includeSoftDeleted">
    /// When <see langword="true"/>, soft-deleted rows are included.
    /// </param>
    Task<int> CountAsync(bool includeSoftDeleted = false);

    /// <summary>
    /// Exposes the underlying <see cref="SQLiteAsyncConnection"/> for
    /// advanced queries (transactions, joins, raw SQL) that the generic
    /// CRUD methods do not cover.  Use sparingly.
    /// </summary>
    SQLiteAsyncConnection Connection { get; }
}
