namespace WardrobeAI.Models;

/// <summary>
/// A calendar entry scheduling an Outfit for a specific date (SDS §8).
/// Feeds the upcoming-events channel of the shopping recommendation service.
///
/// Plans can be created in two modes:
///  - Stand-alone: a user enters a free-text <see cref="Name"/> + <see cref="Occasion"/>
///    + <see cref="PlanDate"/> directly from the Outfit Planner's FAB. <see cref="OutfitId"/>
///    is null and the snapshot strings power the Dashboard's Upcoming Plans card.
///  - Outfit-linked: an existing <see cref="Outfit"/> is scheduled for a date.
///    The Outfit's own <c>Name</c> and <c>Occasion</c> still take precedence on the Dashboard.
/// </summary>
public class PlannedOutfit
{
    public int PlanId { get; set; }

    /// <summary>
    /// Optional FK to <see cref="Outfit"/>. Null for stand-alone plans created from
    /// the Outfit Planner FAB before any outfit is generated.
    /// </summary>
    public int? OutfitId { get; set; }
    public Outfit? Outfit { get; set; }

    /// <summary>
    /// Owning user id, captured at creation time. Stand-alone plans (with
    /// <see cref="OutfitId"/> == null) use this column directly to scope queries;
    /// outfit-linked plans inherit ownership from <see cref="Outfit.UserId"/> and
    /// this column may be null.
    /// </summary>
    public int? UserId { get; set; }
    public User? User { get; set; }

    public DateTime PlanDate { get; set; }

    /// <summary>
    /// Snapshot of the plan's display title captured at creation time so the
    /// Dashboard can render stand-alone plans without joining to Outfit.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Snapshot of the plan's occasion ("Work", "Casual", "Party", etc.) captured at
    /// creation time. Drives the Material icon mapping on the Dashboard.
    /// </summary>
    public string Occasion { get; set; } = string.Empty;

    public string? Notes { get; set; }
}
