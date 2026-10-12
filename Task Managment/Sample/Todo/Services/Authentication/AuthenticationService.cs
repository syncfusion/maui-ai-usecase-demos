using Microsoft.Extensions.Logging;
using Todo.Models.DTOs;
using Todo.Services.Interfaces;
using Todo.Utilities;

namespace Todo.Services.Authentication;

/// <summary>
/// Placeholder implementation of <see cref="IAuthenticationService"/>.  The
/// real implementation is added in Module 2 (Task 2-01) — this skeleton
/// exists so that the DI container can resolve the service in Task 1-05 and
/// every later feature module compiles against the full interface surface.
/// </summary>
public sealed class AuthenticationService : IAuthenticationService
{
    private readonly ILogger<AuthenticationService> _logger;

    /// <summary>
    /// Initialises a new instance of the <see cref="AuthenticationService"/>
    /// class.  The <see cref="ILogger{T}"/> is injected by the DI container.
    /// </summary>
    /// <param name="logger">Logger used for structured diagnostics.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is <see langword="null"/>.</exception>
    public AuthenticationService(ILogger<AuthenticationService> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    /// <inheritdoc />
    public UserDto? CurrentUser => null;

    /// <inheritdoc />
    public bool IsSignedIn => false;

    /// <inheritdoc />
    public event EventHandler<UserDto?>? SignedInChanged;

    /// <inheritdoc />
    public Task<Result<UserDto>> SignInAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning(
            "AuthenticationService.SignInAsync is not implemented yet (Module 2 — Task 2-01).");
        throw new NotImplementedException(
            "Sign-in is implemented in Module 2 (Task 2-01).");
    }

    /// <inheritdoc />
    public Task<Result<UserDto>> SignUpAsync(
        string email,
        string password,
        string? displayName = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException(
            "Sign-up is implemented in Module 2 (Task 2-01).");
    }

    /// <inheritdoc />
    public Task<Result> SignOutAsync(CancellationToken cancellationToken = default)
    {
        SignedInChanged?.Invoke(this, null);
        return System.Threading.Tasks.Task.FromResult(Result.Success());
    }

    /// <inheritdoc />
    public Task<Result> RequestPasswordResetAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException(
            "Password reset is implemented in Module 2 (Task 2-01).");
    }
}
