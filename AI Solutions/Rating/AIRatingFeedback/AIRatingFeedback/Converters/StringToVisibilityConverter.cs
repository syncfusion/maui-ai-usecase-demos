using System.Globalization;

namespace AIRatingFeedback.Converters;

/// <summary>
/// Shows the bound element when the string value is non-empty.
/// </summary>
public sealed class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => !string.IsNullOrWhiteSpace(value as string);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}