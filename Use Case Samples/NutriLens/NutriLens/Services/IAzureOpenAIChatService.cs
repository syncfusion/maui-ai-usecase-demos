namespace NutriLens.Services;

/// <summary>
/// Single-turn chat completion against an Azure OpenAI deployment.
/// Used by the daily-insight pipeline.
/// </summary>
public interface IAzureOpenAIChatService
{
    Task<string> GetCompletionAsync(
        string prompt,
        CancellationToken cancellationToken = default);
}