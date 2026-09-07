namespace SmartAIDatePicker.AIService;

public interface IAzureOpenAIService
{
    string? LastError { get; }

    string LastRawResponse { get; }

    Task<string> GetCompletion(
        string prompt,
        CancellationToken cancellationToken = default);
}
