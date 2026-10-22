using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SmartDailyDecisionCompanion
{
    /// <summary>
    /// AzureBaseService is an abstract class that provides the foundational functionality for interacting with the Azure OpenAI service.
    /// </summary>
    public abstract class AzureBaseService
    {
        // TODO: Add your Azure OpenAI endpoint, deployment name, and API key before running the sample.
        // The endpoint must include the trailing slash, e.g. "https://<your-resource>.openai.azure.com/".
        private const string endpoint = "Enter your Azure OpenAI endpoint here";
        private const string deploymentName = "Enter your Azure OpenAI deployment name here";
        private const string key = "Enter your Azure OpenAI API key here";
        private const string apiVersion = "2024-02-15-preview";

        private static readonly Lazy<HttpClient> _httpClient = CreateHttpClient();

        private readonly SemaphoreSlim _validationLock = new(1, 1);

        private bool _isCredentialValid;
        private bool _isValidated;

        /// <summary>
        /// Gets a value indicating whether the Azure OpenAI credentials (endpoint, deployment, and API key)
        /// were accepted by the service during the last validation attempt.
        /// </summary>
        public bool IsCredentialValid => _isCredentialValid;

        private static Lazy<HttpClient> CreateHttpClient()
        {
            return new Lazy<HttpClient>(() =>
            {
                var client = new HttpClient();
                try
                {
                    client.BaseAddress = new Uri(endpoint, UriKind.Absolute);
                    client.DefaultRequestHeaders.Add("api-key", key);
                }
                catch
                {
                    // Placeholder credentials are not a valid absolute URI; validation reports the failure.
                }

                return client;
            });
        }

        /// <summary>
        /// Validates the Azure OpenAI credentials by sending a lightweight test request and caches the outcome.
        /// This performs I/O and must be awaited explicitly (see <see cref="IAzureAIService.ValidateCredentialAsync"/>);
        /// it is intentionally not invoked from a constructor.
        /// </summary>
        public async Task ValidateCredentialAsync()
        {
            if (_isValidated)
                return;

            await _validationLock.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_isValidated)
                    return;

                if (!IsConfigured())
                {
                    _isCredentialValid = false;
                    return;
                }

                var payload = new
                {
                    messages = new[] { new { role = "user", content = "ping" } },
                    max_completion_tokens = 5,
                };

                var json = JsonSerializer.Serialize(payload);
                var url = $"openai/deployments/{deploymentName}/chat/completions?api-version={apiVersion}";

                using var content = new StringContent(json, Encoding.UTF8, "application/json");
                using var resp = await _httpClient.Value.PostAsync(url, content).ConfigureAwait(false);

                // 2xx means the endpoint, deployment, and key are all accepted by Azure.
                // A non-2xx (401/403/404/429/5xx, etc.) means credentials are wrong, the deployment
                // name is wrong, or the endpoint is unreachable.
                _isCredentialValid = resp.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                _isCredentialValid = false;
            }
            finally
            {
                _isValidated = true;
                _validationLock.Release();
            }
        }

        /// <summary>
        /// Reports whether the placeholder credential constants have been replaced with real values.
        /// </summary>
        private static bool IsConfigured()
        {
            return !string.IsNullOrWhiteSpace(endpoint)
                && !endpoint.StartsWith("Enter your", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(deploymentName)
                && !deploymentName.StartsWith("Enter your", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(key)
                && !key.StartsWith("Enter your", StringComparison.OrdinalIgnoreCase)
                && Uri.TryCreate(endpoint, UriKind.Absolute, out _);
        }

        /// <summary>
        /// Method to send a chat request to the Azure OpenAI service.
        /// </summary>
        /// <param name="messages">The conversation messages, as (role, content) tuples.</param>
        /// <param name="maxTokens">Maximum tokens to generate in the completion.</param>
        /// <returns>The assistant message content, or an empty string when the call fails.</returns>
        protected async Task<string> SendChatAsync((string role, string content)[] messages, int maxTokens = 2500)
        {
            var payload = new
            {
                messages = Array.ConvertAll(messages, m => new { role = m.role, content = m.content }),
                max_completion_tokens = maxTokens,
            };

            var json = JsonSerializer.Serialize(payload);
            var url = $"openai/deployments/{deploymentName}/chat/completions?api-version={apiVersion}";

            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var resp = await _httpClient.Value.PostAsync(url, content).ConfigureAwait(false);

            var respBody = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!resp.IsSuccessStatusCode)
                return string.Empty;

            try
            {
                using var doc = JsonDocument.Parse(respBody);
                if (doc.RootElement.TryGetProperty("choices", out var choices)
                    && choices.ValueKind == JsonValueKind.Array
                    && choices.GetArrayLength() > 0)
                {
                    var first = choices[0];
                    if (first.TryGetProperty("message", out var message)
                        && message.TryGetProperty("content", out var contentEl))
                    {
                        return contentEl.GetString() ?? string.Empty;
                    }
                }
            }
            catch (JsonException)
            {
                // Fall through to the raw body below.
            }

            return respBody ?? string.Empty;
        }
    }

    /// <summary>
    /// Class that implements the IAzureAIService interface and extends the AzureBaseService to provide functionality for sending user prompts to the Azure OpenAI service and receiving responses.
    /// </summary>
    public class AzureAIService : AzureBaseService, IAzureAIService
    {
        /// <summary>
        /// Method to send a user prompt to the Azure OpenAI service and receive a response.
        /// </summary>
        /// <param name="userAIPrompt">The prompt to send to the model.</param>
        /// <returns>The model's response text, or an empty string when the call fails.</returns>
        public async Task<string> GetResultsFromAI(string userAIPrompt)
        {
            try
            {
                var messages = new[]
                {
                    ("system", "You are a concise task-management assistant. Follow the output format requested by the user exactly and return no preamble."),
                    ("user", userAIPrompt)
                };

                var result = await SendChatAsync(messages, maxTokens: 2500);
                return result ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}