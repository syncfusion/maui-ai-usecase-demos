using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;

namespace AITaskPrioritization
{
    internal class AzureOpenAIBaseService
    {
        public static readonly string endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? string.Empty;
        public static readonly string key = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") ?? string.Empty;
        public static readonly string deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? string.Empty;
        private IChatClient? client;

        public bool IsCredentialValid { get; private set; } = false;

        public AzureOpenAIBaseService()
        {
            Initialize();
            _ = ValidateCredential();
        }

        private void Initialize()
        {
            try
            {
                client = new AzureOpenAIClient(new Uri(endpoint),new AzureKeyCredential(key)).AsChatClient(modelId: deploymentName);
            }
            catch
            {
                client = null;
            }
        }

        private async Task ValidateCredential()
        {
            try
            {
                if (client != null)
                {
                    await client.CompleteAsync("Test message");
                    IsCredentialValid = true;
                }
            }
            catch
            {
                IsCredentialValid = false;
            }
        }

        public async Task<string> GetAIResponse(string prompt)
        {
            try
            {
                if (!IsCredentialValid || client == null)
                    return string.Empty;

                var response = await client.CompleteAsync(prompt);

                return response?.ToString() ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
