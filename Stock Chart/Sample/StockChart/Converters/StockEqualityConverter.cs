using System.Globalization;

namespace StockChart.Converters;

public sealed class StockEqualityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Equals(value, parameter);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class StockInequalityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => !Equals(value, parameter);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class StockSelectionBackgroundConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => (value is bool isSelected ? isSelected : Equals(value, parameter))
            ? Color.FromArgb("#DBEAFE")
            : (Application.Current?.RequestedTheme == AppTheme.Dark
                ? Color.FromArgb("#0F172A")
                : Color.FromArgb("#F8FAFC"));

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class InverseBooleanConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool booleanValue && !booleanValue;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool booleanValue && !booleanValue;
}

public sealed class WatchlistStarColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? Color.FromArgb("#F5B301") : Color.FromArgb("#475569");

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class MobilePickerItemAppearanceConverter : IMultiValueConverter
{
    public object Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        var item = values.ElementAtOrDefault(0)?.ToString();
        var selected = item is not null &&
            (string.Equals(item, values.ElementAtOrDefault(1)?.ToString(), StringComparison.Ordinal) ||
             string.Equals(item, values.ElementAtOrDefault(2)?.ToString(), StringComparison.Ordinal) ||
             string.Equals(item, values.ElementAtOrDefault(3)?.ToString(), StringComparison.Ordinal));

        return parameter?.ToString() switch
        {
            "Visible" => selected,
            "TextColor" => selected
                ? (Application.Current?.RequestedTheme == AppTheme.Dark ? Color.FromArgb("#93A9D7") : Color.FromArgb("#2563EB"))
                : (Application.Current?.RequestedTheme == AppTheme.Dark ? Color.FromArgb("#F8FAFC") : Color.FromArgb("#1F2937")),
            _ => selected
                ? (Application.Current?.RequestedTheme == AppTheme.Dark ? Color.FromArgb("#334155") : Color.FromArgb("#EFF6FF"))
                : Colors.Transparent
        };
    }

    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class MobilePickerBackgroundConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true
            ? (Application.Current?.RequestedTheme == AppTheme.Dark
                ? Color.FromArgb("#334155")
                : Color.FromArgb("#DBEAFE"))
            : Colors.Transparent;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
