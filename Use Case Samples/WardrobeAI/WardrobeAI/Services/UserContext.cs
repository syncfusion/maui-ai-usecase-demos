using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WardrobeAI.Data;
using WardrobeAI.Models;
using WardrobeAI.Repositories;

namespace WardrobeAI.Services;

/// <summary>
/// Auto-creates the singleton <see cref="AppConstants.DefaultUserName"/> user on
/// first run and caches its id for the rest of the session. The id is also
/// persisted to <c>Preferences</c> under <c>currentUserId</c> so other tooling
/// (e.g. <see cref="BudgetService"/>) can resolve the same user without
/// going through the context.
/// </summary>
public sealed class UserContext : IUserContext
{
    private const string CurrentUserIdPreferenceKey = "currentUserId";

    private readonly IUserRepository _userRepository;
    private readonly WardrobeDbContext _db;
    private readonly ILogger<UserContext> _logger;

    private readonly SemaphoreSlim _initLock = new(1, 1);
    private int _cachedUserId;
    private bool _initialized;

    public UserContext(
        IUserRepository userRepository,
        WardrobeDbContext db,
        ILogger<UserContext> logger)
    {
        _userRepository = userRepository;
        _db = db;
        _logger = logger;
    }

    public async Task EnsureUserAsync(CancellationToken cancellationToken = default)
    {
        if (_initialized)
        {
            return;
        }

        await _initLock.WaitAsync(cancellationToken);
        try
        {
            if (_initialized)
            {
                return;
            }

            // 1. Honour a previously persisted id if the User row is still alive.
            var fromPrefs = Preferences.Default.Get(CurrentUserIdPreferenceKey, 0);
            if (fromPrefs > 0)
            {
                var existing = await _userRepository.GetAsync(fromPrefs, cancellationToken);
                if (existing is not null)
                {
                    _cachedUserId = existing.UserId;
                    _initialized = true;
                    return;
                }
            }

            // 2. Reuse an existing "Alex Rivera" row (e.g. from a prior install).
            const string defaultName = AppConstants.DefaultUserName;
            var match = await _db.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Name == defaultName, cancellationToken);
            if (match is not null)
            {
                Preferences.Default.Set(CurrentUserIdPreferenceKey, match.UserId);
                _cachedUserId = match.UserId;
                _initialized = true;
                _logger.LogInformation(
                    "Reusing existing default user '{Name}' (UserId={UserId}).",
                    match.Name, match.UserId);
                return;
            }

            // 3. First run — seed the singleton default user.
            var user = new User
            {
                Name = defaultName,
                Email = string.Empty,
                StylePreference = "Casual",
                BudgetLevel = BudgetLevel.Medium,
                Gender = Gender.Unisex,
                AgeGroup = "22-35"
            };
            var saved = await _userRepository.AddAsync(user, cancellationToken);
            Preferences.Default.Set(CurrentUserIdPreferenceKey, saved.UserId);
            _cachedUserId = saved.UserId;
            _initialized = true;
            _logger.LogInformation(
                "Seeded default user '{Name}' (UserId={UserId}).",
                saved.Name, saved.UserId);
        }
        finally
        {
            _initLock.Release();
        }
    }

    public int GetCurrentUserId()
    {
        if (!_initialized)
        {
            return 0;

        }
        return _cachedUserId;
    }
}
