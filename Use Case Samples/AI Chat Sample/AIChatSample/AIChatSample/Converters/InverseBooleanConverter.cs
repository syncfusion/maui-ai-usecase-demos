using System.Globalization;

namespace AIChatSample.Converters;

/// <summary>
/// Inverts a boolean value. Returns <see cref="Visibility.Collapsed"/> when
/// the input is <c>true</c> and vice-versa when the target type is
/// <see cref="Visibility"/>; otherwise returns the inverted bool.
/// </summary>
public class InverseBooleanConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var b = value is bool bv && bv;
        if (targetType == typeof(bool) || targetType == typeof(bool?))
        {
            return !b;
        }
        return !b;
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var b = value is bool bv && bv;
        return !b;
    }
}
