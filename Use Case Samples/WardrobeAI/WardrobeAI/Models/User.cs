namespace WardrobeAI.Models;

/// <summary>
/// User gender options. Required at first-run onboarding (SDS §9.1.1 / wardrobe.prompt §3.5).
/// Drives the gender-aware wardrobe capability matrix and LLM rules overrides.
/// </summary>
public enum Gender
{
    Men,
    Women,
    Unisex,
    Undisclosed
}

/// <summary>
/// Budget level tiers used by the AI shopping recommendation service.
/// </summary>
public enum BudgetLevel
{
    Low,
    Medium,
    Premium,
    Luxury
}

/// <summary>
/// A registered user of the AI Wardrobe Assistant.
/// Columns per SDS §8. PII fields (Name, Email, ProfileImage) are never sent to the LLM.
/// </summary>
public class User
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    /// <summary>Base64-encoded profile picture (optional). PII — never sent to the LLM.</summary>
    public string? ProfileImage { get; set; }
    public string? Location { get; set; }
    public string? AgeGroup { get; set; }

    // Stored as the enum string for SQLite simplicity and direct contract projection.
    public string StylePreference { get; set; } = "Casual";
    public BudgetLevel BudgetLevel { get; set; } = BudgetLevel.Medium;

    /// <summary>Required gender. Defaults to Unisex so existing rows stay valid without a backfill prompt.</summary>
    public Gender Gender { get; set; } = Gender.Unisex;

    public ICollection<ClothingItem> ClothingItems { get; set; } = new List<ClothingItem>();
    public ICollection<Outfit> Outfits { get; set; } = new List<Outfit>();
    public ICollection<ShoppingRecommendation> ShoppingRecommendations { get; set; } = new List<ShoppingRecommendation>();
}
