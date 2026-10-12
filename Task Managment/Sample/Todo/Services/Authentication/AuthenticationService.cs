using Microsoft.Extensions.Logging;
using Microsoft.Maui.ApplicationModel;
using SQLite;
using Todo.Data.Repositories;
using Todo.Models;
using Todo.Models.DTOs;
using Todo.Services.Interfaces;
using Todo.Utilities;

namespace Todo.Services.Authentication;

/// <summary>
/// Local, SQLite-backed implementation of <see cref="IAuthenticationService"/>.
/// The service is responsible for:
/// <list type="number">
///   <item><description>Creating new accounts (with PBKDF2-hashed passwords).</description></item>
///   <item><description>Verifying credentials and issuing short-lived access
///     / long-lived refresh tokens.</description></item>
///   <item><description>Persisting the issued tokens in
///     <see cref="Microsoft.Maui.Storage.SecureStorage"/> so the user remains
///     signed-in across application restarts.</description></item>
///   <item><description>Surfacing a <see cref="SignedInChanged"/> event so
///     ViewModels and the Shell can react to sign-in / sign-out transitions.
///   </description></item>
/// </list>
/// </summary>
/// <remarks>
/// <para>
/// The service is registered as <b>scoped</b> in
/// <see cref="MauiProgram.AddServices"/>.  .NET MAUI does not create DI
/// scopes per request, so a scoped service is effectively a singleton for
/// the application lifetime.  The <see cref="CurrentUser"/> snapshot is
/// kept in process memory and re-hydrated from secure storage on the first
/// access (or via <see cref="RestoreSessionAsync"/> from the splash page).
/// </para>
/// <para>
/// Passwords are hashed with <see cref="PasswordValidator"/> (PBKDF2-SHA256,
/// 200&nbsp;000 iterations) and never leave the device.  The local
/// <c>Users</c> table is the single source of truth for the account record;
/// a future remote backend can transparently take over the same flow by
/// providing an alternative <see cref="IAuthenticationService"/>
/// implementation.
/// </para>
/// </remarks>
public sealed class AuthenticationService : IAuthenticationService
{
    private readonly IRepository<User> _userRepository;
    private readonly PasswordValidator _passwordValidator;
    private readonly TokenManager _tokenManager;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly SemaphoreSlim _sessionLock = new(1, 1);

    private UserDto? _currentUser;
    private AuthToken? _currentToken;

    /// <summary>
    /// Initialises a new instance of the <see cref="AuthenticationService"/>
    /// class.  All dependencies are injected by the DI container.
    /// </summary>
    /// <param name="userRepository">Repository used to read and write the <c>Users</c> table.</param>
    /// <param name="passwordValidator">Helper that hashes and verifies passwords.</param>
    /// <param name="tokenManager">Helper that issues, validates and persists tokens.</param>
    /// <param name="logger">Logger for structured diagnostics.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any of the supplied dependencies is <see langword="null"/>.
    /// </exception>
    public AuthenticationService(
        IRepository<User> userRepository,
        PasswordValidator passwordValidator,
        TokenManager tokenManager,
        ILogger<AuthenticationService> logger)
    {
        ArgumentNullException.ThrowIfNull(userRepository);
        ArgumentNullException.ThrowIfNull(passwordValidator);
        ArgumentNullException.ThrowIfNull(tokenManager);
        ArgumentNullException.ThrowIfNull(logger);

        _userRepository = userRepository;
        _passwordValidator = passwordValidator;
        _tokenManager = tokenManager;
        _logger = logger;
    }

    /// <inheritdoc />
    public UserDto? CurrentUser => _currentUser;

    /// <inheritdoc />
    public bool IsSignedIn => _currentUser is not null && _currentToken is not null;

    /// <inheritdoc />
    public event EventHandler<UserDto?>? SignedInChanged;

    /// <inheritdoc />
    public async Task<Result<UserDto>> SignInAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (!Helpers.IsValidEmail(email))
        {
            return Result<UserDto>.Failure(
                new ArgumentException("E-mail address is not valid.", nameof(email)),
                "Please enter a valid e-mail address.");
        }

        if (string.IsNullOrEmpty(password))
        {
            return Result<UserDto>.Failure(
                new ArgumentException("Password must not be empty.", nameof(password)),
                "Please enter your password.");
        }

        await _sessionLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            string normalisedEmail = email.Trim().ToLowerInvariant();
            User? user = await _userRepository
                .FirstOrDefaultAsync(u => u.Email == normalisedEmail, includeSoftDeleted: false)
                .ConfigureAwait(false);

            if (user is null)
            {
                _logger.LogInformation("Sign-in failed: no account for {Email}", normalisedEmail);
                return Result<UserDto>.Failure(
                    new InvalidOperationException("No account matches the supplied e-mail."),
                    "Invalid e-mail or password.");
            }

            if (!_passwordValidator.Verify(password, user.PasswordHash))
            {
                _logger.LogInformation("Sign-in failed: wrong password for {Email}", normalisedEmail);
                return Result<UserDto>.Failure(
                    new UnauthorizedAccessException("Password does not match the stored hash."),
                    "Invalid e-mail or password.");
            }

            AuthToken token = await _tokenManager.IssueAsync(user.Id).ConfigureAwait(false);
            user.LastSignedInAt = DateTimeOffset.UtcNow;
            await _userRepository.UpdateAsync(user).ConfigureAwait(false);

            await _tokenManager.StoreAsync(token).ConfigureAwait(false);
            await UpdateCurrentUserAsync(ToDto(user, DateTimeOffset.UtcNow), token, cancellationToken)
                .ConfigureAwait(false);

            _logger.LogInformation("User {UserId} signed in", user.Id);
            return Result<UserDto>.Success(_currentUser!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during sign-in for {Email}", email);
            return Result<UserDto>.Failure(ex);
        }
        finally
        {
            _sessionLock.Release();
        }
    }

    /// <inheritdoc />
    public async Task<Result<UserDto>> SignUpAsync(
        string email,
        string password,
        string? displayName = null,
        CancellationToken cancellationToken = default)
    {
        if (!Helpers.IsValidEmail(email))
        {
            return Result<UserDto>.Failure(
                new ArgumentException("E-mail address is not valid.", nameof(email)),
                "Please enter a valid e-mail address.");
        }

        if (!PasswordValidator.MeetsStrengthPolicy(password))
        {
            return Result<UserDto>.Failure(
                new ArgumentException("Password does not meet the strength policy.", nameof(password)),
                "Password must be at least 8 characters and include upper-case, lower-case, digit and symbol.");
        }

        await _sessionLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            string normalisedEmail = email.Trim().ToLowerInvariant();

            User? existing = await _userRepository
                .FirstOrDefaultAsync(u => u.Email == normalisedEmail, includeSoftDeleted: true)
                .ConfigureAwait(false);
            if (existing is not null)
            {
                _logger.LogInformation("Sign-up rejected: {Email} already exists", normalisedEmail);
                return Result<UserDto>.Failure(
                    new InvalidOperationException("E-mail is already registered."),
                    "An account with this e-mail already exists.");
            }

            string passwordHash = _passwordValidator.Hash(password);
            DateTimeOffset now = DateTimeOffset.UtcNow;
            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Email = normalisedEmail,
                PasswordHash = passwordHash,
                DisplayName = string.IsNullOrWhiteSpace(displayName) ? null : displayName.Trim(),
                CreatedAt = now.UtcDateTime,
                UpdatedAt = now.UtcDateTime,
                LastSignedInAt = now,
            };

            try
            {
                await _userRepository.InsertAsync(user).ConfigureAwait(false);
            }
            catch (SQLiteException ex) when (IsUniqueConstraintViolation(ex))
            {
                // Race condition: another caller inserted the same e-mail
                // between our check and our insert.  Surface a friendly
                // error rather than a 500-class exception.
                _logger.LogWarning(ex, "Sign-up lost the unique-email race for {Email}", normalisedEmail);
                return Result<UserDto>.Failure(
                    new InvalidOperationException("E-mail is already registered."),
                    "An account with this e-mail already exists.");
            }

            AuthToken token = await _tokenManager.IssueAsync(user.Id).ConfigureAwait(false);
            await _tokenManager.StoreAsync(token).ConfigureAwait(false);
            await UpdateCurrentUserAsync(ToDto(user, now), token, cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("User {UserId} signed up ({Email})", user.Id, user.Email);
            return Result<UserDto>.Success(_currentUser!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during sign-up for {Email}", email);
            return Result<UserDto>.Failure(ex);
        }
        finally
        {
            _sessionLock.Release();
        }
    }

    /// <inheritdoc />
    public async Task<Result> SignOutAsync(CancellationToken cancellationToken = default)
    {
        await _sessionLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            _tokenManager.Clear();
            await UpdateCurrentUserAsync(null, null, cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("User signed out");
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during sign-out");
            return Result.Failure(ex);
        }
        finally
        {
            _sessionLock.Release();
        }
    }

    /// <inheritdoc />
    public Task<Result> RequestPasswordResetAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        if (!Helpers.IsValidEmail(email))
        {
            return Task.FromResult(Result.Failure(
                new ArgumentException("E-mail address is not valid.", nameof(email)),
                "Please enter a valid e-mail address."));
        }

        // The local-only authentication service has no remote mailer.  The
        // method always succeeds (and logs the request) so the UI can show
        // a neutral "If an account exists, an e-mail has been sent" message
        // without leaking which addresses are registered.
        _logger.LogInformation("Password reset requested for {Email} (no remote mailer available)", email);
        return Task.FromResult(Result.Success());
    }

    /// <summary>
    /// Re-hydrates the in-process <see cref="CurrentUser"/> from a token
    /// already persisted in <see cref="Microsoft.Maui.Storage.SecureStorage"/>.
    /// Called by the splash page on application start-up to honour the
    /// "Authentication state persists on app restart" acceptance criterion.
    /// </summary>
    /// <returns>
    /// A successful <see cref="Result{T}"/> when a valid token is present,
    /// or a failed <see cref="Result{T}"/> with a human-readable
    /// <see cref="Result{T}.ErrorMessage"/> otherwise.
    /// </returns>
    public async Task<Result<UserDto>> RestoreSessionAsync(CancellationToken cancellationToken = default)
    {
        await _sessionLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            AuthToken? token = await _tokenManager.GetCurrentTokenAsync().ConfigureAwait(false);
            if (token is null)
            {
                return Result<UserDto>.Failure(
                    new InvalidOperationException("No persisted token."),
                    "No active session.");
            }

            TokenValidationResult<TokenManager.TokenPayload> validation =
                await _tokenManager.ValidateAsync(token.AccessToken, "access").ConfigureAwait(false);
            if (!validation.IsSuccess)
            {
                _logger.LogInformation("Discarding persisted session: {Reason}", validation.ErrorMessage);
                _tokenManager.Clear();
                await UpdateCurrentUserAsync(null, null, cancellationToken).ConfigureAwait(false);
                return Result<UserDto>.Failure(
                    new InvalidOperationException(validation.ErrorMessage!),
                    "Session expired. Please sign in again.");
            }

            User? user = await _userRepository
                .GetByIdAsync(validation.Value!.Sub, includeSoftDeleted: false)
                .ConfigureAwait(false);
            if (user is null)
            {
                _logger.LogWarning("Persisted token refers to missing user {UserId}", validation.Value!.Sub);
                _tokenManager.Clear();
                await UpdateCurrentUserAsync(null, null, cancellationToken).ConfigureAwait(false);
                return Result<UserDto>.Failure(
                    new KeyNotFoundException("User no longer exists."),
                    "Account no longer exists. Please sign in again.");
            }

            await UpdateCurrentUserAsync(ToDto(user, user.LastSignedInAt), token, cancellationToken)
                .ConfigureAwait(false);
            _logger.LogInformation("Restored session for user {UserId}", user.Id);
            return Result<UserDto>.Success(_currentUser!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while restoring session");
            return Result<UserDto>.Failure(ex);
        }
        finally
        {
            _sessionLock.Release();
        }
    }

    /// <summary>
    /// Refreshes the in-memory access token using the currently-stored
    /// refresh token.  Returns a failed <see cref="Result{T}"/> when no
    /// refresh token is available or it has expired.
    /// </summary>
    public async Task<Result<AuthToken>> RefreshTokenAsync(CancellationToken cancellationToken = default)
    {
        await _sessionLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            AuthToken? current = await _tokenManager.GetCurrentTokenAsync().ConfigureAwait(false);
            if (current is null)
            {
                return Result<AuthToken>.Failure(
                    new InvalidOperationException("No refresh token available."),
                    "Not signed in.");
            }

            TokenValidationResult<AuthToken> refreshed =
                await _tokenManager.RefreshAsync(current.RefreshToken).ConfigureAwait(false);
            if (!refreshed.IsSuccess)
            {
                _logger.LogInformation("Refresh failed: {Reason}", refreshed.ErrorMessage);
                _tokenManager.Clear();
                await UpdateCurrentUserAsync(null, null, cancellationToken).ConfigureAwait(false);
                return Result<AuthToken>.Failure(
                    new InvalidOperationException(refreshed.ErrorMessage!),
                    "Session expired. Please sign in again.");
            }

            await _tokenManager.StoreAsync(refreshed.Value!).ConfigureAwait(false);
            _currentToken = refreshed.Value;
            _logger.LogDebug("Refreshed access token for user {UserId}", _currentUser?.Id);
            return Result<AuthToken>.Success(refreshed.Value!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while refreshing token");
            return Result<AuthToken>.Failure(ex);
        }
        finally
        {
            _sessionLock.Release();
        }
    }

    private Task UpdateCurrentUserAsync(
        UserDto? user,
        AuthToken? token,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _currentUser = user;
        _currentToken = token;

        EventHandler<UserDto?>? handler = SignedInChanged;
        if (handler is null)
        {
            return Task.CompletedTask;
        }

        // SignedInChanged subscribers are documented as expecting to be
        // called on the UI thread, but the service is invoked from arbitrary
        // async contexts.  We hop to the main thread when a dispatcher is
        // available (i.e. when running inside a MAUI host) and invoke
        // synchronously otherwise so unit tests without a dispatcher still
        // observe the event.
        if (MainThread.IsMainThread)
        {
            handler.Invoke(this, user);
            return Task.CompletedTask;
        }

        try
        {
            return MainThread.InvokeOnMainThreadAsync(() => handler.Invoke(this, user));
        }
        catch (InvalidOperationException)
        {
            // No dispatcher (unit test environment).  Invoke inline.
            handler.Invoke(this, user);
            return Task.CompletedTask;
        }
    }

    private static UserDto ToDto(User user, DateTimeOffset? lastSignedInAt) => new()
    {
        Id = user.Id,
        Email = user.Email,
        DisplayName = user.DisplayName,
        LastSignedInAt = lastSignedInAt,
    };

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="ex"/> is the
    /// SQLite "UNIQUE constraint failed" error.  The check inspects both the
    /// <see cref="SQLiteException.Result"/> enum value and the textual
    /// message so it stays robust against sqlite-net / SQLitePCL.raw version
    /// drift (the extended <c>SQLITE_CONSTRAINT_UNIQUE</c> code 2067 is not
    /// surfaced through the standard <c>Result</c> enum on every platform).
    /// </summary>
    private static bool IsUniqueConstraintViolation(SQLiteException ex)
    {
        if (ex.Result == SQLite3.Result.Constraint
            && ex.Message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return ex.Message.Contains("UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase);
    }
}
