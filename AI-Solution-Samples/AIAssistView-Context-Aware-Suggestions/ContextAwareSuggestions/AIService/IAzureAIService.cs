namespace ContextAwareSuggestions;

/// <summary>
/// Defines the contract for Azure AI services used to generate AI responses and context-aware suggestions for the AI AssistView.
/// </summary>
public interface IAzureAIService
{
    /// <summary>
    /// Sends the specified prompt to the Azure AI service and retrieves the generated response.
    /// </summary>
    /// <param name="prompt">
    /// The user prompt or query submitted to the AI service.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a <see cref="ContextSuggestionResponse"/> object with the AI-generated answer and related follow-up suggestions, or <see langword="null"/> if no response is returned.
    /// </returns>
    Task<ContextSuggestionResponse?> GetResponseAsync(string prompt);
}