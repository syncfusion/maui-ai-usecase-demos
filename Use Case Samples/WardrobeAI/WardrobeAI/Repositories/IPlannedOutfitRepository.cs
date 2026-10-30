using WardrobeAI.Models;

namespace WardrobeAI.Repositories;

/// <summary>
/// Persistence boundary for <see cref="PlannedOutfit"/>. Used by the Outfit Planner
/// to save stand-alone plans and by the Dashboard to read upcoming plans.
/// </summary>
public interface IPlannedOutfitRepository
{
    /// <summary>Persists a new plan and returns the saved entity.</summary>
    Task<PlannedOutfit> AddAsync(PlannedOutfit plan, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns today's and future plans for the given user, ordered by <see cref="PlannedOutfit.PlanDate"/>.
    /// Both outfit-linked and stand-alone plans are included. Caller can pass <c>0</c> for
    /// unauthenticated contexts (returns empty).
    /// </summary>
    Task<List<PlannedOutfit>> GetUpcomingForUserAsync(int userId, int take, CancellationToken cancellationToken = default);
}
