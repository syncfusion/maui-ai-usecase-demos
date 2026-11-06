namespace WardrobeAI.Models;

/// <summary>
/// A persisted shopping recommendation produced by the AI shopping service (SDS §8).
/// CompatibilityScore and FashionROI are client-computed, never LLM-produced.
/// </summary>
public class ShoppingRecommendation
{
    public int RecommendationId { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }

    public string ItemName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;

    public int CompatibilityScore { get; set; }
    public double FashionROI { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
