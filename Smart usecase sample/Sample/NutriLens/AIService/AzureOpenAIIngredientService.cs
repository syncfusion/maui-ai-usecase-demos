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

    public async Task<IngredientAnalysisResult> AnalyzeAsync(
        string extractedText,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(extractedText))
            throw new InvalidOperationException("No label content was extracted from the image.");

        var prompt = BuildPrompt(extractedText);

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
                    Evaluate packaged food products for diabetes-friendly eating.
                    Return only valid JSON.
                    Do not invent facts.
                    Use empty arrays for unavailable values.
                    """
                },
                new
                {
                    role = "user",
                    content = prompt
                }
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
        {
            throw new HttpRequestException(
                $"Azure OpenAI request failed. HTTP {(int)response.StatusCode}: {rawResponse}");
        }

        return ParseResponse(rawResponse, extractedText);
    }

    private static string BuildPrompt(string extractedText)
    {
        return $$"""
User profile: Diabetes

Food product information extracted from the package:
{{extractedText}}

Analyze this product and return valid JSON with exactly this structure:

{
  "profile": "Diabetes",
  "productName": "string",
  "extractedText": "string",
  "score": 0,
  "scoreExplanation": "string - one or two sentences explaining exactly why the product received this score",
  "category": "Excellent|Good|Moderate|Poor|Unknown",
  "confidence": "High|Moderate|Low|Unknown",
  "recommendation": "string",
  "summary": "string",
  "positiveAttributes": ["string"],
  "negativeAttributes": ["string"],
  "metabolicConflict": "string",
  "alternativeRecommendations": ["string"],
  "fullIngredients": "string - the full ingredient list exactly as printed on the label, or empty string if not shown",
  "allergens": ["string"],
  "artificialColors": ["string"],
  "additives": [
    {
      "name": "string",
      "purpose": "string - what this additive does in the product",
      "safetyNote": "string - brief safety/regulatory status"
    }
  ],
  "preservatives": [
    {
      "name": "string",
      "purpose": "string",
      "safetyNote": "string"
    }
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
    {
      "name": "string",
      "purpose": "string",
      "riskLevel": "High|Moderate|Low|Unknown",
      "explanation": "string"
    }
  ]
}

Requirements:
- Score must be 0-100 and scoreExplanation must justify the exact score given
- Identify sugars, refined carbs, saturated fats, sodium, preservatives, additives, artificial colors, allergens
- Use exact numeric nutrition values from the label when present; use null when not shown on the label
- Mention diabetes suitability and blood sugar impact
- Use empty arrays or null when information is missing - do not invent values
- Include practical recommendations
- Return only the JSON object
""";
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
                contentElement
                    .EnumerateArray()
                    .Where(x => x.TryGetProperty("text", out _))
                    .Select(x => x.GetProperty("text").GetString() ?? string.Empty));
        }
        else
        {
            resultJson = contentElement.GetString() ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(resultJson))
            throw new InvalidOperationException("Azure OpenAI returned empty analysis content.");

        var result = JsonSerializer.Deserialize<IngredientAnalysisResult>(
            resultJson,
            JsonOptions);

        if (result is null)
            throw new InvalidOperationException("Azure returned invalid NutriLens JSON.");

        result.ExtractedText = extractedText;
        result.Profile = string.IsNullOrWhiteSpace(result.Profile) ? "Diabetes" : result.Profile;
        result.Score = Math.Clamp(result.Score, 0, 100);
        result.Category = string.IsNullOrWhiteSpace(result.Category) ? GetCategory(result.Score) : result.Category;

        return result;
    }

    private static string GetCategory(int score)
    {
        if (score >= 80) return "Excellent";
        if (score >= 60) return "Good";
        if (score >= 40) return "Moderate";
        if (score >= 20) return "Poor";
        return "Unknown";
    }
}