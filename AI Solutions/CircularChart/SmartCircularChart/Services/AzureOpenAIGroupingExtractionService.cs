using System.Globalization;
using System.Text;
using System.Text.Json;

namespace SmartCircularChart;

public sealed class AzureOpenAIGroupingExtractionService : AzureBaseService, IGroupingExtractionService
{
    private static readonly HttpClient HttpClient = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
    };

    private const string SystemPrompt = """
        You configure how a doughnut chart merges small slices into one "Others" slice.
        The chart merges every slice whose measure is less than or equal to GroupTo.
        Return JSON only.
        Do not include markdown, code fences, or explanations outside the JSON.
        The response must match this shape:
        {
          "groupMode": "Value|Percentage|Angle",
          "groupTo": number or null,
          "explanation": "string"
        }
        groupMode "Value" compares the slice value, "Percentage" compares its share of the total (0-100), and "Angle" compares its angle in degrees (0-360).
        Choose the mode that matches the user's wording.
        For a ranking request, use "Value". For "keep the top N" set groupTo to the (N+1)th largest value. For "merge the smallest N" set groupTo to the Nth smallest value.
        If the user wants no grouping or a reset, set groupTo to null.
        """;

    public async Task<GroupingResponse> ExtractAsync(string prompt, IEnumerable<CircularChartData> chartData, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prompt);

        var request = new HttpRequestMessage(HttpMethod.Post, GetChatCompletionsUri());
        request.Headers.TryAddWithoutValidation("api-key", ApiKey);

        var payload = new
        {
            model = DeploymentName,
            messages = new object[]
            {
                new { role = "system", content = SystemPrompt },
                new { role = "user", content = BuildUserMessage(prompt, chartData) }
            },
            temperature = 0f,
            max_completion_tokens = 2048,
            response_format = new { type = "json_object" }
        };

        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        HttpResponseMessage response;
        try
        {
            response = await HttpClient.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Azure OpenAI request failed: {ex.Message}", ex);
        }

        using (response)
        {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Azure OpenAI request failed: HTTP {(int)response.StatusCode} ({response.ReasonPhrase}): {responseBody}");
            }

            using var responseDocument = JsonDocument.Parse(responseBody);
            var content = responseDocument.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? string.Empty;

            var json = ExtractJsonPayload(content);

            try
            {
                return JsonSerializer.Deserialize<GroupingResponse>(json, JsonOptions)
                    ?? throw new InvalidOperationException("Azure OpenAI returned an empty grouping response.");
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("Azure OpenAI returned an invalid grouping response.", ex);
            }
        }
    }

    private static string BuildUserMessage(string prompt, IEnumerable<CircularChartData> chartData)
    {
        var items = chartData.ToList();
        var total = items.Sum(item => item.Value);
        var table = new StringBuilder();
        table.AppendLine(string.Create(CultureInfo.InvariantCulture, $"Chart data, largest first (total value {total}):"));
        foreach (var item in items.OrderByDescending(item => item.Value))
        {
            var percentage = total > 0 ? Math.Round(item.Value / total * 100, 2) : 0;
            var angle = total > 0 ? Math.Round(item.Value / total * 360, 2) : 0;
            table.AppendLine(string.Create(CultureInfo.InvariantCulture, $"- {item.Category}: value {item.Value}, percentage {percentage}, angle {angle}"));
        }

        table.AppendLine();
        table.Append("User request: ").Append(prompt);
        return table.ToString();
    }

    private static string ExtractJsonPayload(string content)
    {
        var cleaned = CleanJson(content);
        var startIndex = cleaned.IndexOf('{');
        var endIndex = cleaned.LastIndexOf('}');

        if (startIndex >= 0 && endIndex > startIndex)
        {
            return cleaned[startIndex..(endIndex + 1)];
        }

        return cleaned;
    }

    private static string CleanJson(string content)
    {
        content = content.Trim();

        if (content.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
        {
            content = content[7..];
        }

        if (content.StartsWith("```", StringComparison.Ordinal))
        {
            content = content[3..];
        }

        if (content.EndsWith("```", StringComparison.Ordinal))
        {
            content = content[..^3];
        }

        return content.Trim();
    }
}
