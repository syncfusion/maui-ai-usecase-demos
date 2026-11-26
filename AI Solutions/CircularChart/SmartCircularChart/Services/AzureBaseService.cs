using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SmartCircularChart
{
    public abstract class AzureBaseService
    {

		protected static readonly string Endpoint = "AZURE_OPENAI_ENDPOINT";
		protected static readonly string DeploymentName = "AZURE_OPENAI_DEPLOYMENT";
		protected static readonly string ApiKey = "AZURE_OPENAI_KEY";
		protected static readonly string ApiVersion = "2024-08-01-preview";

        protected static Uri GetChatCompletionsUri()
        {
            if (string.IsNullOrWhiteSpace(Endpoint) || string.IsNullOrWhiteSpace(DeploymentName) || string.IsNullOrWhiteSpace(ApiKey) ||
                Endpoint == "AZURE_OPENAI_ENDPOINT" || DeploymentName == "AZURE_OPENAI_DEPLOYMENT" || ApiKey == "AZURE_OPENAI_KEY")
            {
                throw new InvalidOperationException("Azure OpenAI credentials are not configured. Set Endpoint, DeploymentName and ApiKey in AzureBaseService.");
            }

            if (!Uri.TryCreate(Endpoint, UriKind.Absolute, out var endpointUri))
            {
                throw new InvalidOperationException("Azure OpenAI endpoint is not a valid absolute URL, e.g. https://<resource-name>.openai.azure.com/");
            }

            return new Uri($"{endpointUri.GetLeftPart(UriPartial.Authority)}/openai/deployments/{DeploymentName}/chat/completions?api-version={ApiVersion}");
        }

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
                model = DeploymentName,
                messages = new object[]
                {
                    new { role = "system", content = SystemPrompt },
                    new { role = "user", content = BuildPrompt(userAIPrompt, chartData) }
                },
                max_completion_tokens = 4096,
                response_format = new { type = "json_object" }
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, GetChatCompletionsUri())
            {
                Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
            };
            request.Headers.TryAddWithoutValidation("api-key", ApiKey);

            HttpResponseMessage response;
            try
            {
                response = await httpClient.SendAsync(request);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                throw new InvalidOperationException($"Azure OpenAI request failed: {ex.Message}", ex);
            }

            using (response)
            {
                var body = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException($"Azure OpenAI request failed: HTTP {(int)response.StatusCode} ({response.ReasonPhrase}): {body}");
                }

                using var document = JsonDocument.Parse(body);
                var choice = document.RootElement.GetProperty("choices")[0];
                var content = choice.GetProperty("message").GetProperty("content").GetString();
                if (string.IsNullOrWhiteSpace(content))
                {
                    var reason = choice.TryGetProperty("finish_reason", out var finish) ? finish.GetString() : "unknown";
                    throw new InvalidOperationException($"Azure OpenAI returned an empty reply (finish_reason: {reason}).");
                }

                return content;
            }
        }
    }
}
