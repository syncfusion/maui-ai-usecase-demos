using System.Threading.Tasks;

namespace SmartDailyDecisionCompanion
{
    /// <summary>
    /// IAzureAIService is an interface that defines the contract for a service that interacts with the Azure OpenAI service to send user prompts and receive responses.
    /// </summary>
    public interface IAzureAIService
    {
        /// <summary>
        /// Method to send a user prompt to the Azure OpenAI service and receive a response.
        /// </summary>
        /// <param name="userAIPrompt">The prompt to send to the model.</param>
        /// <returns>The model's response text.</returns>
        Task<string> GetResultsFromAI(string userAIPrompt);

        /// <summary>
        /// Validates the configured Azure OpenAI credentials. Call this explicitly (and await it)
        /// during startup instead of relying on constructor side effects.
        /// </summary>
        Task ValidateCredentialAsync();

        /// <summary>
        /// Gets a value indicating whether the credentials were accepted during the last validation attempt.
        /// </summary>
        bool IsCredentialValid { get; }
    }
}