namespace NutriLens.Services;

/// <summary>
/// Builds today's daily nutrition insight string, reading from the
/// cache first and falling back to a live AI call when needed.
/// </summary>
public interface IDailyInsightGenerator
{
    Task<string> GetTodayInsightAsync(
        CancellationToken cancellationToken = default);
}