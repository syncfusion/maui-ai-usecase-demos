using System.Globalization;

namespace TravelPlanner.Converters;  // 👈 matches xmlns in XAML

public class EqualsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value?.ToString() == parameter?.ToString();

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => (value is bool b && b) ? parameter?.ToString() : null!;
}
