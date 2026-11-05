using WardrobeAI.Models;

namespace WardrobeAI.Repositories;

/// <summary>
/// Persistence boundary for the <see cref="ClothingItem"/> aggregate (SDS §8).
/// Used by the Closet feature (gallery + Add Clothing popup) and by the AI
/// outfit / shopping services that need to enumerate a user's wardrobe.
/// </summary>
public interface IClothingItemRepository
{
    /// <summary>
    /// Returns every clothing item in the closet, ordered most-recently-added
    /// first. Returns an empty list when no items have been added yet.
    /// </summary>
    Task<List<ClothingItem>> GetAllForUserAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>Loads a single clothing item by id, or null if not found.</summary>
    Task<ClothingItem?> GetByIdAsync(int itemId, CancellationToken cancellationToken = default);

    /// <summary>Persists a new clothing item and returns the saved entity (with assigned id).</summary>
    Task<ClothingItem> AddAsync(ClothingItem item, CancellationToken cancellationToken = default);

    /// <summary>Updates an existing clothing item.</summary>
    Task UpdateAsync(ClothingItem item, CancellationToken cancellationToken = default);

    /// <summary>Deletes a clothing item by id. No-op if the id does not exist.</summary>
    Task DeleteAsync(int itemId, CancellationToken cancellationToken = default);
}
