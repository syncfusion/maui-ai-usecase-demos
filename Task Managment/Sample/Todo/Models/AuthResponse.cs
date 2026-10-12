using Todo.Models.DTOs;

namespace Todo.Models.DTOs;

/// <summary>
/// Response returned from a successful sign-in or sign-up call.  Wraps the
/// freshly issued <see cref="AuthToken"/> together with the
/// <see cref="UserDto"/> snapshot of the authenticated account so that callers
/// can populate the View layer without an extra round-trip.
/// </summary>
public sealed record AuthResponse
{
    /// <summary>The freshly issued access / refresh token pair.</summary>
    public AuthToken Token { get; init; } = new();

    /// <summary>Snapshot of the authenticated user.</summary>
    public UserDto User { get; init; } = new();
}
