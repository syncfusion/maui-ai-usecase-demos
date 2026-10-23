using System.Globalization;

namespace SmartVehicleCare.Converters;

public sealed class WidthToBooleanConverter : IValueConverter
{
    private const double DesktopBreakpoint = 700;
    private const double WideDesktopBreakpoint = 900;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var width = value is double numericWidth ? numericWidth : 0;
        var mode = parameter?.ToString();
        if (string.Equals(mode, "mobile", StringComparison.OrdinalIgnoreCase))
            return width < DesktopBreakpoint;

        if (string.Equals(mode, "bottom-nav", StringComparison.OrdinalIgnoreCase))
            return width < DesktopBreakpoint;

        if (string.Equals(mode, "compact-desktop", StringComparison.OrdinalIgnoreCase))
            return width < DesktopBreakpoint;

        if (string.Equals(mode, "wide", StringComparison.OrdinalIgnoreCase))
            return width >= WideDesktopBreakpoint;

        return width >= DesktopBreakpoint;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}