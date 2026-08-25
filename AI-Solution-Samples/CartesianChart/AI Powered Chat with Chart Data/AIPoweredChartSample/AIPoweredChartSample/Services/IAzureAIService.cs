namespace AIPoweredChartSample
{
    /// <summary>
    /// IAzureAIService is an interface that defines the contract for interacting with an Azure AI service to get results based on user prompts.
    /// </summary>
    public interface IAzureAIService
    {
        /// <summary>
        /// Method to get results from the Azure AI service based on a user-provided prompt.
        /// </summary>
        /// <param name="userAIPrompt"></param>
        /// <returns></returns>
        Task<string> GetResultsFromAI(string userAIPrompt);
    }
}
