using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TravelPlanner.Models;

namespace TravelPlanner.Services
{
    public class OpenAIServices
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private static readonly HttpClient _httpClient = new HttpClient();

        private static readonly string _endpoint = (Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? string.Empty).TrimEnd('/');
        private static readonly string _apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY") ?? string.Empty;
        private static readonly string _deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_CHAT_DEPLOYMENT") ?? "gpt-4o-mini";
        private static readonly string _apiVersion = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_VERSION") ?? "2024-08-01-preview";
        private static readonly string UnsplashAccessKey = Environment.GetEnvironmentVariable("UNSPLASH_ACCESS_KEY") ?? string.Empty;

        // True when Azure OpenAI credentials look usable (non-empty, non-placeholder).
        public static bool HasAzureCredentials =>
            !string.IsNullOrWhiteSpace(_endpoint) &&
            !string.IsNullOrWhiteSpace(_apiKey) &&
            !_apiKey.StartsWith("YOUR_", StringComparison.OrdinalIgnoreCase) &&
            !_apiKey.StartsWith("GIVE_", StringComparison.OrdinalIgnoreCase);

        public static bool HasUnsplashKey => !string.IsNullOrWhiteSpace(UnsplashAccessKey);

        public OpenAIServices()
        {
            // All configuration is read from environment variables (AZURE_OPENAI_* and UNSPLASH_ACCESS_KEY).
            // Do not hardcode secrets in source. See README.md for setup instructions.
        }

        public async Task<TripRecommendation> GetTripRecommendationsAsync(TripInput input, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(input);

            if (!HasAzureCredentials)
                return new TripRecommendation();

            string? content = null;

            try
            {
                var systemPrompt =
                    "You are a travel-planning assistant. Return strictly JSON following this schema: " +
                    "{ \"destinationSummary\": string, " +
                    "\"topPlaces\": [ { \"imageUrl\": string, \"name\": string, \"address\": string, \"rating\": number, \"location\": {latitude,longitude} } ], " +
                    "\"hotels\": [ { \"imageUrl\": string, \"name\": string, \"address\": string, \"rating\": number } ], " +
                    "\"foodSpots\": [ { \"imageUrl\": string, \"name\": string, \"address\": string, \"rating\": number } ] } " +
                    "Ensure exactly 5 items in topPlaces, 5 hotels, and 5 foodSpots. Use public image URLs.";

                var userPrompt =
                    $"Destination: {input.Destination}\n" +
                    $"Dates: {input.StartDate:yyyy-MM-dd} to {input.EndDate:yyyy-MM-dd}\n" +
                    $"Trip type: {input.TripType}\n" +
                    $"Interests: {string.Join(", ", input.Interests)}\n" +
                    "Provide concise, factual data for address and realistic ratings (1.0–5.0).";

                var url = $"{_endpoint}/openai/deployments/{_deploymentName}/chat/completions?api-version={_apiVersion}";
                using var req = new HttpRequestMessage(HttpMethod.Post, url);
                req.Headers.TryAddWithoutValidation("api-key", _apiKey);

                var payload = new
                {
                    model = _deploymentName,
                    messages = new object[]
                    {
                        new { role = "system", content = systemPrompt },
                        new { role = "user", content = userPrompt }
                    },
                    top_p = 1.0
                };
                req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                using var resp = await _httpClient.SendAsync(req, ct);
                if (!resp.IsSuccessStatusCode)
                {
                    var errBody = await resp.Content.ReadAsStringAsync(ct);
                    throw new HttpRequestException($"Azure OpenAI request failed with {(int)resp.StatusCode} {resp.ReasonPhrase}: {errBody}");
                }
                using var stream = await resp.Content.ReadAsStreamAsync(ct);
                using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
                content = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                if (string.IsNullOrWhiteSpace(content))
                    return new TripRecommendation();
            }
            catch (System.Net.Sockets.SocketException)
            {
                // Skip if network/socket error (invalid credentials, wrong endpoint, etc.)
                return new TripRecommendation();
            }
            catch (HttpRequestException)
            {
                // Skip if HTTP error (invalid credentials, bad endpoint, etc.)
                return new TripRecommendation();
            }
            catch
            {
                // Skip on any other error
                return new TripRecommendation();
            }

            if (content.StartsWith("```"))
            {
                int start = content.IndexOf('{');
                int end = content.LastIndexOf('}');
                if (start >= 0 && end > start)
                    content = content.Substring(start, end - start + 1);
            }

            TripRecommendation result;
            try
            {
                using var parsed = JsonDocument.Parse(content);
                var root = parsed.RootElement;
                result = new TripRecommendation
                {
                    DestinationSummary = TryGetString(root, "destinationSummary", "summary", "description")
                };

                var topTask = BuildCardsAsync(root, "topPlaces", fallbackName: "Attraction", ct);
                var hotelTask = BuildCardsAsync(root, "hotels", fallbackName: "Hotel", ct);
                var foodTask = BuildCardsAsync(root, "foodSpots", fallbackName: "Food Spot", ct);
                await Task.WhenAll(topTask, hotelTask, foodTask);

                result.TopPlaces = topTask.Result;
                result.Hotels = hotelTask.Result;
                result.FoodSpots = foodTask.Result;
            }
            catch
            {
                // Fallback to plain deserialize if strict parse fails
                result = JsonSerializer.Deserialize<TripRecommendation>(content, JsonOptions) ?? new TripRecommendation();
                result.TopPlaces ??= new();
                result.Hotels ??= new();
                result.FoodSpots ??= new();
            }

            // Normalize and provide fallbacks so UI always shows name and address
            static void Normalize(System.Collections.Generic.List<PlaceCard> items, string fallbackNamePrefix)
            {
                if (items == null) return;
                foreach (var it in items)
                {
                    if (it == null) continue;
                    if (string.IsNullOrWhiteSpace(it.Name))
                        it.Name = fallbackNamePrefix;
                    if (string.IsNullOrWhiteSpace(it.Address))
                        it.Address = "Address not available";
                    if (string.IsNullOrWhiteSpace(it.ImageUrl))
                        it.ImageUrl = "https://via.placeholder.com/200x150.png?text=Image";
                    if (it.Rating <= 0)
                        it.Rating = 4.2;
                }
            }

            Normalize(result.TopPlaces, "Attraction");
            Normalize(result.Hotels, "Hotel");
            Normalize(result.FoodSpots, "Food Spot");
            return result;

        }

        public async Task<string> GetRawResponseAsync(string systemPrompt, string userPrompt, CancellationToken ct = default)
        {
            if (!HasAzureCredentials)
                return string.Empty;

            try
            {
                if (string.IsNullOrWhiteSpace(systemPrompt)) systemPrompt = "You are a helpful assistant.";
                if (string.IsNullOrWhiteSpace(userPrompt)) return string.Empty;

                var url = $"{_endpoint}/openai/deployments/{_deploymentName}/chat/completions?api-version={_apiVersion}";
                using var req = new HttpRequestMessage(HttpMethod.Post, url);
                req.Headers.TryAddWithoutValidation("api-key", _apiKey);

                var payload = new
                {
                    messages = new object[]
                    {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt ?? string.Empty }
                    },
                    max_tokens = 1400,
                    top_p = 1.0
                };
                req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                using var resp = await _httpClient.SendAsync(req, ct);
                if (!resp.IsSuccessStatusCode)
                {
                    var errBody = await resp.Content.ReadAsStringAsync(ct);
                    throw new HttpRequestException($"Azure OpenAI request failed with {(int)resp.StatusCode} {resp.ReasonPhrase}: {errBody}");
                }
                using var stream = await resp.Content.ReadAsStreamAsync(ct);
                using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
                var content = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();
                return content ?? string.Empty;
            }
            catch (System.Net.Sockets.SocketException)
            {
                // Skip on network error
                return string.Empty;
            }
            catch (HttpRequestException)
            {
                // Skip on HTTP error (invalid credentials, bad endpoint, etc.)
                return string.Empty;
            }
            catch
            {
                // Skip on any other error
                return string.Empty;
            }
        }

        // Helper methods moved to class scope so async image generation can be awaited safely
        private static string TryGetString(JsonElement obj, params string[] names)
        {
            foreach (var n in names)
            {
                if (obj.TryGetProperty(n, out var v))
                {
                    if (v.ValueKind == JsonValueKind.String) return v.GetString() ?? string.Empty;
                    if (v.ValueKind == JsonValueKind.Number) return v.ToString();
                }
            }
            return string.Empty;
        }

        private static double TryGetDouble(JsonElement obj, params string[] names)
        {
            foreach (var n in names)
            {
                if (obj.TryGetProperty(n, out var v))
                {
                    if (v.ValueKind == JsonValueKind.Number && v.TryGetDouble(out var d)) return d;
                    if (v.ValueKind == JsonValueKind.String && double.TryParse(v.GetString(), out var ds)) return ds;
                }
            }
            return 0;
        }

        private async Task<System.Collections.Generic.List<PlaceCard>> BuildCardsAsync(JsonElement root, string arrayName, string fallbackName, CancellationToken ct = default)
        {
            var list = new System.Collections.Generic.List<PlaceCard>();
            if (!root.TryGetProperty(arrayName, out var rootArr) || rootArr.ValueKind != JsonValueKind.Array)
                return list;

            foreach (var item in rootArr.EnumerateArray())
            {
                var name = TryGetString(item, "name", "title", "hotelName", "restaurantName");
                var address = TryGetString(item, "address", "location", "formattedAddress", "vicinity");
                var rating = TryGetDouble(item, "rating", "score", "stars");
                string imageTarget = string.IsNullOrWhiteSpace(name) ? fallbackName : name;
                string imageUrl = "image_not_available.png";

                if (HasUnsplashKey)
                {
                    try
                    {
                        imageUrl = await GetImage(imageTarget);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Image lookup failed for '{imageTarget}': {ex.Message}");
                        imageUrl = "image_not_available.png";
                    }
                }

                var card = new PlaceCard
                {
                    ImageUrl = imageUrl,
                    Name = string.IsNullOrWhiteSpace(name) ? fallbackName : name,
                    Address = string.IsNullOrWhiteSpace(address) ? "Address not available" : address,
                    Rating = rating <= 0 ? 4.2 : rating
                };

                // Extract coordinates from common shapes in the AI response
                try
                {
                    if (item.TryGetProperty("location", out var loc))
                    {
                        if (loc.ValueKind == JsonValueKind.Object)
                        {
                            var lat = TryGetDouble(loc, "latitude", "lat");
                            var lon = TryGetDouble(loc, "longitude", "lon", "lng");
                            if (lat != 0 && lon != 0)
                            {
                                card.Latitude = lat;
                                card.Longitude = lon;
                            }
                        }
                        else if (loc.ValueKind == JsonValueKind.Array)
                        {
                            try
                            {
                                var arr = loc.EnumerateArray().ToArray();
                                if (arr.Length >= 2 && arr[0].TryGetDouble(out var a0) && arr[1].TryGetDouble(out var a1))
                                {
                                    // Heuristic: first value in [-90,90] is latitude
                                    if (a0 >= -90 && a0 <= 90)
                                    {
                                        card.Latitude = a0;
                                        card.Longitude = a1;
                                    }
                                    else
                                    {
                                        card.Latitude = a1;
                                        card.Longitude = a0;
                                    }
                                }
                            }
                            catch { }
                        }
                        else if (loc.ValueKind == JsonValueKind.String)
                        {
                            var s = loc.GetString();
                            if (!string.IsNullOrWhiteSpace(s))
                            {
                                var parts = s.Split(new[] {',',' '}, StringSplitOptions.RemoveEmptyEntries);
                                if (parts.Length >= 2
                                    && double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var p0)
                                    && double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var p1))
                                {
                                    // same heuristic
                                    if (p0 >= -90 && p0 <= 90)
                                    {
                                        card.Latitude = p0;
                                        card.Longitude = p1;
                                    }
                                    else
                                    {
                                        card.Latitude = p1;
                                        card.Longitude = p0;
                                    }
                                }
                            }
                        }
                    }

                    // Fallback: some responses include "coordinates" or "geo"
                    if (!card.Latitude.HasValue || !card.Longitude.HasValue)
                    {
                        if (item.TryGetProperty("coordinates", out var coords) && coords.ValueKind == JsonValueKind.Array)
                        {
                            try
                            {
                                var arr = coords.EnumerateArray().ToArray();
                                if (arr.Length >= 2 && arr[0].TryGetDouble(out var c0) && arr[1].TryGetDouble(out var c1))
                                {
                                    if (c0 >= -90 && c0 <= 90)
                                    {
                                        card.Latitude = c0;
                                        card.Longitude = c1;
                                    }
                                    else
                                    {
                                        card.Latitude = c1;
                                        card.Longitude = c0;
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                }
                catch { }

                list.Add(card);
            }

            return list;
        }

        private async Task<string> GetImage(string location)
        {
            string query = location.Trim();
            if (string.IsNullOrEmpty(query) || !HasUnsplashKey)
                return "image_not_available.png";

            try
            {
                string url = $"https://api.unsplash.com/search/photos?query={Uri.EscapeDataString(query)}&client_id={UnsplashAccessKey}&per_page=1";
                var response = await _httpClient.GetStringAsync(url);

                using var doc = JsonDocument.Parse(response);
                var results = doc.RootElement.GetProperty("results");

                if (results.GetArrayLength() > 0)
                {
                    return results[0].GetProperty("urls").GetProperty("regular").GetString() ?? "image_not_available.png";
                }

                return "image_not_available.png";
            }
            catch
            {
                return "image_not_available.png";
            }
        }
    }
}
