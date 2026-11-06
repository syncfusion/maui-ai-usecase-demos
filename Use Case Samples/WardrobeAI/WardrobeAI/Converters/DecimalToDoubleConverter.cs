using System.Globalization;

namespace WardrobeAI.Converters;

/// <summary>
/// Converts a <see cref="decimal"/> to a <see cref="double"/> so XAML bindings
/// on <c>Slider.Value</c> (which is a <c>double</c>) can bind to
/// <c>decimal</c> view-model properties without losing precision. Two-way
/// bindings round back to the nearest step via the slider's discrete
/// <c>Minimum</c> / <c>Maximum</c> range.
/// </summary>
public sealed class DecimalToDoubleConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value switch
        {
            decimal d => (double)d,
            double dd => dd,
            int i => (double)i,
            _ => 0d
        };

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value switch
        {
            double d => (decimal)d,
            decimal dd => dd,
            int i => (decimal)i,
            _ => 0m
        };
}
