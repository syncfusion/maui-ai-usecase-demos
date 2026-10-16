using System.Globalization;

namespace SmartVehicleCare.Converters;

public sealed class WidthToThicknessConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var width = value is double numericWidth ? numericWidth : 0;
        var definitions = parameter?.ToString()?.Split('|', StringSplitOptions.TrimEntries) ?? [];
        var definition = width >= 700
            ? definitions.ElementAtOrDefault(0)
            : width >= 480
                ? definitions.ElementAtOrDefault(1)
                : definitions.ElementAtOrDefault(2);

        if (string.IsNullOrWhiteSpace(definition))
            return new Thickness(16);

        var values = definition.Split(',', StringSplitOptions.TrimEntries);
        if (values.Length != 4 || values.Any(valuePart => !double.TryParse(valuePart, NumberStyles.Float, culture, out _)))
            return new Thickness(16);

        return new Thickness(
            double.Parse(values[0], culture),
            double.Parse(values[1], culture),
            double.Parse(values[2], culture),
            double.Parse(values[3], culture));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}