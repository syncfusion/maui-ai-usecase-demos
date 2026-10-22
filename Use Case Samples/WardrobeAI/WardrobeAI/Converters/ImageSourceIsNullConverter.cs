using System.Globalization;

namespace WardrobeAI.Converters;

/// <summary>
/// Returns <c>true</c> when the bound <see cref="ImageSource"/> is
/// <c>null</c>, <c>false</c> otherwise. Used by the shopping page to
/// conditionally reveal a placeholder image when the curated asset
/// referenced by wardrobe.json could not be resolved (or has not yet
/// loaded).
/// </summary>
public sealed class ImageSourceIsNullConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is null;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
