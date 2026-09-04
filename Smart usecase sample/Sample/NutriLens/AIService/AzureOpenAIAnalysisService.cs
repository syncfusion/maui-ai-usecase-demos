
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using NutriLens.Models;

namespace NutriLens.Services;

public static class AzureOpenAIConfig
{
    public const string BaseEndpoint = "https://your-openai-resource.openai.azure.com/";
     
    public const string DeploymentName = "your-DeploymentName";
     
    public const string ApiKey =
        "your-azure-openai-api-key";

     
}
public sealed class AzureOpenAIAnalysisService
    : IAzureOpenAIAnalysisService
{
    private const string BaseEndpoint = AzureOpenAIConfig.BaseEndpoint; 

    private const string DeploymentName = AzureOpenAIConfig.DeploymentName;

    private const string ApiKey =AzureOpenAIConfig.ApiKey;

    private static readonly HttpClient Http = new()
    {
        Timeout = TimeSpan.FromSeconds(90)
    };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<ProductAnalysis> AnalyzeAsync(
        FileResult image,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var imageStream =
            await image.OpenReadAsync();

        using var memoryStream = new MemoryStream();

        await imageStream.CopyToAsync(
            memoryStream,
            cancellationToken);

        var imageBytes = memoryStream.ToArray();
        var base64Image = Convert.ToBase64String(imageBytes);
        var mediaType = GetMediaType(image.FileName);

        var requestBody = CreateRequestBody(
            base64Image,
            mediaType);

        var requestJson =
            JsonSerializer.Serialize(requestBody);

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{BaseEndpoint}/chat/completions");

        request.Headers.Add("api-key", ApiKey);

        // Avoid the StringContent constructor overload issue.
        var content = new StringContent(
            requestJson,
            Encoding.UTF8);

        content.Headers.ContentType =
            new MediaTypeHeaderValue("application/json");

        request.Content = content;

        using var response = await Http.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        var rawResponse =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        System.Diagnostics.Debug.WriteLine(
            $"[NutriLens AI] Status: {(int)response.StatusCode}");

        System.Diagnostics.Debug.WriteLine(
            $"[NutriLens AI] Response: {rawResponse}");

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Azure OpenAI request failed. " +
                $"HTTP {(int)response.StatusCode}: {rawResponse}");
        }

        return ParseResponse(rawResponse);
    }

    private static object CreateRequestBody(
        string base64Image,
        string mediaType)
    {
        var imageDataUrl =
            $"data:{mediaType};base64,{base64Image}";

        return new
        {
            model = DeploymentName,

            messages = new object[]
            {
                new
                {
                    role = "system",
                    content = """
You are NutriLens, an AI food-label analysis assistant.

Analyze the supplied food package, ingredient list, or nutrition label.
Use OCR and image understanding.
Return only valid JSON.
Do not invent unavailable information.
Use null, empty strings, or empty arrays when data is unavailable.
The overall score must be between 0 and 100.
Risk levels must be High, Moderate, Low, or Unknown.
This information is educational and is not medical advice.
"""
                },

                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new
                        {
                            type = "text",
                            text = """
Analyze this food product image.

Return JSON using this structure:

{
  "productName": "string",
  "ingredientSummary": "string",
  "fullIngredients": "string",
  "ingredients": [
    {
      "name": "string",
      "purpose": "string",
      "riskLevel": "High|Moderate|Low|Unknown",
      "explanation": "string"
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
  "preservatives": [],
  "artificialColors": [],
  "allergens": [],
  "additives": [],
  "healthAssessment": {
    "overallScore": 0,
    "verdict": "string",
    "scoreExplanation": "string",
    "positiveFactors": [],
    "negativeFactors": [],
    "riskIndicators": [
      {
        "title": "string",
        "description": "string",
        "riskLevel": "High|Moderate|Low|Unknown"
      }
    ],
    "healthInsights": "string",
    "recommendation": "string"
  },
  "confidence": "High|Moderate|Low|Unknown"
}
"""
                        },

                        new
                        {
                            type = "image_url",
                            image_url = new
                            {
                                url = imageDataUrl
                            }
                        }
                    }
                }
            },

            temperature = 0.1,
            max_completion_tokens = 3000,

            response_format = new
            {
                type = "json_object"
            }
        };
    }

    private static ProductAnalysis ParseResponse(
        string rawResponse)
    {
        using var document =
            JsonDocument.Parse(rawResponse);

        if (!document.RootElement.TryGetProperty(
                "choices",
                out var choices) ||
            choices.GetArrayLength() == 0)
        {
            throw new InvalidOperationException(
                "Azure response does not contain choices.");
        }

        var content = choices[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException(
                "Azure returned empty response content.");
        }

        var analysis =
            JsonSerializer.Deserialize<ProductAnalysis>(
                content,
                JsonOptions);

        if (analysis is null)
        {
            throw new InvalidOperationException(
                "Azure returned invalid analysis JSON.");
        }

        analysis.HealthAssessment.OverallScore =
            Math.Clamp(
                analysis.HealthAssessment.OverallScore,
                0,
                100);

        for (var index = 0;
             index < analysis.Ingredients.Count;
             index++)
        {
            analysis.Ingredients[index].Number =
                (index + 1).ToString("00");
        }

        return analysis;
    }

    private static string GetMediaType(
        string fileName)
    {
        return Path.GetExtension(fileName)
            .ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            _ => "image/jpeg"
        };
    }
}