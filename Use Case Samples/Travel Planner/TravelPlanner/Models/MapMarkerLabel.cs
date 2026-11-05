namespace TravelPlanner.Models;

/// <summary>
/// Lightweight wrapper that carries the display label for a map marker.
/// Used as the BindingContext of each MapMarker so the tooltip template
/// can bind to a single, predictable <see cref="Name"/> property regardless
/// of whether the source is a PlaceCard, a destination query string, or
/// the device location sentinel.
/// </summary>
public class MapMarkerLabel : Microsoft.Maui.Controls.BindableObject
{
    public static readonly Microsoft.Maui.Controls.BindableProperty SourceProperty =
        Microsoft.Maui.Controls.BindableProperty.Create(
            nameof(Source),
            typeof(object),
            typeof(MapMarkerLabel),
            null);

    public static readonly Microsoft.Maui.Controls.BindableProperty NameProperty =
        Microsoft.Maui.Controls.BindableProperty.Create(
            nameof(Name),
            typeof(string),
            typeof(MapMarkerLabel),
            string.Empty);

    public static readonly Microsoft.Maui.Controls.BindableProperty DescriptionProperty =
        Microsoft.Maui.Controls.BindableProperty.Create(
            nameof(Description),
            typeof(string),
            typeof(MapMarkerLabel),
            string.Empty);

    public static readonly Microsoft.Maui.Controls.BindableProperty CategoryProperty =
        Microsoft.Maui.Controls.BindableProperty.Create(
            nameof(Category),
            typeof(string),
            typeof(MapMarkerLabel),
            string.Empty);

    public string Name
    {
        get => (string)GetValue(NameProperty);
        set => SetValue(NameProperty, value);
    }

    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public string Category
    {
        get => (string)GetValue(CategoryProperty);
        set => SetValue(CategoryProperty, value);
    }

    public object? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public string DisplayName => Name;
    public string DisplayCategory => Category;

    public static MapMarkerLabel From(object? source, string fallback, string? description = null, string? category = null)
    {
        var label = source as MapMarkerLabel ?? new MapMarkerLabel();
        label.Source = source;
        label.Category = category ?? fallback;
        label.Description = description ?? string.Empty;
        label.Name = fallback;

        if (source is null)
            return label;

        if (source is MapMarkerLabel already)
        {
            label.Name = string.IsNullOrWhiteSpace(already.Name) ? fallback : already.Name;
            label.Description = string.IsNullOrWhiteSpace(already.Description) ? label.Description : already.Description;
            label.Category = string.IsNullOrWhiteSpace(already.Category) ? label.Category : already.Category;
            return label;
        }

        var sourceType = source.GetType();

        var nameProp = sourceType.GetProperty("Name");
        if (nameProp?.GetValue(source) is string name && !string.IsNullOrWhiteSpace(name))
            label.Name = name;

        var titleProp = sourceType.GetProperty("Title");
        if (string.IsNullOrWhiteSpace(label.Name) && titleProp?.GetValue(source) is string title && !string.IsNullOrWhiteSpace(title))
            label.Name = title;

        var placeNameProp = sourceType.GetProperty("PlaceName");
        if (string.IsNullOrWhiteSpace(label.Name) && placeNameProp?.GetValue(source) is string placeName && !string.IsNullOrWhiteSpace(placeName))
            label.Name = placeName;

        var addressProp = sourceType.GetProperty("Address");
        if (addressProp?.GetValue(source) is string address && !string.IsNullOrWhiteSpace(address))
            label.Description = address;

        var categoryProp = sourceType.GetProperty("Category");
        if (categoryProp?.GetValue(source) is string cat && !string.IsNullOrWhiteSpace(cat))
            label.Category = cat;

        if (source is string s && !string.IsNullOrWhiteSpace(s))
            label.Name = s;

        return label;
    }
}
