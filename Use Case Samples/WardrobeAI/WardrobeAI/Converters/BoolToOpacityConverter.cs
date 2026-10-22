using System.Globalization;

namespace WardrobeAI.Converters;

/// <summary>
/// Converts a boolean to opacity: true -> 1.0, false -> 0.5.
/// Used to dim locked achievements and disabled items.
/// </summary>
public sealed class BoolToOpacityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? (b ? 1.0 : 0.5) : 0.5;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
