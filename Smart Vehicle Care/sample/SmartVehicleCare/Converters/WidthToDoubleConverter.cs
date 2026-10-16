using System.Globalization;

namespace SmartVehicleCare.Converters;

public sealed class WidthToDoubleConverter : IValueConverter
{
    private const double MobileBreakpoint = 700;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var width = value is double numericWidth ? numericWidth : 0;
        var definitions = parameter?.ToString()?.Split(',', StringSplitOptions.TrimEntries) ?? [];
        if (definitions.Length != 2)
            return 0d;

        var selectedDefinition = width >= MobileBreakpoint ? definitions[0] : definitions[1];
        return double.Parse(selectedDefinition, culture);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}