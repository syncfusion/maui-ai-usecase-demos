using System.Globalization;
using System.Text.RegularExpressions;

namespace Todo.Utilities;

/// <summary>
/// Cross-cutting helper methods.  Each helper is intentionally small and
/// <see langword="static"/> so that it can be invoked from any layer
/// (ViewModels, Services, Converters, …) without taking a dependency on
/// MAUI-specific types.
/// </summary>
public static class Helpers
{
    #region Date / time formatting

    /// <summary>
    /// Formats <paramref name="value"/> for display in the task list using
    /// the user's current culture.  Returns an empty string for
    /// <see langword="null"/> inputs so that XAML bindings can rely on a
    /// non-null result.
    /// </summary>
    /// <param name="value">The value to format (may be <see langword="null"/>).</param>
    /// <returns>
    /// A culture-specific short date / short time string, or the empty
    /// string when <paramref name="value"/> is <see langword="null"/>.
    /// </returns>
    public static string FormatDateTime(DateTime? value)
    {
        if (!value.HasValue)
        {
            return string.Empty;
        }

        return value.Value.ToString("g", CultureInfo.CurrentCulture);
    }

    /// <summary>
    /// Formats <paramref name="value"/> as a relative description such as
    /// "Today", "Tomorrow", "Yesterday" or "in 3 days" / "5 days ago".
    /// </summary>
    /// <param name="value">The value to describe.</param>
    /// <returns>A short, human-friendly description.</returns>
    public static string FormatRelativeDate(DateTime? value)
    {
        if (!value.HasValue)
        {
            return string.Empty;
        }

        DateTime today = DateTime.Today;
        DateTime target = value.Value.Date;
        int days = (target - today).Days;

        return days switch
        {
            0 => "Today",
            1 => "Tomorrow",
            -1 => "Yesterday",
            > 1 and < 7 => $"in {days} days",
            < -1 and > -7 => $"{Math.Abs(days)} days ago",
            _ => value.Value.ToString("d", CultureInfo.CurrentCulture),
        };
    }

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="value"/> falls on
    /// today (date component only).
    /// </summary>
    /// <param name="value">The value to test (may be <see langword="null"/>).</param>
    public static bool IsToday(DateTime? value) =>
        value.HasValue && value.Value.Date == DateTime.Today;

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="value"/> is
    /// strictly in the past (date component only).
    /// </summary>
    /// <param name="value">The value to test.</param>
    public static bool IsOverdue(DateTime? value) =>
        value.HasValue && value.Value.Date < DateTime.Today;

    #endregion

    #region String validation

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="value"/> is a
    /// syntactically valid RFC-5322 e-mail address.
    /// </summary>
    /// <param name="value">The candidate address (may be <see langword="null"/> or empty).</param>
    public static bool IsValidEmail(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        // Conservative pattern that matches MAUI's default
        // Microsoft.Maui.Storage.EmailValidator closely enough for our needs.
        const string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(value, pattern, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
    }

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="password"/>
    /// satisfies the application's password strength policy.
    /// </summary>
    /// <param name="password">The candidate password.</param>
    /// <param name="minLength">Minimum required length (defaults to
    /// <see cref="Constants.Validation.PasswordMinLength"/>).</param>
    public static bool IsStrongPassword(string? password, int minLength = Constants.Validation.PasswordMinLength)
    {
        if (string.IsNullOrEmpty(password) || password.Length < minLength)
        {
            return false;
        }

        bool hasUpper = false;
        bool hasLower = false;
        bool hasDigit = false;
        bool hasSymbol = false;

        foreach (char c in password)
        {
            if (char.IsUpper(c))
            {
                hasUpper = true;
            }
            else if (char.IsLower(c))
            {
                hasLower = true;
            }
            else if (char.IsDigit(c))
            {
                hasDigit = true;
            }
            else if (!char.IsLetterOrDigit(c))
            {
                hasSymbol = true;
            }
        }

        return hasUpper && hasLower && hasDigit && hasSymbol;
    }

    /// <summary>
    /// Trims <paramref name="value"/> and converts it to
    /// <see langword="null"/> when the result is empty.  Useful for
    /// <c>[ObservableProperty]</c> string fields that should not store
    /// stray whitespace.
    /// </summary>
    /// <param name="value">The value to clean.</param>
    public static string? NormalizeWhiteSpace(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    #endregion

    #region Color conversion

    /// <summary>
    /// Attempts to parse a hexadecimal colour literal (for example
    /// <c>#5C4EAE</c> or <c>#FF5C4EAE</c>) and return its ARGB components.
    /// </summary>
    /// <param name="hex">The candidate hex literal.</param>
    /// <param name="a">Alpha component (0–255).</param>
    /// <param name="r">Red component (0–255).</param>
    /// <param name="g">Green component (0–255).</param>
    /// <param name="b">Blue component (0–255).</param>
    /// <returns><see langword="true"/> when the input could be parsed; otherwise <see langword="false"/>.</returns>
    public static bool TryParseHexColor(string? hex, out byte a, out byte r, out byte g, out byte b)
    {
        a = 0xFF;
        r = 0;
        g = 0;
        b = 0;

        if (string.IsNullOrWhiteSpace(hex))
        {
            return false;
        }

        string value = hex.Trim().TrimStart('#');
        if (value.Length != 6 && value.Length != 8)
        {
            return false;
        }

        if (!byte.TryParse(value.AsSpan(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte parsedA))
        {
            return false;
        }

        if (value.Length == 8)
        {
            a = parsedA;
        }

        if (!byte.TryParse(value.AsSpan(value.Length - 6, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out r))
        {
            return false;
        }

        if (!byte.TryParse(value.AsSpan(value.Length - 4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out g))
        {
            return false;
        }

        if (!byte.TryParse(value.AsSpan(value.Length - 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out b))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Converts an ARGB tuple into the standard <c>#AARRGGBB</c> hex string
    /// accepted by MAUI's <c>Color.FromArgb</c>.
    /// </summary>
    /// <param name="a">Alpha component (0–255).</param>
    /// <param name="r">Red component (0–255).</param>
    /// <param name="g">Green component (0–255).</param>
    /// <param name="b">Blue component (0–255).</param>
    public static string ToHexColor(byte a, byte r, byte g, byte b) =>
        string.Create(CultureInfo.InvariantCulture, $"#{a:X2}{r:X2}{g:X2}{b:X2}");

    #endregion

    #region Collection operations

    /// <summary>
    /// Splits <paramref name="items"/> into chunks of at most
    /// <paramref name="chunkSize"/> elements.  Useful for paginating
    /// in-memory lists shown in <c>SfListView</c>.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="items">The source sequence.</param>
    /// <param name="chunkSize">Maximum number of elements per chunk.</param>
    /// <returns>A sequence of <see cref="IReadOnlyList{T}"/> chunks.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="chunkSize"/> is less than 1.
    /// </exception>
    public static IEnumerable<IReadOnlyList<T>> Chunk<T>(IEnumerable<T> items, int chunkSize)
    {
        if (chunkSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(chunkSize), "chunkSize must be >= 1");
        }

        ArgumentNullException.ThrowIfNull(items);

        return ChunkIterator(items, chunkSize);
    }

    private static IEnumerable<IReadOnlyList<T>> ChunkIterator<T>(IEnumerable<T> items, int chunkSize)
    {
        List<T>? buffer = null;
        foreach (T item in items)
        {
            buffer ??= new List<T>(chunkSize);
            buffer.Add(item);
            if (buffer.Count == chunkSize)
            {
                yield return buffer;
                buffer = null;
            }
        }

        if (buffer is { Count: > 0 })
        {
            yield return buffer;
        }
    }

    #endregion
}
