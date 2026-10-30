using Microsoft.Extensions.Logging;

namespace WardrobeAI.Services;

/// <summary>
/// Preference-backed <see cref="IBudgetService"/>. Backed by MAUI
/// <c>Preferences</c> so the value survives app restarts without a
/// database migration.
/// </summary>
public sealed class BudgetService : IBudgetService
{
    private const decimal DefaultMonthlyBudgetUsd = 200m;
    private const string PreferenceKeyFormat = "monthly_budget_user_{0}";

    private readonly ILogger<BudgetService> _logger;

    public BudgetService(ILogger<BudgetService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public decimal GetMonthlyBudget(int userId, decimal defaultValue = DefaultMonthlyBudgetUsd)
    {
        var key = FormatKey(userId);
        if (!Preferences.Default.ContainsKey(key))
        {
            return defaultValue;
        }

        var raw = Preferences.Default.Get(key, 0m);
        return raw > 0 ? raw : defaultValue;
    }

    /// <inheritdoc />
    public void SetMonthlyBudget(int userId, decimal value)
    {
        if (value <= 0m)
        {
            _logger.LogWarning("Ignoring invalid monthly budget {Value} for user {UserId}", value, userId);
            return;
        }

        Preferences.Default.Set(FormatKey(userId), value);
        _logger.LogInformation("Set monthly budget for user {UserId} to {Value} USD", userId, value);
    }

    /// <inheritdoc />
    public void ClearMonthlyBudget(int userId)
    {
        Preferences.Default.Remove(FormatKey(userId));
        _logger.LogInformation("Cleared monthly budget override for user {UserId}", userId);
    }

    /// <inheritdoc />
    public decimal GetDefaultBudget() => DefaultMonthlyBudgetUsd;

    private static string FormatKey(int userId) => string.Format(PreferenceKeyFormat, userId);
}
