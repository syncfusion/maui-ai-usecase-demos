using System.Globalization;

namespace Todo.Converters;

/// <summary>
/// Returns <c>true</c> when the integer value is greater than zero.
/// Used by the sidebar's "Lists" section header to show / hide
/// based on the number of user-defined lists (Module-05).
/// </summary>
public sealed class CountToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int count)
            return count > 0;
        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}