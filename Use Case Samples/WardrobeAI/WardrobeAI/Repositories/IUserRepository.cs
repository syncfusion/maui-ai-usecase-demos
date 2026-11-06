using WardrobeAI.Models;

namespace WardrobeAI.Repositories;

/// <summary>
/// Persistence boundary for the User aggregate (SDS §8). Used by onboarding and profile.
/// </summary>
public interface IUserRepository
{
    Task<User?> GetAsync(int userId, CancellationToken cancellationToken = default);
    Task<User> AddAsync(User user, CancellationToken cancellationToken = default);
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
