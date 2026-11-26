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

        protected const string SystemPrompt =
            "You convert natural language chart grouping instructions into a single JSON object for a doughnut chart that merges small slices into 'Others'. " +
            "Return only the JSON object {\"Kind\": <kind>, \"Amount\": <number>}. No markdown, no explanation.";

        protected static string BuildPrompt(string userAIPrompt)
        {
            return "Pick Kind and Amount for the request.\n" +
                   "Kind values:\n" +
                   "- \"Value\": merge slices whose absolute value is at or below Amount (e.g. 'group by value under 25' -> Amount 25).\n" +
                   "- \"Percentage\": merge slices whose share of the total, in percent, is at or below Amount (e.g. 'percentage under 5' -> Amount 5).\n" +
                   "- \"Angle\": merge slices whose angle, in degrees, is at or below Amount (e.g. 'smaller than 10 degrees' -> Amount 10).\n" +
                   "- \"Top\": keep only the Amount largest slices and merge the rest (e.g. 'show only the top 4 products' -> Amount 4).\n" +
                   "- \"Smallest\": merge the Amount smallest slices (e.g. 'merge the smallest 3 slices' -> Amount 3).\n" +
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
                    new { role = "system", content = SystemPrompt },
                    new { role = "user", content = BuildPrompt(userAIPrompt) }
                },
                temperature = 0.1,
                max_tokens = 100,
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
