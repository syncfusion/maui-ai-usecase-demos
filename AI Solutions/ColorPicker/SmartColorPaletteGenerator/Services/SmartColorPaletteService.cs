using System.Text.Json;

namespace SmartColorPaletteGenerator
{
    /// <summary>
    /// ISmartColorPaletteService is an interface that defines the contract for a service that generates palette recommendations.
    /// </summary>
    public interface ISmartColorPaletteService
    {
        Task<IReadOnlyList<ColorPaletteResponse>> GeneratePaletteVariationsAsync(string styleDescription);
    }

    /// <summary>
    /// SmartColorPaletteService is a concrete implementation of the ISmartColorPaletteService interface.
    /// </summary>
    public class SmartColorPaletteService : ISmartColorPaletteService
    {
        private readonly IAzureAIService aiService;

        /// <summary>
        /// Initializes a new instance of the SmartColorPaletteService class with the specified IAzureAIService.
        /// </summary>
        /// <param name="aiService"></param>
        public SmartColorPaletteService(IAzureAIService aiService)
        {
            this.aiService = aiService;
        }

        /// <summary>
        /// Generates three palette recommendations based on the provided design style description.
        /// </summary>
        /// <param name="styleDescription"></param>
        /// <returns></returns>
        public async Task<IReadOnlyList<ColorPaletteResponse>> GeneratePaletteVariationsAsync(string styleDescription)
        {
            var prompt =
                "You are a professional UI/UX color designer and naming expert.\n\n" +
                "Analyze the following product or app description:\n\n" +
                $"\"{styleDescription}\"\n\n" +
                "Return ONLY valid JSON as an array of exactly three palette recommendations.\n" +
                "Each recommendation must include: themeName, matchPercentage, colors (five hex values), and rationale.\n\n" +
                "JSON format:\n" +
                "[\n" +
                "  {\n" +
                "    \"themeName\": \"...\",\n" +
                "    \"matchPercentage\": 92,\n" +
                "    \"colors\": [\"#HEX\", \"#HEX\", \"#HEX\", \"#HEX\", \"#HEX\"],\n" +
                "    \"rationale\": \"...\"\n" +
                "  }\n" +
                "]\n\n" +
                "Guidelines:\n" +
                "- Generate three distinct recommendations with unique names and color combinations.\n" +
                "- Match percentages should be believable, sorted highest to lowest.\n" +
                "- Colors must be visually accessible and relevant to the prompt.\n" +
                "- Return only JSON without markdown or commentary.";

            var response = await aiService.GetResultsFromAI(prompt);
            System.Diagnostics.Debug.WriteLine($"[SmartColorPaletteService] Prompt: {styleDescription}");
            System.Diagnostics.Debug.WriteLine($"[SmartColorPaletteService] Raw AI response: {response}");
            System.Diagnostics.Debug.WriteLine("=================================");
            System.Diagnostics.Debug.WriteLine($"Prompt : {styleDescription}");
            System.Diagnostics.Debug.WriteLine("=================================");

            System.Diagnostics.Debug.WriteLine(response);

            System.Diagnostics.Debug.WriteLine("=================================");

            return ParseResponse(response, styleDescription);
        }

        private static IReadOnlyList<ColorPaletteResponse> ParseResponse(string response, string styleDescription)
        {
            var fallback = CreateFallbackPalettes(styleDescription);
            if (string.IsNullOrWhiteSpace(response))
            {
                System.Diagnostics.Debug.WriteLine("[SmartColorPaletteService] Empty AI response. Using fallback palettes.");
                return fallback;
            }

            try
            {
                var json = ExtractJson(response);
                if (string.IsNullOrWhiteSpace(json))
                {
                    System.Diagnostics.Debug.WriteLine("[SmartColorPaletteService] No JSON block found in AI response. Using fallback palettes.");
                    return fallback;
                }

                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind != JsonValueKind.Array)
                {
                    System.Diagnostics.Debug.WriteLine("[SmartColorPaletteService] Parsed JSON is not an array. Using fallback palettes.");
                    return fallback;
                }

                var results = new List<ColorPaletteResponse>();
                foreach (var item in doc.RootElement.EnumerateArray())
                {
                    if (item.ValueKind != JsonValueKind.Object)
                        continue;

                    var colors = new List<string>();
                    if (item.TryGetProperty("colors", out var colorsElement) && colorsElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var color in colorsElement.EnumerateArray())
                        {
                            if (color.ValueKind == JsonValueKind.String)
                            {
                                var hex = color.GetString();
                                if (!string.IsNullOrWhiteSpace(hex))
                                    colors.Add(hex);
                            }
                        }
                    }

                    while (colors.Count < 5)
                        colors.Add(fallback[results.Count % fallback.Count].Colors[colors.Count]);

                    results.Add(new ColorPaletteResponse
                    {
                        ThemeName = GetString(item, "themeName", fallback[results.Count % fallback.Count].ThemeName),
                        MatchPercentage = GetInt(item, "matchPercentage", fallback[results.Count % fallback.Count].MatchPercentage),
                        Colors = colors.Take(5).ToList(),
                        Rationale = GetString(item, "rationale", fallback[results.Count % fallback.Count].Rationale)
                    });
                }

                System.Diagnostics.Debug.WriteLine($"[SmartColorPaletteService] Parsed {results.Count} palette(s) from AI response.");
                foreach (var result in results)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"Theme : {result.ThemeName}");

                    System.Diagnostics.Debug.WriteLine(
                        $"Colors : {string.Join(", ", result.Colors)}");
                }
                return results.Count >= 3 ? results.Take(3).ToList() : MergeWithFallback(results, fallback);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SmartColorPaletteService] Failed to parse AI response: {ex.Message}");
            }

            System.Diagnostics.Debug.WriteLine("[SmartColorPaletteService] Falling back to default palettes.");
            return fallback;
        }

        private static List<ColorPaletteResponse> CreateFallbackPalettes(string styleDescription)
        {
            return new List<ColorPaletteResponse>
            {
                new()
                {
                    ThemeName = $"{styleDescription} Studio",
                    MatchPercentage = 96,
                    Colors = new List<string> { "#2563EB", "#60A5FA", "#93C5FD", "#EFF6FF", "#1E3A8A" },
                    Rationale = "Balanced blues with a clean, trustworthy feel."
                },
                new()
                {
                    ThemeName = $"{styleDescription} Breeze",
                    MatchPercentage = 91,
                    Colors = new List<string> { "#0F766E", "#14B8A6", "#5EEAD4", "#CCFBF1", "#134E4A" },
                    Rationale = "Fresh teal tones for a modern, calming interface."
                },
                new()
                {
                    ThemeName = $"{styleDescription} Midnight",
                    MatchPercentage = 88,
                    Colors = new List<string> { "#111827", "#374151", "#6366F1", "#C7D2FE", "#F9FAFB" },
                    Rationale = "Deep contrast with a polished, premium visual language."
                }
            };
        }

        private static List<ColorPaletteResponse> MergeWithFallback(List<ColorPaletteResponse> results, List<ColorPaletteResponse> fallback)
        {
            while (results.Count < 3)
            {
                results.Add(fallback[results.Count]);
            }

            return results.Take(3).ToList();
        }

        private static string ExtractJson(string response)
        {
            var start = response.IndexOf('[');
            var end = response.LastIndexOf(']');
            if (start < 0 || end <= start)
                return string.Empty;

            return response.Substring(start, end - start + 1);
        }

        private static string GetString(JsonElement root, string propertyName, string fallback)
        {
            return root.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.String
                ? prop.GetString() ?? fallback
                : fallback;
        }

        private static int GetInt(JsonElement root, string propertyName, int fallback)
        {
            return root.TryGetProperty(propertyName, out var prop) && prop.TryGetInt32(out var value)
                ? value
                : fallback;
        }
    }
}
