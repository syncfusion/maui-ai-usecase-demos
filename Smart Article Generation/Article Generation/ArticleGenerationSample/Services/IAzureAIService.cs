using System.Threading.Tasks;

namespace ArticleGenerationSample
{
    /// <summary>
    /// Abstraction for Azure AI operations used by view models and other services.
    /// </summary>
    public interface IAzureAIService
    {
        /// <summary>
        /// Initialize or reinitialize the underlying client/state if credentials are available.
        /// </summary>
        void InitializeClient();

        /// <summary>
        /// Request results from the AI pipeline.
        /// </summary>
        /// <param name="userPrompt">User-visible prompt used to select offline samples.</param>
        /// <param name="userAIPrompt">AI steering prompt passed to the service.</param>
        /// <returns>String result (HTML/Markdown) from the AI service.</returns>
        Task<string> GetResultsFromAI(string userPrompt, string userAIPrompt);
    }
}
