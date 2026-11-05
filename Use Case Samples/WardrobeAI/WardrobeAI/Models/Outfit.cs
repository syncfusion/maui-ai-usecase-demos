namespace WardrobeAI.Models;

/// <summary>
/// A named combination of clothing items assembled for a specific occasion.
/// Columns per SDS §8. Score is client-computed by AI/Scoring, never by the LLM.
/// </summary>
public class Outfit
{
    public int OutfitId { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Occasion { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Score { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public ICollection<OutfitItem> OutfitItems { get; set; } = new List<OutfitItem>();
    public ICollection<PlannedOutfit> PlannedOutfits { get; set; } = new List<PlannedOutfit>();
}
