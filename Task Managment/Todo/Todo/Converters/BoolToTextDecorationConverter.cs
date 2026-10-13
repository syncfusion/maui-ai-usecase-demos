using System.Globalization;

namespace Todo.Converters;

/// <summary>
/// Maps a <see cref="bool"/> to a <see cref="TextDecorations"/> value
/// so that completed notes render with a strikethrough title.
/// </summary>
public sealed class BoolToTextDecorationConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool completed && completed ? TextDecorations.Strikethrough : TextDecorations.None;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}