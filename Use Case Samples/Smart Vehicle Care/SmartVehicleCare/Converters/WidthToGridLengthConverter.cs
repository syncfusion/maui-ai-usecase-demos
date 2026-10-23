using System.Globalization;

namespace SmartVehicleCare.Converters;

public sealed class WidthToGridLengthConverter : IValueConverter
{
    private const double DesktopBreakpoint = 700;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var width = value is double numericWidth ? numericWidth : 0;
        if (string.Equals(parameter?.ToString(), "bottom-nav", StringComparison.OrdinalIgnoreCase))
            return new GridLength(width < DesktopBreakpoint ? 64 : 0);

        var layoutMode = parameter?.ToString();
        if (layoutMode is "kpi-column-3" or "kpi-column-4")
            return width < DesktopBreakpoint ? new GridLength(0) : new GridLength(1, GridUnitType.Star);

        if (layoutMode is "kpi-column-1" or "kpi-column-2")
            return new GridLength(1, GridUnitType.Star);

        if (width < DesktopBreakpoint)
            return new GridLength(0.001);

        var desktopDefinition = parameter?.ToString() ?? "220";
        if (desktopDefinition.EndsWith("*", StringComparison.Ordinal))
        {
            var weightText = desktopDefinition[..^1];
            var weight = string.IsNullOrWhiteSpace(weightText) ? 1 : double.Parse(weightText, culture);
            return new GridLength(weight, GridUnitType.Star);
        }

        return new GridLength(double.Parse(desktopDefinition, culture));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}