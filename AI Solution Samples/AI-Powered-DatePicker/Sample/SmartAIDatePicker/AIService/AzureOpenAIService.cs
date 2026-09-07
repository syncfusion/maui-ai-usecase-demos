using System.Text;
using System.Text.Json;

namespace SmartAIDatePicker.AIService;

public sealed class AzureOpenAIService : IAzureOpenAIService
{
    private const string BaseEndpoint = "ENDPOINT";

    private const string DeploymentName =
        "gpt-5-mini";

    private const string ApiKey = "API_KEY";

    private static readonly HttpClient Http = new()
    {
        Timeout = TimeSpan.FromSeconds(30)
    };

    public string? LastError { get; private set; }

    public string LastRawResponse { get; private set; } = string.Empty;

    public async Task<string> GetCompletion(
        string prompt,
        CancellationToken cancellationToken = default)
    {
        LastError = null;
        LastRawResponse = string.Empty;

        var requestBody = new
        {
            model = DeploymentName,
            messages = new object[]
            {
                new
                {
                    role = "system",
                    content =
                        "Interpret the user's natural-language request, including informal wording, spelling errors, " +
                        "abbreviations, and date formats such as 10/02/2010. Resolve date questions from every " +
                        "category, including historical events, wars, births, deaths, inventions, holidays, " +
                        "anniversaries, weekdays, leap days, seasons, month boundaries, and relative dates. " +
                        "Use the reference date supplied by the user as today. " +
                        "Return exactly one JSON object with these properties: date (yyyy-MM-dd or null), " +
                        "minimumDate (yyyy-MM-dd or null), maximumDate (yyyy-MM-dd or null), " +
                        "blackoutDates (array of yyyy-MM-dd strings or null), dayInterval (integer or null), " +
                        "monthInterval (integer or null), yearInterval (integer or null), " +
                        "enableLooping (boolean or null), format (string or null), and error (string or null). " +
                        "Only set picker properties explicitly requested or clearly implied by the user. " +
                        "Use minimumDate and maximumDate for all date filters. For 'do not show dates after 2023', " +
                        "set maximumDate to 2023-12-31. For 'only dates after 10/02/2010', interpret the date using " +
                        "the user's locale context and set minimumDate to the following day. For 'before March 2010', " +
                        "set maximumDate to 2010-03-31. For 'after March 2010', set minimumDate to 2010-04-01. " +
                        "For historical events, return the commonly accepted start or occurrence date; for a war, " +
                        "return the date it began unless the user explicitly asks for its end. " +
                        "For next or upcoming events, return the first occurrence strictly after today. " +
                        "For this use the current period; for last use the previous occurrence; and for in N " +
                        "days/weeks/months add exactly N units to today. For recurring holidays without a year, " +
                        "use the next occurrence. If Independence Day has no country, use US Independence Day, July 4. " +
                        "Resolve aliases such as WWI/First World War and leap day. Do not invent dates for genuine " +
                        "ambiguity. For an invalid or non-date request, set date to null and error to INVALID_REQUEST."
                },
                new
                {
                    role = "user",
                    content = prompt
                }
            },
            max_completion_tokens = 800,
            reasoning_effort = "high",
            response_format = new
            {
                type = "json_object"
            }
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

        System.Diagnostics.Debug.WriteLine(
            $"[AzureAI] Status: {(int)response.StatusCode}");

        System.Diagnostics.Debug.WriteLine(
            $"[AzureAI] Raw Response: {raw}");

        if (!response.IsSuccessStatusCode)
        {
            LastError =
                $"HTTP {(int)response.StatusCode}\n\n{raw}";

            return LastError;
        }

        using var document = JsonDocument.Parse(raw);

        if (!document.RootElement.TryGetProperty(
                "choices",
                out var choices))
        {
            LastError =
                $"No 'choices' property found.\n\nRaw Response:\n{raw}";

            return LastError;
        }

        if (choices.GetArrayLength() == 0)
        {
            LastError = "Azure returned no choices.";
            return LastError;
        }

        var content = choices[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        if (string.IsNullOrWhiteSpace(content))
        {
            LastError =
                $"Azure returned empty content.\n\nRaw Response:\n{raw}";

            return LastError;
        }

        return content.Trim();
    }
}