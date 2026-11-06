namespace WardrobeAI.Services;

/// <summary>
/// Resolves the active user for the lifetime of the app. Replaces the
/// pre-P7 <c>IAuthService</c> since the app no longer authenticates a real
/// user — the default <c>Alex Rivera</c> row is created on first run.
/// </summary>
public interface IUserContext
{
    /// <summary>
    /// Returns the stable <see cref="Models.User.UserId"/> for the active
    /// session. Must be called only after <see cref="EnsureUserAsync"/> has
    /// run during application startup.
    /// </summary>
    int GetCurrentUserId();

    /// <summary>
    /// Idempotently guarantees a default <see cref="Models.User"/> row exists
    /// and that subsequent calls to <see cref="GetCurrentUserId"/> return a
    /// non-zero id. Safe to invoke more than once.
    /// </summary>
    Task EnsureUserAsync(CancellationToken cancellationToken = default);
}
