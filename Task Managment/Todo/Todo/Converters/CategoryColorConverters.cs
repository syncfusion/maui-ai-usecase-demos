using System.Globalization;
using Todo.Common;

namespace Todo.Converters;

/// <summary>
/// Maps a <see cref="NotesListCategory"/> value to its accent color
/// resource key, returning the <see cref="Color"/> directly so that
/// the consumer does not need an indirection.
/// </summary>
public sealed class CategoryToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var key = value is NotesListCategory c ? c.ToAccentColorKey() : "MyNotesColor";
        return Application.Current?.Resources.TryGetValue(key, out var resource) == true && resource is Color color
            ? color
            : Color.FromArgb("#757575");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}