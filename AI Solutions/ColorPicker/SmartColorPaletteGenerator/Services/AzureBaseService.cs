#nullable enable
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Maui.Controls;

namespace SmartColorPaletteGenerator
{
    /// <summary>
    /// AzureBaseService is an abstract class that provides the foundational functionality for interacting with the Azure OpenAI service.
    /// </summary>
    public abstract class AzureBaseService
    {

		protected static readonly string Endpoint = "AZURE_OPENAI_ENDPOINT";
		protected static readonly string DeploymentName = "AZURE_OPENAI_DEPLOYMENT";
		protected static readonly string ApiKey = "AZURE_OPENAI_KEY";
		private const string apiVersion = "2024-06-01";

        private static readonly HttpClient _httpClient = CreateHttpClient();
        private static readonly SemaphoreSlim _validationLock = new(1, 1);
        private static bool _isCredentialValid = false;
        private static bool isAlreadyValidated = false;
        private static bool isValidationInProgress = false;
        internal static event CredentialValidated OnCredentialChanged = delegate { };

        private static HttpClient CreateHttpClient()
        {
            var client = new HttpClient();
            try
            {
                client.BaseAddress = new Uri(endpoint, UriKind.Absolute);
                client.DefaultRequestHeaders.Add("api-key", key);
            }
            catch
            {
                // keep the sample resilient; validation will mark credentials invalid
            }

            return client;
        }

        /// <summary>
        /// It initializes the HttpClient with the Azure OpenAI endpoint and API key if it hasn't been initialized already. 
        /// </summary>
        public AzureBaseService()
        {
            _ = ValidateCredentialAsync();
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Azure OpenAI credentials (endpoint and API key) are valid. When set, it triggers the OnCredentialChanged event to notify subscribers of the change in credential validity status.
        /// </summary>
        internal static bool IsCredentialValid
        {
            get => _isCredentialValid;
            set
            {
                if (_isCredentialValid != value)
                {
                    _isCredentialValid = value;
                    OnCredentialChanged?.Invoke(value);
                }
            }
        }

        /// <summary>
        /// Method to validate the Azure OpenAI credentials by sending a lightweight test request.
        /// Validity is determined by the HTTP status code (2xx = valid) rather than the response body,
        /// because the model can legitimately return an empty body even when credentials are correct.
        /// </summary>
        /// <returns></returns>
        private async Task ValidateCredentialAsync()
        {
            if (isAlreadyValidated || isValidationInProgress)
                return;

            if (!await _validationLock.WaitAsync(0).ConfigureAwait(false))
                return;

            isValidationInProgress = true;

            try
            {
                if (_httpClient == null)
                {
                    IsCredentialValid = false;
                    await ShowAlertAsync();
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
                using var resp = await _httpClient.PostAsync(url, content).ConfigureAwait(false);

                // 2xx means the endpoint, deployment and key are all accepted by Azure.
                // A non-2xx (401/403/404/429/5xx, etc.) means credentials are wrong,
                // the deployment name is wrong, or the endpoint is unreachable.
                IsCredentialValid = resp.IsSuccessStatusCode;

                if (!IsCredentialValid)
                {
                    await ShowAlertAsync();
                }
            }
            catch (Exception)
            {
                IsCredentialValid = false;
                await ShowAlertAsync();
            }
            finally
            {
                isAlreadyValidated = true;
                isValidationInProgress = false;
                _validationLock.Release();
            }
        }

        /// <summary>
        /// Method to display an alert to the user if the Azure OpenAI credentials are invalid.
        /// The alert is only shown when IsCredentialValid is false (i.e. the status code was not 2xx
        /// or the request threw an exception). It is never shown for successful validations.
        /// </summary>
        /// <returns></returns>
        private Task ShowAlertAsync()
        {
            System.Diagnostics.Debug.WriteLine("The Azure API key or endpoint is missing or incorrect. Please verify your credentials.");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Method to send a chat request to the Azure OpenAI service.
        /// </summary>
        /// <param name="messages"></param>
        /// <param name="maxTokens"></param>
        /// <param name="temperature"></param>
        /// <returns></returns>
        internal async Task<string> SendChatAsync((string role, string content)[] messages, int maxTokens = 2500, float temperature = 1f)
        {
            if (_httpClient == null)
                return string.Empty;

            var payload = new
            {
                messages = Array.ConvertAll(messages, m => new { role = m.role, content = m.content }),
                max_completion_tokens = maxTokens,
            };

            var json = JsonSerializer.Serialize(payload);
            var url = $"openai/deployments/{deploymentName}/chat/completions?api-version={apiVersion}";

            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var resp = await _httpClient.PostAsync(url, content);
            var respBody = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                return string.Empty;

            try
            {
                using var doc = JsonDocument.Parse(respBody ?? string.Empty);
                if (doc.RootElement.TryGetProperty("choices", out var choices) && choices.ValueKind == JsonValueKind.Array && choices.GetArrayLength() > 0)
                {
                    var first = choices[0];
                    if (first.TryGetProperty("message", out var message) && message.TryGetProperty("content", out var contentEl))
                        return contentEl.GetString() ?? string.Empty;

                    if (first.TryGetProperty("text", out var textEl))
                        return textEl.GetString() ?? string.Empty;
                }
            }
            catch
            {
            }

            return respBody ?? string.Empty;
        }

        /// <summary>
        /// Method signature for the event that is triggered when the validity of the Azure OpenAI credentials changes.
        /// </summary>
        /// <param name="isCredentialValid"></param>
        internal delegate void CredentialValidated(bool isCredentialValid);
    }

    /// <summary>
    /// Class that implements the IAzureAIService interface and extends the AzureBaseService to provide functionality for sending user prompts to the Azure OpenAI service and receiving responses. 
    /// </summary>
    public class AzureAIService : AzureBaseService, IAzureAIService
    {
        /// <summary>
        /// AzureAIService constructor that initializes the base AzureBaseService. 
        /// </summary>
        public AzureAIService() { }

        /// <summary>
        /// Method to send a user prompt to the Azure OpenAI service and receive a response. 
        /// </summary>
        /// <param name="userAIPrompt"></param>
        /// <returns></returns>
        public async Task<string> GetResultsFromAI(string userAIPrompt)
        {
            try
            {
                var messages = new[]
                {
                    ("system", "You are an assistant that responds with concise, valid JSON when asked."),
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
