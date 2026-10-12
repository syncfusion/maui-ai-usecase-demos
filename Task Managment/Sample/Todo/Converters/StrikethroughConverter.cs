using System.Globalization;

namespace Todo.Converters;

/// <summary>
/// Maps a <see cref="bool"/> to a MAUI <see cref="TextDecorations"/> value so
/// a label can be styled with a strikethrough when its associated task is
/// marked as completed.
/// </summary>
/// <remarks>
/// Returns <see cref="TextDecorations.Strikethrough"/> when the source is
/// <see langword="true"/>, <see cref="TextDecorations.None"/> otherwise.
/// Any non-bool source resolves to <see cref="TextDecorations.None"/> so
/// the converter is safe to bind to nullable / unknown values.
/// </remarks>
public sealed class StrikethroughConverter : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool source = value is bool b && b;
        return source ? TextDecorations.Strikethrough : TextDecorations.None;
    }

    /// <inheritdoc/>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TextDecorations decorations)
        {
            return decorations == TextDecorations.Strikethrough;
        }

        return false;
    }
}