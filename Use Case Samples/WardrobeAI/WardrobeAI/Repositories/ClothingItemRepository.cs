using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WardrobeAI.Data;
using WardrobeAI.Models;

namespace WardrobeAI.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IClothingItemRepository"/> over
/// <see cref="WardrobeDbContext"/>. Singleton-scoped via DI; the underlying
/// <see cref="WardrobeDbContext"/> is scoped, so a fresh instance is obtained
/// per call through the DbContext registered in <c>MauiProgram.cs</c>.
/// </summary>
public sealed class ClothingItemRepository : IClothingItemRepository
{
    private readonly WardrobeDbContext _context;
    private readonly ILogger<ClothingItemRepository> _logger;

    public ClothingItemRepository(WardrobeDbContext context, ILogger<ClothingItemRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<ClothingItem>> GetAllForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
        {
            return new List<ClothingItem>();
        }

        return await _context.ClothingItems
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public Task<ClothingItem?> GetByIdAsync(int itemId, CancellationToken cancellationToken = default)
        => _context.ClothingItems.FirstOrDefaultAsync(c => c.ItemId == itemId, cancellationToken);

    public async Task<ClothingItem> AddAsync(ClothingItem item, CancellationToken cancellationToken = default)
    {
        var added = await _context.ClothingItems.AddAsync(item, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation(
            "Created clothing item {ItemId} ({Name}) for user {UserId} in category {Category}",
            item.ItemId, item.Name, item.UserId, item.Category);
        return added.Entity;
    }

    public async Task UpdateAsync(ClothingItem item, CancellationToken cancellationToken = default)
    {
        _context.ClothingItems.Update(item);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Updated clothing item {ItemId}", item.ItemId);
    }

    public async Task DeleteAsync(int itemId, CancellationToken cancellationToken = default)
    {
        var existing = await _context.ClothingItems.FirstOrDefaultAsync(c => c.ItemId == itemId, cancellationToken);
        if (existing is null)
        {
            return;
        }

        _context.ClothingItems.Remove(existing);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Deleted clothing item {ItemId}", itemId);
    }
}
