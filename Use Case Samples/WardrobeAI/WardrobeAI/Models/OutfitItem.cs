namespace WardrobeAI.Models;

/// <summary>
/// Join table linking an Outfit to its constituent ClothingItems (SDS §8).
/// Not sent to the LLM — resolved/flattened into itemIds[] before projection.
/// </summary>
public class OutfitItem
{
    public int OutfitItemId { get; set; }

    public int OutfitId { get; set; }
    public Outfit? Outfit { get; set; }

    public int ClothingItemId { get; set; }
    public ClothingItem? ClothingItem { get; set; }
}
