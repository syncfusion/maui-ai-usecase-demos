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

    // ----- existing overload, unchanged behavior -----
    public Task<IngredientAnalysisResult> AnalyzeAsync(
        string extractedText,
        CancellationToken cancellationToken = default)
        => AnalyzeAsync(extractedText, preferences: null, cancellationToken);

    // ----- new personalized overload -----
    public async Task<IngredientAnalysisResult> AnalyzeAsync(
        string extractedText,
        UserDietaryPreference? preferences,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(extractedText))
            throw new InvalidOperationException(
                "No label content was extracted from the image.");

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
                    Personalize recommendations strictly according to the user profile
                    section provided in the user message. When the profile is empty,
                    fall back to a neutral, balanced evaluation.
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
        request.Headers.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        request.Content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        using var response = await Http.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        var rawResponse = await response.Content.ReadAsStringAsync(
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Azure OpenAI request failed. " +
                $"HTTP {(int)response.StatusCode}: {rawResponse}");
        }

        return ParseResponse(rawResponse, extractedText);
    }

    // ----------------------------------------------------------------
    // Prompt builder — composes the personalization directive from the
    // selected preferences. Empty profile → neutral default behavior.
    // ----------------------------------------------------------------
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
  "profile": "string - the effective profile used (e.g. 'Diabetes, Hypertension' or 'Standard')",
  "productName": "string",
  "extractedText": "string",
  "score": 0,
  "scoreExplanation": "string - one or two sentences explaining exactly why the product received this score, referencing the user profile when one is set",
  "category": "Excellent|Good|Moderate|Poor|Unknown",
  "confidence": "High|Moderate|Low|Unknown",
  "recommendation": "string - personalized to the profile when set",
  "summary": "string - personalized AI summary",
  "positiveAttributes": ["string"],
  "negativeAttributes": ["string"],
  "metabolicConflict": "string - tailored to selected health considerations",
  "alternativeRecommendations": ["string"],
  "fullIngredients": "string - the full ingredient list exactly as printed on the label, or empty string if not shown",
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
- Score must be 0-100, scoreExplanation must justify the exact score.
- When the user profile is set, EVERY recommendation / conflict / warning
  must be consistent with that profile and never contradict itself.
- Reduce Sugar  → flag added sugars and sugar-heavy ingredients.
- High Protein  → comment on protein content.
- Low Carb      → comment on total carbohydrates.
- Heart Health  → comment on saturated fat and sodium.
- Nut-Free      → detect nut allergens.
- Gluten-Free   → detect gluten-containing grains (wheat, barley, rye).
- Vegan         → flag animal-derived ingredients.
- Dairy-Free    → detect milk, lactose, casein, whey, butter, cream.
- Diabetes      → glycemic impact and blood-sugar concerns.
- Hypertension  → sodium and salt additives, blood-pressure impact.
- Use exact numeric nutrition values from the label when present; null otherwise.
- Use empty arrays or null when information is missing — never invent values.
- Return only the JSON object.
""";
    }

    private static string BuildProfileDirective(
        UserDietaryPreference? preferences)
    {
        if (preferences is null || preferences.IsEmpty)
            return "User profile: (none set) — apply standard neutral ingredient analysis.";

        var parts = new List<string>();

        if (preferences.DietaryGoals.Count > 0)
            parts.Add("Dietary Goals: " + string.Join(", ", preferences.DietaryGoals));
        if (preferences.AllergiesAndPreferences.Count > 0)
            parts.Add("Allergies & Preferences: " +
                string.Join(", ", preferences.AllergiesAndPreferences));
        if (preferences.HealthConsiderations.Count > 0)
            parts.Add("Health Considerations: " +
                string.Join(", ", preferences.HealthConsiderations));

        return parts.Count == 0
            ? "User profile: (none set) — apply standard neutral ingredient analysis."
            : "User profile (personalize the analysis to ALL of the following):\n" +
               string.Join('\n', parts);
    }

    private static IngredientAnalysisResult ParseResponse(
        string rawResponse,
        string extractedText)
    {
        using var document = JsonDocument.Parse(rawResponse);

        if (!document.RootElement.TryGetProperty("choices", out var choices) ||
            choices.ValueKind != JsonValueKind.Array ||
            choices.GetArrayLength() == 0)
        {
            throw new InvalidOperationException(
                "Azure response did not contain a valid choices array.");
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
            throw new InvalidOperationException(
                "Azure OpenAI returned empty analysis content.");

        var result = JsonSerializer.Deserialize<IngredientAnalysisResult>(
            resultJson, JsonOptions);

        if (result is null)
            throw new InvalidOperationException("Azure returned invalid NutriLens JSON.");

        result.ExtractedText = extractedText;
        result.Profile = string.IsNullOrWhiteSpace(result.Profile)
            ? "Standard"
            : result.Profile;
        result.Score = Math.Clamp(result.Score, 0, 100);
        result.Category = string.IsNullOrWhiteSpace(result.Category)
            ? GetCategory(result.Score)
            : result.Category;

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