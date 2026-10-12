namespace Todo.Models.DTOs;

/// <summary>
/// Payload accepted by <see cref="Todo.Services.Interfaces.IAuthenticationService.SignUpAsync"/>.
/// All fields are required (except <see cref="DisplayName"/>) and the
/// authentication service is responsible for re-validating the values before
/// creating a <see cref="User"/> record.
/// </summary>
/// <param name="Email">E-mail address to register.</param>
/// <param name="Password">Clear-text password (will be hashed before persistence).</param>
/// <param name="DisplayName">Optional friendly name shown in the UI.</param>
public sealed record SignUpRequest(
    string Email,
    string Password,
    string? DisplayName = null);
