using System.Globalization;

namespace TravelPlanner.Converters;

/// <summary>
/// Converts string to boolean: true if not null/empty, false otherwise.
/// Used for visibility binding in XAML.
/// </summary>
public class StringNotNullOrEmptyConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string str)
            return !string.IsNullOrWhiteSpace(str);

        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
