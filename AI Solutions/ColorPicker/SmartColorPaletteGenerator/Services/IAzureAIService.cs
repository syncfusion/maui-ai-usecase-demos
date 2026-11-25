using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SmartColorPaletteGenerator
{
    /// <summary>
    /// IAzureAIService is an interface that defines the contract for a service that interacts with the Azure OpenAI service to send user prompts and receive responses.
    /// </summary>
    public interface IAzureAIService
    {
        /// <summary>
        /// Method to send a user prompt to the Azure OpenAI service and receive a response.
        /// </summary>
        /// <param name="userAIPrompt"></param>
        /// <returns></returns>
        Task<string> GetResultsFromAI(string userAIPrompt);
    }
}
