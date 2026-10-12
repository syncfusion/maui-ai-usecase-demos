namespace Todo.Models.DTOs;

/// <summary>
/// Payload accepted by <see cref="Todo.Services.Interfaces.IAuthenticationService.SignInAsync"/>.
/// Wraps the user-supplied credentials so the service signature remains
/// stable when additional fields (for example, a tenant identifier or a
/// remember-me flag) are introduced later.
/// </summary>
/// <param name="Email">E-mail address used as the sign-in identifier.</param>
/// <param name="Password">Clear-text password (never logged or persisted).</param>
/// <param name="RememberMe">
/// When <see langword="true"/>, the issued <see cref="AuthToken"/> is stored in
/// secure storage and the user is restored on next application start.
/// </param>
public sealed record LoginRequest(
    string Email,
    string Password,
    bool RememberMe = false);
