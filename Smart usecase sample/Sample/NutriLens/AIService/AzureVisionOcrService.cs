using System.Net.Http.Headers;
using System.Text.Json;

namespace NutriLens.Services;

public sealed class AzureVisionOcrService : IIngredientOcrService
{
    // Azure AI Vision resource, NOT Azure OpenAI 
    private const string VisionEndpoint = AzureOpenAIConfig.BaseEndpoint;

    private const string DeploymentName = AzureOpenAIConfig.DeploymentName;

    private const string VisionKey = AzureOpenAIConfig.ApiKey;

    private static readonly HttpClient Http = new()
    {
        Timeout = TimeSpan.FromSeconds(90)
    };

    public async Task<string> ExtractTextAsync(
        FileResult image,
        CancellationToken cancellationToken = default)
    {
        if (image is null)
            throw new ArgumentNullException(nameof(image));

        if (string.IsNullOrWhiteSpace(image.FullPath))
            throw new InvalidOperationException("The selected image is invalid.");

        using var imageStream = await image.OpenReadAsync();
        using var memoryStream = new MemoryStream();
        await imageStream.CopyToAsync(memoryStream, cancellationToken);

        var bytes = memoryStream.ToArray();

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{VisionEndpoint}/vision/v3.2/read/analyze");

        request.Headers.Add("Ocp-Apim-Subscription-Key", VisionKey);
        request.Headers.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        request.Content = new ByteArrayContent(bytes);
        request.Content.Headers.ContentType =
            new MediaTypeHeaderValue(GetMediaType(image.FileName));

        using var response = await Http.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        var rawResponse = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Azure Vision OCR failed. HTTP {(int)response.StatusCode}: {rawResponse}");
        }

        // Azure Read API can return 202 Accepted with an Operation-Location
        if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
        {
            var operationLocation = response.Headers
                .TryGetValues("Operation-Location", out var values)
                ? values.FirstOrDefault()
                : null;

            if (string.IsNullOrWhiteSpace(operationLocation))
            {
                throw new InvalidOperationException(
                    "Azure Vision accepted the OCR job but did not return an operation location.");
            }

            for (var attempt = 0; attempt < 20; attempt++)
            {
                await Task.Delay(1000, cancellationToken);

                using var pollRequest = new HttpRequestMessage(
                    HttpMethod.Get,
                    operationLocation);

                pollRequest.Headers.Add("Ocp-Apim-Subscription-Key", VisionKey);

                using var pollResponse = await Http.SendAsync(
                    pollRequest,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken);

                var pollBody = await pollResponse.Content.ReadAsStringAsync(cancellationToken);

                if (!pollResponse.IsSuccessStatusCode)
                {
                    throw new HttpRequestException(
                        $"Azure Vision OCR polling failed. HTTP {(int)pollResponse.StatusCode}: {pollBody}");
                }

                var extracted = TryExtractReadText(pollBody);

                if (!string.IsNullOrWhiteSpace(extracted))
                {
                    return extracted;
                }
            }

            throw new InvalidOperationException(
                "Azure Vision OCR completed but returned no readable text.");
        }

        var extractedText = TryExtractReadText(rawResponse);

        if (string.IsNullOrWhiteSpace(extractedText))
        {
            throw new InvalidOperationException(
                "No readable text was detected from the product image.");
        }

        return extractedText;
    }

    private static string TryExtractReadText(string rawResponse)
    {
        try
        {
            using var document = JsonDocument.Parse(rawResponse);

            if (document.RootElement.TryGetProperty("readResult", out var readResult))
            {
                var lines = new List<string>();

                if (readResult.TryGetProperty("blocks", out var blocks))
                {
                    foreach (var block in blocks.EnumerateArray())
                    {
                        if (block.TryGetProperty("lines", out var blockLines))
                        {
                            foreach (var line in blockLines.EnumerateArray())
                            {
                                if (line.TryGetProperty("text", out var textElement))
                                {
                                    var value = textElement.GetString();
                                    if (!string.IsNullOrWhiteSpace(value))
                                    {
                                        lines.Add(value.Trim());
                                    }
                                }
                            }
                        }
                    }
                }

                var combined = string.Join(Environment.NewLine, lines);
                if (!string.IsNullOrWhiteSpace(combined))
                    return combined;
            }

            if (document.RootElement.TryGetProperty("analyzeResult", out var analyzeResult))
            {
                if (analyzeResult.TryGetProperty("readResult", out var nestedReadResult))
                {
                    var lines = new List<string>();

                    if (nestedReadResult.TryGetProperty("blocks", out var blocks))
                    {
                        foreach (var block in blocks.EnumerateArray())
                        {
                            if (block.TryGetProperty("lines", out var blockLines))
                            {
                                foreach (var line in blockLines.EnumerateArray())
                                {
                                    if (line.TryGetProperty("text", out var textElement))
                                    {
                                        var value = textElement.GetString();
                                        if (!string.IsNullOrWhiteSpace(value))
                                        {
                                            lines.Add(value.Trim());
                                        }
                                    }
                                }
                            }
                        }
                    }

                    var combined = string.Join(Environment.NewLine, lines);
                    if (!string.IsNullOrWhiteSpace(combined))
                        return combined;
                }
            }
        }
        catch (JsonException)
        {
            // Intentionally ignored; no readable OCR result.
        }

        return string.Empty;
    }

    private static string GetMediaType(string fileName)
    {
        return Path.GetExtension(fileName)
            .ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            _ => "image/jpeg"
        };
    }
}