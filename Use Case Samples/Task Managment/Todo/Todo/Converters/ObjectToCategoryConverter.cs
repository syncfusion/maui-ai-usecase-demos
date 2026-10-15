using System.Globalization;
using Todo.Common;

namespace Todo.Converters;

/// <summary>
/// Converts between <see cref="object"/> (the type used by
/// <c>CollectionView.SelectedItem</c>) and <see cref="NotesListCategory"/>.
/// <c>null</c> values round-trip as <see cref="NotesListCategory.MyNotes"/>.
/// </summary>
public sealed class ObjectToCategoryConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is NotesListCategory c ? c : NotesListCategory.MyNotes;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is NotesListCategory c ? c : NotesListCategory.MyNotes;
}