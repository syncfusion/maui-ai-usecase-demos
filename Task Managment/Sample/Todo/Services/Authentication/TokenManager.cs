using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;
using Todo.Models.DTOs;

namespace Todo.Services.Authentication;

/// <summary>
/// Issues, signs, validates and refreshes <see cref="AuthToken"/> instances
/// used by <see cref="AuthenticationService"/>.  Tokens are self-contained
/// (a compact <c>header.payload.signature</c> blob with an embedded expiry)
/// and are persisted in <see cref="SecureStorage"/> so the user remains
/// signed-in across application restarts.
/// </summary>
/// <remarks>
/// <para>
/// The signing key is a 32-byte HMAC-SHA256 key generated on first use and
/// stored in <see cref="SecureStorage"/>.  In a production deployment the key
/// would be derived from the device's hardware-backed keystore, but
/// .NET MAUI does not expose that abstraction consistently across platforms
/// and the keychain-backed <see cref="SecureStorage"/> already provides the
/// "secret not readable by other apps" guarantee.
/// </para>
/// <para>
/// The manager exposes both a <c>GetCurrentTokenAsync</c> accessor (used by
/// the splash page on app start-up) and a <c>RefreshAsync</c> entry point
/// (used by the API layer when an access token has expired).
/// </para>
/// </remarks>
public sealed class TokenManager
{
    /// <summary>Default lifetime of the access token.</summary>
    public static readonly TimeSpan AccessTokenLifetime = TimeSpan.FromHours(1);

    /// <summary>Default lifetime of the refresh token.</summary>
    public static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(30);

    /// <summary>SecureStorage key under which the signing key is persisted.</summary>
    public const string SigningKeyStorageKey = "auth.token.signing-key";

    /// <summary>SecureStorage key under which the access token is persisted.</summary>
    public const string AccessTokenStorageKey = "auth.token.access";

    /// <summary>SecureStorage key under which the refresh token is persisted.</summary>
    public const string RefreshTokenStorageKey = "auth.token.refresh";

    private const string AccessTokenType = "access";
    private const string RefreshTokenType = "refresh";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    private readonly ILogger<TokenManager> _logger;
    private byte[]? _signingKey;

    /// <summary>
    /// Initialises a new instance of the <see cref="TokenManager"/> class.
    /// The signing key is loaded (or generated) lazily on the first
    /// <c>IssueAsync</c> call so the constructor stays cheap.
    /// </summary>
    /// <param name="logger">Logger for diagnostics.</param>
    public TokenManager(ILogger<TokenManager> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    /// <summary>
    /// Issues a fresh access / refresh token pair for the supplied
    /// <paramref name="userId"/>.  The caller is expected to persist the
    /// returned value through <see cref="StoreAsync"/> immediately.
    /// </summary>
    /// <param name="userId">Stable user identifier embedded in the token payload.</param>
    /// <param name="now">Override for the current UTC time (testability hook).</param>
    /// <returns>The freshly issued <see cref="AuthToken"/>.</returns>
    public async Task<AuthToken> IssueAsync(string userId, DateTimeOffset? now = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("User id must be a non-empty string.", nameof(userId));
        }

        await EnsureSigningKeyAsync().ConfigureAwait(false);

        DateTimeOffset issuedAt = now ?? DateTimeOffset.UtcNow;
        DateTimeOffset accessExpiry = issuedAt + AccessTokenLifetime;
        DateTimeOffset refreshExpiry = issuedAt + RefreshTokenLifetime;

        string access = await SignTokenAsync(new TokenPayload
        {
            Sub = userId,
            Type = AccessTokenType,
            Iat = issuedAt.ToUnixTimeSeconds(),
            Exp = accessExpiry.ToUnixTimeSeconds(),
        }).ConfigureAwait(false);

        string refresh = await SignTokenAsync(new TokenPayload
        {
            Sub = userId,
            Type = RefreshTokenType,
            Iat = issuedAt.ToUnixTimeSeconds(),
            Exp = refreshExpiry.ToUnixTimeSeconds(),
        }).ConfigureAwait(false);

        _logger.LogInformation(
            "Issued new auth tokens for user {UserId} (access expires {AccessExpiry:o}, refresh expires {RefreshExpiry:o})",
            userId,
            accessExpiry,
            refreshExpiry);

        return new AuthToken
        {
            AccessToken = access,
            RefreshToken = refresh,
            ExpiresAt = accessExpiry,
            RefreshExpiresAt = refreshExpiry,
            TokenType = AccessTokenType,
        };
    }

    /// <summary>
    /// Issues a new access token from a valid <paramref name="refreshToken"/>
    /// and rotates the refresh token at the same time (the old refresh token
    /// remains valid until its original expiry to keep the implementation
    /// simple — rotation revocation is a future enhancement).
    /// </summary>
    /// <param name="refreshToken">A previously issued refresh token.</param>
    /// <param name="now">Override for the current UTC time (testability hook).</param>
    /// <returns>
    /// A <see cref="TokenValidationResult{T}"/> wrapping the refreshed
    /// <see cref="AuthToken"/> on success or a human-readable error message
    /// on failure.
    /// </returns>
    public async Task<TokenValidationResult<AuthToken>> RefreshAsync(
        string refreshToken,
        DateTimeOffset? now = null)
    {
        TokenValidationResult<TokenPayload> validation = await ValidateAsync(refreshToken, RefreshTokenType, now)
            .ConfigureAwait(false);
        if (!validation.IsSuccess)
        {
            return TokenValidationResult<AuthToken>.Failure(validation.ErrorMessage!);
        }

        AuthToken fresh = await IssueAsync(validation.Value!.Sub, now).ConfigureAwait(false);
        return TokenValidationResult<AuthToken>.Success(fresh);
    }

    /// <summary>
    /// Validates <paramref name="token"/> and returns the embedded payload
    /// when the signature is correct, the token has not expired and the
    /// <see cref="TokenPayload.Type"/> matches the supplied
    /// <paramref name="expectedType"/>.
    /// </summary>
    /// <param name="token">The compact-serialised token to validate.</param>
    /// <param name="expectedType">Required <see cref="TokenPayload.Type"/> (e.g. <c>access</c>).</param>
    /// <param name="now">Override for the current UTC time (testability hook).</param>
    public async Task<TokenValidationResult<TokenPayload>> ValidateAsync(
        string? token,
        string expectedType,
        DateTimeOffset? now = null)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return TokenValidationResult<TokenPayload>.Failure("Token is missing.");
        }

        string[] parts = token.Split('.');
        if (parts.Length != 3)
        {
            return TokenValidationResult<TokenPayload>.Failure("Token is malformed.");
        }

        await EnsureSigningKeyAsync().ConfigureAwait(false);

        byte[] expectedSignature;
        byte[] providedSignature;
        try
        {
            expectedSignature = ComputeSignature(parts[0], parts[1]);
            providedSignature = Base64UrlDecode(parts[2]);
        }
        catch (FormatException)
        {
            return TokenValidationResult<TokenPayload>.Failure("Token signature is not valid base64url.");
        }

        if (!CryptographicOperations.FixedTimeEquals(expectedSignature, providedSignature))
        {
            return TokenValidationResult<TokenPayload>.Failure("Token signature is invalid.");
        }

        TokenPayload? payload;
        try
        {
            string json = Encoding.UTF8.GetString(Base64UrlDecode(parts[1]));
            payload = JsonSerializer.Deserialize<TokenPayload>(json, SerializerOptions);
        }
        catch (Exception ex) when (ex is FormatException or JsonException)
        {
            return TokenValidationResult<TokenPayload>.Failure("Token payload could not be deserialised.");
        }

        if (payload is null)
        {
            return TokenValidationResult<TokenPayload>.Failure("Token payload is empty.");
        }

        if (!string.Equals(payload.Type, expectedType, StringComparison.Ordinal))
        {
            return TokenValidationResult<TokenPayload>.Failure(
                $"Token type mismatch (expected '{expectedType}', got '{payload.Type}').");
        }

        DateTimeOffset nowUtc = now ?? DateTimeOffset.UtcNow;
        if (nowUtc.ToUnixTimeSeconds() >= payload.Exp)
        {
            return TokenValidationResult<TokenPayload>.Failure("Token has expired.");
        }

        return TokenValidationResult<TokenPayload>.Success(payload);
    }

    /// <summary>
    /// Returns the currently-stored access token, or <see langword="null"/>
    /// when no token is present.  Does not validate the token; callers should
    /// use <see cref="ValidateAsync"/> for that.
    /// </summary>
    public async Task<AuthToken?> GetCurrentTokenAsync()
    {
        string? access = await SecureStorage.GetAsync(AccessTokenStorageKey).ConfigureAwait(false);
        string? refresh = await SecureStorage.GetAsync(RefreshTokenStorageKey).ConfigureAwait(false);
        if (string.IsNullOrEmpty(access) || string.IsNullOrEmpty(refresh))
        {
            return null;
        }

        TokenValidationResult<TokenPayload> validation = await ValidateAsync(access, AccessTokenType)
            .ConfigureAwait(false);
        if (!validation.IsSuccess)
        {
            _logger.LogDebug("Stored access token is no longer valid: {Reason}", validation.ErrorMessage);
            return null;
        }

        // ExpiresAt / RefreshExpiresAt are not stored on disk; callers can
        // obtain them by re-validating the access and refresh tokens.
        return new AuthToken
        {
            AccessToken = access,
            RefreshToken = refresh,
            TokenType = AccessTokenType,
        };
    }

    /// <summary>
    /// Persists the supplied <see cref="AuthToken"/> in
    /// <see cref="SecureStorage"/> so the user remains signed-in across
    /// application restarts.
    /// </summary>
    /// <param name="token">The token to persist.</param>
    public async Task StoreAsync(AuthToken token)
    {
        ArgumentNullException.ThrowIfNull(token);

        await SecureStorage.SetAsync(AccessTokenStorageKey, token.AccessToken).ConfigureAwait(false);
        await SecureStorage.SetAsync(RefreshTokenStorageKey, token.RefreshToken).ConfigureAwait(false);

        _logger.LogDebug("Persisted auth tokens to secure storage");
    }

    /// <summary>
    /// Removes every persisted token from <see cref="SecureStorage"/>.
    /// Idempotent — a missing entry is not an error.
    /// </summary>
    public void Clear()
    {
        try
        {
            SecureStorage.Remove(AccessTokenStorageKey);
            SecureStorage.Remove(RefreshTokenStorageKey);
            _logger.LogDebug("Cleared auth tokens from secure storage");
        }
        catch (Exception ex)
        {
            // SecureStorage.Remove may throw on platforms where the entry
            // does not exist; that is not a real failure.
            _logger.LogDebug(ex, "Clear() swallowed a non-fatal exception");
        }
    }

    private async Task<string> SignTokenAsync(TokenPayload payload)
    {
        string header = Base64UrlEncode(Encoding.UTF8.GetBytes("{\"alg\":\"HS256\",\"typ\":\"JWT\"}"));
        string json = JsonSerializer.Serialize(payload, SerializerOptions);
        string body = Base64UrlEncode(Encoding.UTF8.GetBytes(json));
        string signature = Base64UrlEncode(ComputeSignature(header, body));
        return $"{header}.{body}.{signature}";
    }

    private byte[] ComputeSignature(string header, string body)
    {
        using var hmac = new HMACSHA256(_signingKey!);
        byte[] data = Encoding.UTF8.GetBytes($"{header}.{body}");
        return hmac.ComputeHash(data);
    }

    private async Task EnsureSigningKeyAsync()
    {
        if (_signingKey is not null)
        {
            return;
        }

        string? existing = await SecureStorage.GetAsync(SigningKeyStorageKey).ConfigureAwait(false);
        if (!string.IsNullOrEmpty(existing))
        {
            try
            {
                _signingKey = Convert.FromBase64String(existing);
                if (_signingKey.Length == 32)
                {
                    return;
                }

                _logger.LogWarning("Discarding signing key with unexpected length {Length}", _signingKey.Length);
            }
            catch (FormatException ex)
            {
                _logger.LogWarning(ex, "Discarding malformed signing key");
            }
        }

        _signingKey = RandomNumberGenerator.GetBytes(32);
        await SecureStorage.SetAsync(SigningKeyStorageKey, Convert.ToBase64String(_signingKey))
            .ConfigureAwait(false);
        _logger.LogInformation("Generated new auth token signing key (32 bytes)");
    }

    private static string Base64UrlEncode(ReadOnlySpan<byte> data) =>
        Convert.ToBase64String(data).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static byte[] Base64UrlDecode(string value)
    {
        string padded = value.Replace('-', '+').Replace('_', '/');
        switch (padded.Length % 4)
        {
            case 2: padded += "=="; break;
            case 3: padded += "="; break;
        }
        return Convert.FromBase64String(padded);
    }

    /// <summary>
    /// Compact JSON payload embedded in a <see cref="AuthToken"/>.
    /// </summary>
    public sealed class TokenPayload
    {
        /// <summary>Subject (user id).</summary>
        public string Sub { get; set; } = string.Empty;

        /// <summary>Token discriminator ("access" or "refresh").</summary>
        public string Type { get; set; } = AccessTokenType;

        /// <summary>Issued-at timestamp (Unix seconds).</summary>
        public long Iat { get; set; }

        /// <summary>Expiry timestamp (Unix seconds).</summary>
        public long Exp { get; set; }
    }
}

/// <summary>
/// Lightweight discriminated result returned by
/// <see cref="TokenManager.ValidateAsync"/>.  Modelled separately from
/// <see cref="Todo.Utilities.Result{T}"/> so the success path can carry a
/// strongly-typed payload without an extra layer of boxing.
/// </summary>
/// <typeparam name="T">The value returned on success.</typeparam>
public readonly record struct TokenValidationResult<T>
{
    /// <summary>Whether the validation succeeded.</summary>
    public bool IsSuccess { get; }

    /// <summary>The validated payload (when <see cref="IsSuccess"/> is <see langword="true"/>).</summary>
    public T? Value { get; }

    /// <summary>A human-readable failure reason (when <see cref="IsSuccess"/> is <see langword="false"/>).</summary>
    public string? ErrorMessage { get; }

    private TokenValidationResult(bool isSuccess, T? value, string? errorMessage)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorMessage = errorMessage;
    }

    /// <summary>Wraps a successful validation.</summary>
    public static TokenValidationResult<T> Success(T value) => new(true, value, null);

    /// <summary>Wraps a failed validation with a human-readable reason.</summary>
    public static TokenValidationResult<T> Failure(string errorMessage) => new(false, default, errorMessage);
}
