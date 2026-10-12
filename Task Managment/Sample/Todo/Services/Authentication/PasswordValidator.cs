using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Todo.Utilities;

namespace Todo.Services.Authentication;

/// <summary>
/// Stateless helper that produces and verifies PBKDF2-SHA256 password hashes
/// and exposes the application-wide password-strength policy used by
/// <see cref="AuthenticationService"/>.
/// </summary>
/// <remarks>
/// <para>
/// The stored format is <c>pbkdf2-sha256$iterations$base64(salt)$base64(hash)</c>
/// so the algorithm and iteration count travel with the value.  This makes it
/// possible to upgrade the parameters (more iterations, switch to
/// <c>SHA512</c>, …) without invalidating every existing hash because the
/// validator picks the algorithm and parameters from the stored prefix.
/// </para>
/// <para>
/// <b>Why PBKDF2?</b>  The task description calls out PBKDF2 (or similar)
/// and PBKDF2 ships in <c>System.Security.Cryptography</c> on every platform
/// that .NET MAUI targets (Android, iOS, macOS, Windows) without an extra
/// native dependency.  200&nbsp;000 iterations of HMAC-SHA256 is the OWASP
/// 2023 recommendation for PBKDF2-SHA256 and is a comfortable fit for the
/// expected sign-in cadence (one hash per sign-in).
/// </para>
/// </remarks>
public sealed class PasswordValidator
{
    /// <summary>Default PBKDF2 iteration count (OWASP 2023 baseline for SHA-256).</summary>
    public const int DefaultIterations = 200_000;

    /// <summary>Length of the randomly generated salt, in bytes.</summary>
    public const int SaltLength = 16;

    /// <summary>Length of the derived key, in bytes.</summary>
    public const int HashLength = 32;

    private const string AlgorithmName = "pbkdf2-sha256";
    private const int MinIterations = 1_000;

    private readonly ILogger<PasswordValidator> _logger;
    private readonly int _iterations;

    /// <summary>
    /// Initialises a new instance of the <see cref="PasswordValidator"/>
    /// class with the supplied iteration count.  The count is clamped to a
    /// safe minimum so a misconfigured DI registration cannot downgrade
    /// existing hashes to a brute-forceable value.
    /// </summary>
    /// <param name="logger">Logger for diagnostics.</param>
    /// <param name="iterations">
    /// PBKDF2 iteration count.  Defaults to
    /// <see cref="DefaultIterations"/>.  Values below
    /// <see cref="MinIterations"/> are clamped.
    /// </param>
    public PasswordValidator(ILogger<PasswordValidator> logger, int iterations = DefaultIterations)
    {
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;
        _iterations = Math.Max(iterations, MinIterations);
    }

    /// <summary>
    /// Gets the iteration count actually used by the validator.  Exposed so
    /// that tests can assert the configured value and so that
    /// <see cref="AuthenticationService"/> can log the value once at start-up.
    /// </summary>
    public int Iterations => _iterations;

    /// <summary>
    /// Hashes <paramref name="password"/> using PBKDF2-SHA256 with a fresh
    /// random salt.  The returned string is safe to persist in the
    /// <c>Users.PasswordHash</c> column.
    /// </summary>
    /// <param name="password">The clear-text password.</param>
    /// <returns>
    /// A self-describing string of the form
    /// <c>pbkdf2-sha256$iterations$base64(salt)$base64(hash)</c>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="password"/> is <see langword="null"/> or
    /// empty.
    /// </exception>
    public string Hash(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            throw new ArgumentException("Password must not be empty.", nameof(password));
        }

        Span<byte> salt = stackalloc byte[SaltLength];
        RandomNumberGenerator.Fill(salt);

        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            _iterations,
            HashAlgorithmName.SHA256,
            HashLength);

        string result = string.Join(
            '$',
            AlgorithmName,
            _iterations.ToString(System.Globalization.CultureInfo.InvariantCulture),
            Convert.ToBase64String(salt),
            Convert.ToBase64String(hash));

        // Best-effort scrub of the derived key from managed memory.
        CryptographicOperations.ZeroMemory(hash);

        return result;
    }

    /// <summary>
    /// Verifies that <paramref name="password"/> matches the supplied
    /// <paramref name="storedHash"/> using a constant-time comparison so the
    /// function does not leak how many leading bytes match.
    /// </summary>
    /// <param name="password">The clear-text password entered by the user.</param>
    /// <param name="storedHash">The PBKDF2 string previously produced by <see cref="Hash"/>.</param>
    /// <returns>
    /// <see langword="true"/> when <paramref name="password"/> matches;
    /// <see langword="false"/> when it does not or when
    /// <paramref name="storedHash"/> is malformed.
    /// </returns>
    public bool Verify(string? password, string? storedHash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrWhiteSpace(storedHash))
        {
            return false;
        }

        string[] parts = storedHash.Split('$');
        if (parts.Length != 4 || parts[0] != AlgorithmName)
        {
            _logger.LogWarning("Discarding password hash with unknown algorithm prefix '{Prefix}'", parts[0]);
            return false;
        }

        if (!int.TryParse(parts[1], System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int iterations)
            || iterations < MinIterations)
        {
            _logger.LogWarning("Discarding password hash with invalid iteration count '{Iterations}'", parts[1]);
            return false;
        }

        byte[] salt;
        byte[] expected;
        try
        {
            salt = Convert.FromBase64String(parts[2]);
            expected = Convert.FromBase64String(parts[3]);
        }
        catch (FormatException ex)
        {
            _logger.LogWarning(ex, "Discarding password hash with malformed base64 segment");
            return false;
        }

        if (expected.Length != HashLength || salt.Length != SaltLength)
        {
            _logger.LogWarning("Discarding password hash with unexpected salt/hash length");
            return false;
        }

        byte[] candidate = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            HashLength);

        bool equal = CryptographicOperations.FixedTimeEquals(candidate, expected);

        // Scrub the candidate key from memory before returning.
        CryptographicOperations.ZeroMemory(candidate);

        return equal;
    }

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="password"/> is
    /// non-null, at least <see cref="Constants.Validation.PasswordMinLength"/>
    /// characters long and contains at least one upper-case letter,
    /// lower-case letter, digit and symbol.  Delegates to
    /// <see cref="Helpers.IsStrongPassword"/> so the rule stays in one place.
    /// </summary>
    /// <param name="password">The candidate password.</param>
    public static bool MeetsStrengthPolicy(string? password) =>
        Helpers.IsStrongPassword(password);
}
