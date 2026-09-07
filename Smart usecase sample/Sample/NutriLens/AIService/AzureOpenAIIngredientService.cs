using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using NutriLens.Models;

namespace NutriLens.Services;

public sealed class AzureOpenAIIngredientService : IAzureOpenAIIngredientService
{
    private const string BaseEndpoint = AzureOpenAIConfig.BaseEndpoint;
    private const string DeploymentName = AzureOpenAIConfig.DeploymentName;
    private const string ApiKey = AzureOpenAIConfig.ApiKey;

    private static readonly HttpClient Http = new()
    {
        Timeout = TimeSpan.FromSeconds(90)
    };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public Task<IngredientAnalysisResult> AnalyzeAsync(
        string extractedText,
        CancellationToken cancellationToken = default)
        => AnalyzeAsync(extractedText, preferences: null, cancellationToken);

    public async Task<IngredientAnalysisResult> AnalyzeAsync(
        string extractedText,
        UserDietaryPreference? preferences,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(extractedText))
            throw new InvalidOperationException("No label content was extracted from the image.");

        var prompt = BuildPrompt(extractedText, preferences);

        var requestBody = new
        {
            model = DeploymentName,
            messages = new object[]
            {
                new
                {
                    role = "system",
                    content = """
                    You are NutriLens, an ingredient and nutrition analysis assistant.
                    Evaluate packaged food products.
                    Return only valid JSON.
                    Do not invent facts.
                    Use empty arrays for unavailable values.
                    Personalize recommendations strictly according to the user profile section provided in the user message. When the profile is empty, fall back to a neutral, balanced evaluation.
                    """
                },
                new { role = "user", content = prompt }
            },
            max_completion_tokens = 1800,
            reasoning_effort = "minimal"
        };

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{BaseEndpoint.TrimEnd('/')}/chat/completions");

        request.Headers.Add("api-key", ApiKey);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        request.Content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        using var response = await Http.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        var rawResponse = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"Azure OpenAI request failed. HTTP {(int)response.StatusCode}: {rawResponse}");

        return ParseResponse(rawResponse, extractedText);
    }

    private static string BuildPrompt(
        string extractedText,
        UserDietaryPreference? preferences)
    {
        var profileSection = BuildProfileDirective(preferences);

        return $$"""
{{profileSection}}

Food product information extracted from the package:
{{extractedText}}

Analyze this product and return valid JSON with exactly this structure:

{
  "profile": "string",
  "productName": "string",
  "extractedText": "string",
  "score": 0,
  "scoreExplanation": "string",
  "category": "Excellent|Good|Moderate|Poor|Unknown",
  "confidence": "High|Moderate|Low|Unknown",
  "recommendation": "string",
  "summary": "string",
  "positiveAttributes": ["string"],
  "negativeAttributes": ["string"],
  "metabolicConflict": "string",
  "alternativeRecommendations": ["string"],
  "fullIngredients": "string",
  "allergens": ["string"],
  "artificialColors": ["string"],
  "additives": [
    { "name": "string", "purpose": "string", "safetyNote": "string" }
  ],
  "preservatives": [
    { "name": "string", "purpose": "string", "safetyNote": "string" }
  ],
  "nutrition": {
    "servingSize": "string",
    "servingsPerContainer": "string",
    "calories": null,
    "totalFatGrams": null,
    "saturatedFatGrams": null,
    "sodiumMg": null,
    "carbohydratesGrams": null,
    "sugarsGrams": null,
    "addedSugarsGrams": null,
    "proteinGrams": null,
    "fiberGrams": null
  },
  "ingredientBreakdown": [
    { "name": "string", "purpose": "string", "riskLevel": "High|Moderate|Low|Unknown", "explanation": "string" }
  ]
}

Requirements:
- Score must be 0-100.
- If the user selected Reduce Sugar, flag sugars, syrups, sweeteners.
- If High Protein, highlight protein benefits.
- If Low Carb, flag high carb ingredients.
- If Heart Health, focus on saturated fat, trans fat, sodium.
- If Nut-free, detect nuts.
- If Gluten-free, detect wheat, barley, rye, malt.
- If Vegan, detect animal-derived ingredients.
- If Dairy-free, detect milk, cheese, butter, whey, casein.
- If Diabetes, focus on glycemic impact.
- If Hypertension, focus on sodium and blood-pressure impact.
- If no preferences are selected, return a standard neutral analysis.
- Return only the JSON object.
""";
    }

    private static string BuildProfileDirective(UserDietaryPreference? preferences)
    {
        if (preferences is null || preferences.IsEmpty)
            return "User profile: (none set) — apply standard neutral ingredient analysis.";

        var parts = new List<string>();

        if (preferences.DietaryGoals.Count > 0)
            parts.Add("Dietary Goals: " + string.Join(", ", preferences.DietaryGoals));
        if (preferences.AllergiesAndPreferences.Count > 0)
            parts.Add("Allergies & Preferences: " + string.Join(", ", preferences.AllergiesAndPreferences));
        if (preferences.HealthConsiderations.Count > 0)
            parts.Add("Health Considerations: " + string.Join(", ", preferences.HealthConsiderations));

        return "User profile (personalize the analysis to ALL of the following):\n" + string.Join('\n', parts);
    }

    private static IngredientAnalysisResult ParseResponse(string rawResponse, string extractedText)
    {
        using var document = JsonDocument.Parse(rawResponse);

        if (!document.RootElement.TryGetProperty("choices", out var choices) ||
            choices.ValueKind != JsonValueKind.Array ||
            choices.GetArrayLength() == 0)
        {
            throw new InvalidOperationException("Azure response did not contain a valid choices array.");
        }

        var message = choices[0].GetProperty("message");
        var contentElement = message.GetProperty("content");

        string resultJson;

        if (contentElement.ValueKind == JsonValueKind.Array)
        {
            resultJson = string.Concat(
                contentElement.EnumerateArray()
                    .Where(x => x.TryGetProperty("text", out _))
                    .Select(x => x.GetProperty("text").GetString() ?? string.Empty));
        }
        else
        {
            resultJson = contentElement.GetString() ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(resultJson))
            throw new InvalidOperationException("Azure OpenAI returned empty analysis content.");

        var result = JsonSerializer.Deserialize<IngredientAnalysisResult>(resultJson, JsonOptions);
        if (result is null)
            throw new InvalidOperationException("Azure returned invalid NutriLens JSON.");

        result.ExtractedText = extractedText;
        result.Profile = string.IsNullOrWhiteSpace(result.Profile) ? "Standard" : result.Profile;
        result.Score = Math.Clamp(result.Score, 0, 100);
        result.Category = string.IsNullOrWhiteSpace(result.Category) ? GetCategory(result.Score) : result.Category;

        return result;
    }

    private static string GetCategory(int score)
        => score switch
        {
            >= 80 => "Excellent",
            >= 60 => "Good",
            >= 40 => "Moderate",
            >= 20 => "Poor",
            _ => "Unknown"
        };
}