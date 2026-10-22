using System.Globalization;

namespace WardrobeAI.Converters;

/// <summary>
/// Returns a <see cref="Thickness"/> for content padding that scales with the
/// current container width, so the page stays centered and the margin
/// shrinks gracefully as the window is resized.
/// </summary>
/// <remarks>
/// Mapping (window width → horizontal padding):
/// <list type="bullet">
///   <item><description>≥ 1200 px → 40 px</description></item>
///   <item><description>900–1199 px → 32 px</description></item>
///   <item><description>600–899 px → 24 px</description></item>
///   <item><description>&lt; 600 px → 16 px</description></item>
/// </list>
/// Vertical padding is fixed so scroll rhythm is preserved.
/// </remarks>
public sealed class AdaptivePaddingConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        double width = value switch
        {
            double d => d,
            int i => i,
            float f => f,
            _ => 0d,
        };

        double horizontal = width switch
        {
            >= 1200 => 40,
            >= 900 => 32,
            >= 600 => 24,
            _ => 16,
        };

        // Default vertical rhythm (matches the original `40,16,40,24` Desktop idiom).
        return new Thickness(horizontal, 16, horizontal, 24);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}