using System.Globalization;

namespace Todo.Converters;

/// <summary>
/// Returns the logical NOT of a boolean value. Returns <c>true</c>
/// when the input is not a boolean (e.g. <c>null</c>).
/// </summary>
public sealed class InvertedBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : true;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : false;
}