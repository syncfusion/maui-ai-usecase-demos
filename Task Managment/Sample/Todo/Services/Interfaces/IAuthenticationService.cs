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
    /// <param name="rememberMe">
    /// When <see langword="true"/>, the e-mail address is persisted in
    /// <c>SecureStorage</c> so the next launch can pre-populate the sign-in
    /// form.  The password is <b>never</b> persisted — only the
    /// (non-sensitive) identifier.  Defaults to <see langword="false"/>.
    /// </param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> describing the outcome.  On success the
    /// <see cref="Result{T}.Value"/> is the authenticated
    /// <see cref="UserDto"/>.
    /// </returns>
    Task<Result<UserDto>> SignInAsync(
        string email,
        string password,
        bool rememberMe = false,
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
    /// Signs the current user out and clears any cached credentials (including
    /// the remembered e-mail address from <c>SecureStorage</c>).
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
    /// <remarks>
    /// Implementations are expected to perform a <b>proactive</b> refresh of
    /// the access token when the persisted token is within the refresh
    /// window (see <see cref="RefreshTokenAsync"/>) so the auto-login flow
    /// never has to surface an "expired token" error to the user.
    /// </remarks>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> when a valid token is present,
    /// or a failed <see cref="Result{T}"/> with a human-readable
    /// <see cref="Result{T}.ErrorMessage"/> otherwise.
    /// </returns>
    Task<Result<UserDto>> RestoreSessionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes the in-memory access token using the currently-stored
    /// refresh token.  Returns a failed <see cref="Result{T}"/> when no
    /// refresh token is available or it has expired.  On failure the
    /// persisted tokens are cleared and <see cref="SignedInChanged"/> is
    /// raised with a <see langword="null"/> payload so the rest of the app
    /// can react to the sign-out.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> describing the outcome.  On success the
    /// <see cref="Result{T}.Value"/> is the freshly-issued
    /// <see cref="Models.AuthToken"/>.
    /// </returns>
    Task<Result<AuthToken>> RefreshTokenAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the e-mail address that was last signed-in with the
    /// "Remember me" checkbox enabled, or <see langword="null"/> when no
    /// address is remembered.  The value is read from
    /// <c>SecureStorage</c> and is used by the sign-in page to pre-populate
    /// the e-mail field.
    /// </summary>
    /// <returns>
    /// The remembered e-mail address, or <see langword="null"/> when no
    /// address is remembered or the secure store is unavailable.
    /// </returns>
    Task<string?> GetRememberedEmailAsync(CancellationToken cancellationToken = default);
}
