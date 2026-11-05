namespace WardrobeAI.Models;

/// <summary>
/// A single garment in the user's closet. Columns per SDS §8.
/// ImageUrl is never sent to the LLM (large payload, no LLM value).
/// </summary>
public class ClothingItem
{
    public int ItemId { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }

    /// <summary>
    /// Base64-encoded image (gallery / camera capture). PII-tier payload
    /// (can be large) and never sent to the LLM.
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>Display name chosen by the user (e.g. "Navy Oxford").</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Free-form category label sourced from <see cref="Constants.ClothingOptions.Categories"/>
    /// (e.g. "T-Shirts", "Kurtas", "Sarees"). Stored as a plain string so the
    /// catalog can be extended without a schema migration.
    /// </summary>
    public string Category { get; set; } = string.Empty;

    public string Color { get; set; } = string.Empty;
    public string Pattern { get; set; } = "Solid";

    /// <summary>Material / fabric label sourced from <see cref="Constants.ClothingOptions.Materials"/>.</summary>
    public string Fabric { get; set; } = "Cotton";
    public string? Brand { get; set; }
    public string Season { get; set; } = "All";
    public string Style { get; set; } = "Casual";
    public string Occasion { get; set; } = "Casual";

    public int UsageCount { get; set; }
    public bool Favorite { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;

    public ICollection<OutfitItem> OutfitItems { get; set; } = new List<OutfitItem>();
}
