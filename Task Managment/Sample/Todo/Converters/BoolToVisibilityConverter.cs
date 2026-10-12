using System.Globalization;

namespace Todo.Converters;

/// <summary>
/// Converts a <see cref="bool"/> to MAUI's <c>IsVisible</c> representation
/// (also a <see cref="bool"/>).  The naming reflects Microsoft MAUI's
/// <c>Visibility</c> concept so XAML bindings read naturally:
/// <c>IsVisible="{Binding IsLoading, Converter=...}"</c>.
/// </summary>
/// <remarks>
/// <para>
/// Pass <c>"invert"</c> as the <c>ConverterParameter</c> to invert the
/// mapping without registering a second converter:
/// <c>IsVisible="{Binding IsEmpty, Converter=..., ConverterParameter=invert}"</c>.
/// </para>
/// <para>
/// Non-bool values are treated as <see langword="false"/> (hidden) when
/// <c>ConverterParameter</c> is not <c>"invert"</c>, and as
/// <see langword="true"/> when it is.  This makes the converter safe to
/// bind to nullable or unknown source values without crashing.
/// </para>
/// </remarks>
public sealed class BoolToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Singleton-friendly parameter string that flips the mapping.
    /// </summary>
    public const string InvertParameter = "invert";

    /// <inheritdoc/>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool source = value is bool b && b;
        return IsInverted(parameter) ? !source : source;
    }

    /// <inheritdoc/>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // For two-way bindings on IsVisible the value is still a bool, so the
        // same inversion rule applies in reverse.
        bool source = value is bool b && b;
        return IsInverted(parameter) ? !source : source;
    }

    private static bool IsInverted(object? parameter) =>
        parameter is string s &&
        string.Equals(s, InvertParameter, StringComparison.OrdinalIgnoreCase);
}
