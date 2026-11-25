using System.Globalization;

namespace SmartColorPaletteGenerator
{
    public sealed class ColorStringToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string text && Color.TryParse(text, out var color))
            {
                return color;
            }

            return Colors.Transparent;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is Color color ? color.ToArgbHex() : null;
        }
    }
}