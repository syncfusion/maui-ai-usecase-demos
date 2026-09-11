using System.Globalization;

namespace SmartVehicleCare.Converters;

/// <summary>
/// Converts a boolean to one of two colors (or brushes when target type is Brush).
/// </summary>
public class BoolToColorConverter : IValueConverter
{
    public Color TrueColor { get; set; } = Colors.Blue;
    public Color FalseColor { get; set; } = Colors.Gray;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var color = (value is bool b && b) ? TrueColor : FalseColor;

        // When the target property expects a Brush (e.g. Border.Stroke), wrap in SolidColorBrush
        if (targetType == typeof(Brush) || targetType == typeof(SolidColorBrush))
            return new SolidColorBrush(color);

        return color;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
