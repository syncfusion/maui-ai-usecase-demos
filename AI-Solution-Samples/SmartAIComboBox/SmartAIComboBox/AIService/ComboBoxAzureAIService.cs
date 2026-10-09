using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace SmartAIComboBox.SmartAIComboBox
{
    /// <summary>
    /// Azure OpenAI chat-completion service for the SmartAIComboBox fruit-selection sample.
    /// Mirrors the SmartAIDatePicker.AzureOpenAIService pattern exactly:
    /// no constructor logic, no IsCredentialValid probe, no SK abstractions.
    /// The caller invokes GetCompletion() directly and parses the result.
    /// </summary>
    public sealed class ComboBoxAzureAIService
    {
        private const string BaseEndpoint =
            "https://your-openai-resource.openai.azure.com/";

        private const string DeploymentName = "your-DeploymentName";

        private const string ApiKey =
            "your-azure-openai-api-key";

        private static readonly HttpClient Http = new()
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        public string? LastError { get; private set; }

        public string LastRawResponse { get; private set; } = string.Empty;

        /// <summary>
        /// Single entry point — sends the AI the prompt the ViewModel built
        /// (menu + intent rules + user query) and returns the raw fruit list text.
        /// </summary>
        public async Task<string> GetCompletion(
            string prompt,
            CancellationToken cancellationToken = default)
        {
            LastError = null;
            LastRawResponse = string.Empty;

            try
            {
                var requestBody = new
                {
                    model = DeploymentName,
                    messages = new object[]
                    {
                        new
                        {
                            role = "system",
                            content =
                                "You are an AI fruit-selection assistant for a MultiSelect fruit menu. " +
                                "Choose fruits from the provided menu that match the user's natural-language request. " +
                                "Return ONLY the exact fruit Names from the menu, one per line. " +
                                "Do NOT include tags, numbers, dashes, bullets, explanations, or any preamble. " +
                                "If no fruit matches, return exactly the word: Empty"
                        },
                        new
                        {
                            role = "user",
                            content = prompt
                        }
                    },
                    max_completion_tokens = 800,
                    reasoning_effort = "minimal"
                };

                var url = $"{BaseEndpoint}/chat/completions";

                using var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    url);

                request.Headers.Add("api-key", ApiKey);

                request.Content = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json");

                var response = await Http.SendAsync(
                    request,
                    cancellationToken);

                var raw = await response.Content.ReadAsStringAsync(
                    cancellationToken);

                LastRawResponse = raw;

                Debug.WriteLine($"[AzureAI] Status: {(int)response.StatusCode}");
                Debug.WriteLine($"[AzureAI] Raw Response: {raw}");

                if (!response.IsSuccessStatusCode)
                {
                    LastError = $"HTTP {(int)response.StatusCode}\n\n{raw}";
                    return LastError;
                }

                using var document = JsonDocument.Parse(raw);

                if (!document.RootElement.TryGetProperty("choices", out var choices))
                {
                    LastError = $"No 'choices' property found.\n\nRaw Response:\n{raw}";
                    return LastError;
                }

                var content = choices[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                if (string.IsNullOrWhiteSpace(content))
                {
                    LastError = $"Azure returned empty content.\n\nRaw Response:\n{raw}";
                    return LastError;
                }

                return content.Trim();
            }
            catch (Exception ex)
            {
                LastError = $"Exception:\n{ex}";
                Debug.WriteLine($"[AzureAI] Exception: {ex}");
                return LastError;
            }
        }
    }
}