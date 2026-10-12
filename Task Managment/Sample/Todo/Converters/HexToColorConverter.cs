using System.Globalization;

namespace Todo.Converters;

/// <summary>
/// Converts a hexadecimal colour string (for example <c>#5C4EAE</c>) into
/// a MAUI <see cref="Color"/>.  Designed for the sidebar / list-header icon
/// glyphs whose tint comes from the data layer as a string.
/// </summary>
/// <remarks>
/// <para>
/// The converter accepts the same shorthand that
/// <see cref="Color.FromArgb(string)"/> does: <c>#RGB</c>, <c>#RRGGBB</c>
/// and <c>#AARRGGBB</c> are all recognised.  An empty string, a <see langword="null"/>
/// source or a malformed value resolves to a transparent colour so the
/// glyph silently disappears rather than crashing the binding pipeline.
/// </para>
/// </remarks>
public sealed class HexToColorConverter : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string hex || string.IsNullOrWhiteSpace(hex))
        {
            return Colors.Transparent;
        }

        try
        {
            return Color.FromArgb(hex);
        }
        catch (Exception)
        {
            // Malformed hex — fall back to transparent rather than
            // surfacing the exception through the binding engine.
            return Colors.Transparent;
        }
    }

    /// <inheritdoc/>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Color color)
        {
            return string.Empty;
        }

        // Round-trip back to a #AARRGGBB hex string so two-way bindings
        // on colour values produce a stable representation.
        return $"#{color.ToArgbHex(true)}";
    }
}