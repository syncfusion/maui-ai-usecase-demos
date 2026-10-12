namespace Todo.Models.DTOs;

/// <summary>
/// Opaque, signed access / refresh token pair returned by
/// <see cref="Todo.Services.Interfaces.IAuthenticationService"/> after a
/// successful sign-in, sign-up or token refresh.  The token is self-contained
/// (it embeds its expiry) so the <see cref="Todo.Services.Authentication.TokenManager"/>
/// can validate it without a database round-trip.
/// </summary>
/// <remarks>
/// <para>
/// The token follows the loosely-JWT structure of
/// <c>base64url(header).base64url(payload).base64url(signature)</c> but the
/// payload is a flat JSON document rather than the full RFC 7519 claim set
/// because the application does not have a remote identity provider yet.
/// </para>
/// <para>
/// <see cref="TokenType"/> is a discriminator so that
/// <see cref="Todo.Services.Authentication.TokenManager"/> can refuse to
/// accept, for example, a refresh token where an access token is expected.
/// </para>
/// </remarks>
public sealed record AuthToken
{
    /// <summary>Compact-serialised token (header.payload.signature).</summary>
    public string AccessToken { get; init; } = string.Empty;

    /// <summary>
    /// Refresh token used to obtain a new <see cref="AccessToken"/> without
    /// prompting the user to re-enter their credentials.
    /// </summary>
    public string RefreshToken { get; init; } = string.Empty;

    /// <summary>UTC expiry of the <see cref="AccessToken"/>.</summary>
    public DateTimeOffset ExpiresAt { get; init; }

    /// <summary>UTC expiry of the <see cref="RefreshToken"/>.</summary>
    public DateTimeOffset RefreshExpiresAt { get; init; }

    /// <summary>Discriminator of the token payload ("access" or "refresh").</summary>
    public string TokenType { get; init; } = "access";
}
