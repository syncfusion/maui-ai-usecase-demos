using System.Globalization;

namespace Todo.Converters;

/// <summary>
/// Returns <c>true</c> when the value is NOT <c>null</c>.
/// </summary>
public sealed class NotNullToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is not null;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}