using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WardrobeAI.Data;
using WardrobeAI.Models;

namespace WardrobeAI.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IUserRepository"/> over <see cref="WardrobeDbContext"/>.
/// </summary>
public sealed class UserRepository : IUserRepository
{
    private readonly WardrobeDbContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(WardrobeDbContext context, ILogger<UserRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public Task<User?> GetAsync(int userId, CancellationToken cancellationToken = default)
        => _context.Users.FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        var added = await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Created user {UserId} with gender {Gender}", user.UserId, user.Gender);
        return added.Entity;
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Updated user {UserId}", user.UserId);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
