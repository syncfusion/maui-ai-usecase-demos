using System.Globalization;

namespace Todo.Converters;

/// <summary>
/// Formats a nullable <see cref="DateTime"/> using a culture-aware
/// short date pattern. Returns an empty string for <c>null</c>.
/// </summary>
public sealed class DateTimeFormatConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTime dt)
            return dt.ToString("MMM d", culture);
        return string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}