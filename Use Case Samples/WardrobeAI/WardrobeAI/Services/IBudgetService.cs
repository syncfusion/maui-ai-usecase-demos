namespace WardrobeAI.Services;

/// <summary>
/// Per-user monthly shopping budget. Stored in <c>Preferences</c> under
/// <c>monthly_budget_user_{userId}</c> so each account on the same device
/// keeps its own number (the value is per-user, not per-device).
/// </summary>
public interface IBudgetService
{
    /// <summary>
    /// Returns the current monthly budget in USD for <paramref name="userId"/>.
    /// Returns the <paramref name="defaultValue"/> (USD 200) when no budget
    /// has been set yet.
    /// </summary>
    decimal GetMonthlyBudget(int userId, decimal defaultValue = 200m);

    /// <summary>
    /// Persists a new monthly budget in USD for <paramref name="userId"/>.
    /// Values &lt;= 0 are rejected and the call is a no-op.
    /// </summary>
    void SetMonthlyBudget(int userId, decimal value);

    /// <summary>
    /// Removes the per-user override and falls back to the device-wide
    /// <see cref="GetDefaultBudget"/> value.
    /// </summary>
    void ClearMonthlyBudget(int userId);

    /// <summary>
    /// Default monthly budget (USD 200) used when no per-user value is set.
    /// </summary>
    decimal GetDefaultBudget();
}
