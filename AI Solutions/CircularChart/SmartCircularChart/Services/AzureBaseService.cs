using System.Globalization;
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
            "You configure the grouping of a doughnut chart. Small slices are merged into a single 'Others' slice " +
            "by the chart's GroupMode and GroupTo properties. Return only a JSON object {\"GroupMode\": <mode>, \"GroupTo\": <number or null>}. " +
            "No markdown, no explanation.";

        protected static string BuildPrompt(string userAIPrompt, IEnumerable<CircularChartData> chartData)
        {
            var items = chartData.ToList();
            var total = items.Sum(item => item.Value);
            var table = new StringBuilder();
            foreach (var item in items.OrderByDescending(item => item.Value))
            {
                var percentage = total > 0 ? Math.Round(item.Value / total * 100, 2) : 0;
                var angle = total > 0 ? Math.Round(item.Value / total * 360, 2) : 0;
                table.AppendLine(string.Create(CultureInfo.InvariantCulture, $"- {item.Category}: value {item.Value}, percentage {percentage}, angle {angle}"));
            }

            return "Chart data (sorted from largest to smallest, total value " + total.ToString(CultureInfo.InvariantCulture) + "):\n" +
                   table +
                   "\nThe chart merges every slice whose measure is LESS THAN OR EQUAL TO GroupTo into 'Others'. " +
                   "Choose GroupMode and GroupTo so that exactly the slices the user wants merged fall at or below GroupTo and all other slices stay above it.\n" +
                   "GroupMode values:\n" +
                   "- \"Value\": compare the slice value with GroupTo.\n" +
                   "- \"Percentage\": compare the slice percentage (0-100) with GroupTo.\n" +
                   "- \"Angle\": compare the slice angle in degrees (0-360) with GroupTo.\n" +
                   "Use the measure that matches the user's wording. For ranking requests such as 'keep the top N' or 'merge the smallest N', " +
                   "use \"Value\" and set GroupTo to the value of the largest slice that must be merged (for 'top N' that is the (N+1)th largest value; for 'smallest N' it is the Nth smallest value). " +
                   "If the user wants no grouping or wants to reset the chart, return GroupTo null.\n" +
                   $"The user prompt is: {userAIPrompt}";
        }
    }

    public class AzureAIService : AzureBaseService, IAzureAIService
    {
        private static readonly HttpClient httpClient = new();

        public async Task<string> GetResultsFromAI(string userAIPrompt, IEnumerable<CircularChartData> chartData)
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
                    new { role = "user", content = BuildPrompt(userAIPrompt, chartData) }
                },
                temperature = 0.1,
                max_tokens = 200,
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
