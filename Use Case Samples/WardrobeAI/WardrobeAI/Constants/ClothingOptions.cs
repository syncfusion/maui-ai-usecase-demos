namespace WardrobeAI.Constants;

/// <summary>
/// Static catalog of selectable values for the seven clothing attribute fields
/// shown in the Add Clothing popup. Exposed as plain string lists so that the
/// Syncfusion chip groups can bind them directly via <c>ItemsSource</c>.
///
/// These values are persisted verbatim into the corresponding
/// <see cref="Models.ClothingItem"/> string columns
/// (<c>Category</c>, <c>Occasion</c>, <c>Style</c>, <c>Color</c>,
/// <c>Fabric</c>, <c>Season</c>, <c>Pattern</c>).
/// </summary>
public static class ClothingOptions
{
    /// <summary>Clothing categories (e.g. T-Shirts, Kurtas, Sarees).</summary>
    public static readonly IReadOnlyList<string> Categories = new[]
    {
        "T-Shirts", "Shirts", "Polo Shirts", "Hoodies", "Sweatshirts",
        "Jackets", "Coats", "Blazers", "Sweaters", "Jeans", "Trousers",
        "Shorts", "Joggers", "Leggings", "Skirts", "Dresses", "Kurtas",
        "Sarees", "Ethnic Wear", "Activewear", "Sleepwear", "Innerwear",
        "Swimwear"
    };

    /// <summary>Occasion tags (e.g. Business, Wedding, Lounge Wear).</summary>
    public static readonly IReadOnlyList<string> Occasions = new[]
    {
        "Casual", "Formal", "Business", "Party Wear", "Wedding", "Festive",
        "Sports", "Travel", "Beach", "Lounge Wear"
    };

    /// <summary>Style tags (e.g. Slim Fit, Streetwear).</summary>
    public static readonly IReadOnlyList<string> Styles = new[]
    {
        "Classic", "Modern", "Minimalist", "Streetwear", "Vintage",
        "Oversized", "Slim Fit", "Regular Fit", "Relaxed Fit", "Athletic Fit"
    };

    /// <summary>Color tags (e.g. Black, Beige).</summary>
    public static readonly IReadOnlyList<string> Colors = new[]
    {
        "Black", "White", "Blue", "Red", "Green", "Yellow", "Pink",
        "Purple", "Brown", "Grey", "Beige"
    };

    /// <summary>Material / fabric tags (e.g. Cotton, Linen).</summary>
    public static readonly IReadOnlyList<string> Materials = new[]
    {
        "Cotton", "Linen", "Denim", "Polyester", "Wool", "Silk", "Rayon",
        "Leather", "Organic Cotton", "Bamboo Fabric"
    };

    /// <summary>Season tags (e.g. Summer, All Season).</summary>
    public static readonly IReadOnlyList<string> Seasons = new[]
    {
        "Summer", "Winter", "Spring", "Autumn", "All Season"
    };

    /// <summary>Pattern tags (e.g. Solid, Embroidered).</summary>
    public static readonly IReadOnlyList<string> Patterns = new[]
    {
        "Solid", "Striped", "Checked", "Printed", "Floral",
        "Graphic", "Geometric", "Embroidered"
    };
}
