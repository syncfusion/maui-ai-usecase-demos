using System.Globalization;

namespace Todo.Converters;

/// <summary>
/// Maps a boolean (important) to a star glyph: filled when true,
/// outline when false.
/// </summary>
public sealed class BoolToStarGlyphConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b && b ? "\u2605" : "\u2606";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Maps a boolean (important) to a star color (gold) or muted gray.
/// </summary>
public sealed class BoolToStarColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b && b ? Color.FromArgb("#FFC107") : Color.FromArgb("#BDBDBD");

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}