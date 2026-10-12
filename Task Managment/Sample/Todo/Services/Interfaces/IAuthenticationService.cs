using Todo.Models.DTOs;
using Todo.Utilities;

namespace Todo.Services.Interfaces;

/// <summary>
/// Contract for the authentication service used by the ViewModels to sign the
/// user in, sign them out and inspect the currently-authenticated user.
/// Concrete implementation is added in Module 2 (Task 2-01).
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Gets the currently-authenticated user, or <see langword="null"/> if no
    /// user is signed in.
    /// </summary>
    UserDto? CurrentUser { get; }

    /// <summary>
    /// Gets a value indicating whether a user is currently signed in.
    /// </summary>
    bool IsSignedIn { get; }

    /// <summary>
    /// Raised when the signed-in state changes (sign-in, sign-out, token
    /// refresh, etc.).  Subscribers are expected to be on the UI thread.
    /// </summary>
    event EventHandler<UserDto?>? SignedInChanged;

    /// <summary>
    /// Attempts to sign the user in using the supplied credentials.
    /// </summary>
    /// <param name="email">The e-mail address used as the user identifier.</param>
    /// <param name="password">The user-supplied password.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> describing the outcome.  On success the
    /// <see cref="Result{T}.Value"/> is the authenticated
    /// <see cref="UserDto"/>.
    /// </returns>
    Task<Result<UserDto>> SignInAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a brand-new account and signs the user in.
    /// </summary>
    /// <param name="email">The e-mail address to register.</param>
    /// <param name="password">The user-supplied password.</param>
    /// <param name="displayName">Optional display name.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> describing the outcome.  On success the
    /// <see cref="Result{T}.Value"/> is the newly-created
    /// <see cref="UserDto"/>.
    /// </returns>
    Task<Result<UserDto>> SignUpAsync(
        string email,
        string password,
        string? displayName = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Signs the current user out and clears any cached credentials.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A <see cref="Result"/> describing the outcome.</returns>
    Task<Result> SignOutAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a password-reset e-mail to the supplied address.
    /// </summary>
    /// <param name="email">The e-mail address to send the reset link to.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A <see cref="Result"/> describing the outcome.</returns>
    Task<Result> RequestPasswordResetAsync(
        string email,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Re-hydrates the in-memory <see cref="CurrentUser"/> from a token
    /// previously persisted in <c>SecureStorage</c>.  Called by the splash
    /// page on application start-up to honour the "Authentication state
    /// persists on app restart" acceptance criterion.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> when a valid token is present,
    /// or a failed <see cref="Result{T}"/> with a human-readable
    /// <see cref="Result{T}.ErrorMessage"/> otherwise.
    /// </returns>
    Task<Result<UserDto>> RestoreSessionAsync(CancellationToken cancellationToken = default);
}
