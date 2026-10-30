using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WardrobeAI.Data;
using WardrobeAI.Models;

namespace WardrobeAI.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IPlannedOutfitRepository"/> over <see cref="WardrobeDbContext"/>.
/// </summary>
public sealed class PlannedOutfitRepository : IPlannedOutfitRepository
{
    private readonly WardrobeDbContext _context;
    private readonly ILogger<PlannedOutfitRepository> _logger;

    public PlannedOutfitRepository(WardrobeDbContext context, ILogger<PlannedOutfitRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PlannedOutfit> AddAsync(PlannedOutfit plan, CancellationToken cancellationToken = default)
    {
        var added = await _context.PlannedOutfits.AddAsync(plan, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Created planned outfit {PlanId} for date {PlanDate:O}", plan.PlanId, plan.PlanDate);
        return added.Entity;
    }

    public async Task<List<PlannedOutfit>> GetUpcomingForUserAsync(int userId, int take, CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
        {
            return new List<PlannedOutfit>();
        }

        var today = DateTime.Today;

        // Include both stand-alone plans (owned via PlannedOutfit.UserId) and
        // outfit-linked plans (owned via Outfit.UserId). The Dashboard renders
        // stand-alone plans using the plan's own Name/Occasion snapshots.
        IQueryable<PlannedOutfit> query =
            from p in _context.PlannedOutfits.AsNoTracking()
            where p.PlanDate >= today
                  && ((p.UserId == userId)
                      || (p.Outfit != null && p.Outfit.UserId == userId))
            select p;

        query = query.OrderBy(p => p.PlanDate);

        if (take > 0)
        {
            query = query.Take(take);
        }

        return await query.ToListAsync(cancellationToken);
    }
}
