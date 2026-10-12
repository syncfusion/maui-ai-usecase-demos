using System.Globalization;
using Todo.Utilities;

namespace Todo.Converters;

/// <summary>
/// Converts a <see cref="DateTimeOffset"/> or <see cref="DateTime"/> value
/// to a short, locale-aware date string for the task-card metadata row.
/// </summary>
/// <remarks>
/// <para>
/// Uses <see cref="DateFormats.Short"/> (the application-wide "d" pattern)
/// by default, but can be overridden with <c>ConverterParameter="time"</c>
/// to emit the short-time pattern from <see cref="DateFormats"/> instead.
/// </para>
/// <para>
/// Null inputs are surfaced as an empty string so the binding engine never
/// raises a binding error on an empty metadata row.
/// </para>
/// </remarks>
public sealed class DateToShortConverter : IValueConverter
{
    /// <summary>Convenience <c>ConverterParameter</c> to emit the time pattern.</summary>
    public const string TimeParameter = "time";

    /// <inheritdoc/>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return string.Empty;
        }

        bool emitTime = parameter is string s
            && string.Equals(s, TimeParameter, StringComparison.OrdinalIgnoreCase);

        string pattern = emitTime ? Constants.DateFormats.ShortTime : Constants.DateFormats.Short;

        DateTimeOffset? dto = value switch
        {
            DateTimeOffset offset => offset,
            DateTime dt            => new DateTimeOffset(DateTime.SpecifyKind(dt, DateTimeKind.Utc)),
            _                      => null,
        };

        if (dto is null)
        {
            return string.Empty;
        }

        try
        {
            return dto.Value.ToLocalTime().ToString(pattern, CultureInfo.CurrentCulture);
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    /// <inheritdoc/>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException("DateToShortConverter only supports Convert.");
}