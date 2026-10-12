namespace Todo.Models.DTOs;

/// <summary>
/// Transport-layer representation of an authenticated user.  Used as the
/// <see cref="Result{T}.Value"/> of every <c>IAuthenticationService</c>
/// method.  Concrete model is fleshed out in Module 2.
/// </summary>
public sealed record UserDto
{
    /// <summary>Stable, opaque identifier for the user (provider-supplied).</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>The user's e-mail address (also used as the sign-in id).</summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>Optional human-readable name shown in the UI.</summary>
    public string? DisplayName { get; init; }

    /// <summary>UTC timestamp of the most recent successful sign-in.</summary>
    public DateTimeOffset? LastSignedInAt { get; init; }
}
