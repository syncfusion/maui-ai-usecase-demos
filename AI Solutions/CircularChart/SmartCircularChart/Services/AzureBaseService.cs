using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SmartCircularChart
{
    public abstract class AzureBaseService
    {

		protected const string endpoint = "endpoint";
		protected const string deploymentName = "deploymentName";
		protected const string key = "key";

        protected static string BuildPrompt(string userAIPrompt)
        {
            return $"You are a chart grouping assistant. Convert the user request into JSON only. " +
                   $"Return a JSON array of objects with Category, Value, Group, GroupMode, and GroupTo fields. " +
                   $"Use these rules when possible: 'Group by value under 15', 'Group products by category', 'Merge small values into Others'. " +
                   $"The user prompt is: {userAIPrompt}";
        }
    }

    public class AzureAIService : AzureBaseService, IAzureAIService
    {
        private static readonly HttpClient httpClient = new();

        public async Task<string> GetResultsFromAI(string userAIPrompt)
        {
            if (string.IsNullOrWhiteSpace(userAIPrompt))
            {
                return string.Empty;
            }

            var requestBody = new
            {
                messages = new[]
                {
                    new { role = "system", content = "You convert natural language chart grouping instructions into strictly valid JSON for a Syncfusion circular chart sample. Return only the JSON array with Category, Value, Group, GroupMode, and GroupTo fields. No markdown, no explanation, no prose." },
                    new { role = "user", content = BuildPrompt(userAIPrompt) }
                },
                temperature = 0.1,
                max_tokens = 500,
                response_format = new { type = "json_object" }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, $"{endpoint}openai/deployments/{deploymentName}/chat/completions?api-version=2024-06-01")
            {
                Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
            };
            request.Headers.Add("api-key", key);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                using var response = await httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    return string.Empty;
                }

                var json = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(json);
                var content = document.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content");

                return content.GetString() ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
