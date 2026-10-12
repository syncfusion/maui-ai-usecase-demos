using System.Globalization;
using Todo.Utilities;

namespace Todo.Converters;

/// <summary>
/// Converts a <see cref="DateTime"/>, nullable <see cref="DateTime"/>, or
/// <see cref="DateTimeOffset"/> to its culture-specific string
/// representation.  The format is supplied as the <c>ConverterParameter</c>
/// (for example <c>"d"</c> for short date, <c>"t"</c> for short time, or
/// <c>"g"</c> for general date+time).  When no parameter is supplied the
/// short date pattern <c>"d"</c> is used, matching
/// <see cref="Constants.DateFormats.Short"/>.
/// </summary>
/// <remarks>
/// Null, empty and unparseable values yield <see cref="string.Empty"/> so
/// XAML bindings can rely on a non-null result.  <c>ConvertBack</c> parses
/// the string using the supplied <see cref="CultureInfo"/> and the same
/// format when one was provided.
/// </remarks>
public sealed class DateTimeToStringConverter : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return string.Empty;
        }

        DateTime? dateTime = value switch
        {
            DateTime dt => dt,
            DateTimeOffset dto => dto.LocalDateTime,
            _ => null,
        };

        if (!dateTime.HasValue)
        {
            return string.Empty;
        }

        string format = parameter as string ?? Constants.DateFormats.Short;
        return dateTime.Value.ToString(format, culture);
    }

    /// <inheritdoc/>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string text || string.IsNullOrWhiteSpace(text))
        {
            return DateTime.MinValue;
        }

        string format = parameter as string ?? Constants.DateFormats.Short;
        if (DateTime.TryParseExact(text, format, culture, DateTimeStyles.None, out var exact))
        {
            return exact;
        }

        return DateTime.TryParse(text, culture, DateTimeStyles.None, out var parsed)
            ? parsed
            : DateTime.MinValue;
    }
}
