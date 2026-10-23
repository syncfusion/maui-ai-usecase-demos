using System.Text.Json;

namespace SmartVehicleCare.Services;

// Lightweight wrapper over a parsed JSON array so callers can consume
// System.Text.Json results without lifetime constraints on JsonDocument.
internal class JsonElementArray : List<JsonElement>
{
    public JsonElementArray() { }
    public JsonElementArray(IEnumerable<JsonElement> items) : base(items) { }
}

internal class ServiceCenterDataHelper
{
    // Primary + mirrors tried in order until one succeeds
    private static readonly string[] OverpassServers =
    {
        "https://overpass-api.de/api/interpreter",
        "https://overpass.kumi.systems/api/interpreter",
        "https://maps.mail.ru/osm/tools/overpass/api/interpreter",
    };

    // Shared client — avoids socket exhaustion from per-call creation
    private static readonly HttpClient Http = new()
    {
        Timeout = TimeSpan.FromSeconds(25),
        DefaultRequestHeaders = { { "User-Agent", "SmartVehicleCare/1.0 (vehicle-care-ai-sample)" } }
    };

    internal string LastQuery { get; private set; } = string.Empty;
    internal string LastRawResponse { get; private set; } = string.Empty;

    // mode: "Fuel" → fuel stations only; "Service" → repair/service shops only; null → both
    internal async Task<JsonElement?> GetNearbyPlacesAsync(
        double latitude, double longitude,
        int radiusKm = 10, int maxResults = 8,
        string? mode = null)
    {
        var r      = radiusKm * 1000;
        var latStr = latitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var lonStr = longitude.ToString(System.Globalization.CultureInfo.InvariantCulture);

        string nodeFilters;
        if (string.Equals(mode, "Fuel", StringComparison.OrdinalIgnoreCase))
        {
            nodeFilters = $"""
                  node["amenity"="fuel"](around:{r},{latStr},{lonStr});
                """;
        }
        else if (string.Equals(mode, "Service", StringComparison.OrdinalIgnoreCase))
        {
            nodeFilters = $"""
                  node["amenity"="car_repair"](around:{r},{latStr},{lonStr});
                  node["amenity"="car_wash"](around:{r},{latStr},{lonStr});
                  node["amenity"="vehicle_inspection"](around:{r},{latStr},{lonStr});
                  node["shop"="car_repair"](around:{r},{latStr},{lonStr});
                  node["shop"="car"](around:{r},{latStr},{lonStr});
                  node["shop"="car_parts"](around:{r},{latStr},{lonStr});
                """;
        }
        else
        {
            nodeFilters = $"""
                  node["amenity"="fuel"](around:{r},{latStr},{lonStr});
                  node["amenity"="car_repair"](around:{r},{latStr},{lonStr});
                  node["amenity"="car_wash"](around:{r},{latStr},{lonStr});
                  node["amenity"="vehicle_inspection"](around:{r},{latStr},{lonStr});
                  node["shop"="car_repair"](around:{r},{latStr},{lonStr});
                  node["shop"="car"](around:{r},{latStr},{lonStr});
                  node["shop"="car_parts"](around:{r},{latStr},{lonStr});
                """;
        }

        var query = $"[out:json][timeout:25];\n(\n{nodeFilters}\n);\nout center {maxResults};";
        LastQuery = query;

        // Try each Overpass server in turn; stop on first successful response
        Exception? lastEx = null;
        foreach (var server in OverpassServers)
        {
            try
            {
                using var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("data", query)
                });
                using var request = new HttpRequestMessage(HttpMethod.Post, server)
                {
                    Content = content
                };
                request.Headers.Accept.ParseAdd("application/json");

                using var response = await Http.SendAsync(request);
                var responseText = await response.Content.ReadAsStringAsync();
                LastRawResponse = responseText;

                // HTML response = server error/busy — try next mirror
                if (responseText.TrimStart().StartsWith("<"))
                    continue;

                if (!response.IsSuccessStatusCode)
                    continue;

                if (string.IsNullOrWhiteSpace(responseText)) continue;

                using var doc = JsonDocument.Parse(responseText);
                var root = doc.RootElement;
                if (root.ValueKind != JsonValueKind.Object
                    || !root.TryGetProperty("elements", out var elements)
                    || elements.ValueKind != JsonValueKind.Array
                    || elements.GetArrayLength() == 0)
                    continue;

                var results = new List<JsonElement>();
                foreach (var element in elements.EnumerateArray())
                {
                    if (element.ValueKind != JsonValueKind.Object) continue;

                    var lat = GetDouble(element, "lat") ?? GetDouble(element, "center", "lat");
                    var lon = GetDouble(element, "lon") ?? GetDouble(element, "center", "lon");
                    if (lat == null || lon == null) continue;

                    if (!element.TryGetProperty("tags", out var tags)
                        || tags.ValueKind != JsonValueKind.Object) continue;

                    var name        = GetString(tags, "name");
                    var amenity     = GetString(tags, "amenity");
                    var shop        = GetString(tags, "shop");
                    var serviceType = GetString(tags, "service");
                    var fuel        = GetString(tags, "fuel");
                    var type = !string.IsNullOrWhiteSpace(amenity) ? amenity
                              : !string.IsNullOrWhiteSpace(shop) ? shop
                              : !string.IsNullOrWhiteSpace(serviceType) ? serviceType
                              : "place";

                    var normalizedType = type switch
                    {
                        "fuel" => "Fuel Station",
                        "car_repair" => "Service Center",
                        "car" => "Auto Service",
                        "car_parts" => "Auto Parts",
                        "vehicle_inspection" => "Inspection Center",
                        _ => "Nearby Location",
                    };

                    if (string.IsNullOrWhiteSpace(name))
                        name = normalizedType == "Fuel Station" ? "Fuel Station" : "Service Center";

                    var addressParts = new[]
                    {
                        GetString(tags, "addr:street"),
                        GetString(tags, "addr:city"),
                        GetString(tags, "addr:postcode"),
                    };
                    var address = string.Join(", ", addressParts.Where(p => !string.IsNullOrWhiteSpace(p)));
                    if (string.IsNullOrWhiteSpace(address))
                        address = $"Near {latitude:F4}, {longitude:F4}";

                    var openingHoursRaw = GetString(tags, "opening_hours");
                    var openTime = string.IsNullOrWhiteSpace(openingHoursRaw)
                        ? "Hours unavailable"
                        : openingHoursRaw;

                    var detail = normalizedType == "Fuel Station"
                        ? "Nearby fuel station for refueling and quick top-ups"
                        : "Nearby auto service center for maintenance and repairs";

                    if (string.Equals(fuel, "diesel", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(fuel, "petrol", StringComparison.OrdinalIgnoreCase))
                        detail = "Fuel station offering petrol or diesel";

                    results.Add(JsonSerializer.SerializeToElement(new Dictionary<string, object?>
                    {
                        ["Name"]       = name,
                        ["Details"]    = detail,
                        ["OpenTime"]   = openTime,
                        ["Latitude"]   = lat.Value,
                        ["Longitude"]  = lon.Value,
                        ["Address"]    = address,
                        ["Type"]       = normalizedType,
                        ["DistanceKm"] = HaversineKm(latitude, longitude, lat.Value, lon.Value),
                    }));
                }

                if (results.Count == 0) continue;

                var ordered = results
                    .OrderBy(x => x.TryGetProperty("DistanceKm", out var d) && d.ValueKind != JsonValueKind.Null ? d.GetDouble() : double.MaxValue)
                    .Take(maxResults)
                    .ToList();
                return JsonSerializer.SerializeToElement(new Dictionary<string, object?> { ["markercollections"] = ordered });
            }
            catch (Exception ex)
            {
                lastEx = ex;
                System.Diagnostics.Debug.WriteLine($"[Overpass:{server}] {ex.Message}");
            }
        }

        // All servers failed — caller falls back to AI or static
        System.Diagnostics.Debug.WriteLine($"[Overpass] All servers failed. Last: {lastEx?.Message}");
        LastRawResponse = $"All Overpass servers unavailable. Last error: {lastEx?.Message}";
        return null;
    }

    // Shared helpers used by VehicleCenterViewModel for System.Text.Json payloads

    // Parses a raw JSON array, or an object wrapping the array under any common key
    internal static JsonElementArray? ParseJsonArray(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        if (root.ValueKind == JsonValueKind.Array)
            return new JsonElementArray(root.EnumerateArray().ToList());
        if (root.ValueKind == JsonValueKind.Object)
        {
            foreach (var key in new[] { "markercollections", "places", "locations", "results", "stations", "centers", "data" })
                if (root.TryGetProperty(key, out var value)
                    && value.ValueKind == JsonValueKind.Array)
                    return new JsonElementArray(value.EnumerateArray().ToList());
        }
        return null;
    }

    // Unwraps the "markercollections" array from a GetNearbyPlacesAsync result
    internal static JsonElementArray? UnwrapMarkerCollections(JsonElement? data)
    {
        if (data == null) return null;
        var value = data.Value;
        if (value.ValueKind != JsonValueKind.Object) return null;
        if (!value.TryGetProperty("markercollections", out var arr)
            || arr.ValueKind != JsonValueKind.Array) return null;
        return new JsonElementArray(arr.EnumerateArray().ToList());
    }

    internal static bool HasProperty(JsonElement obj, string name)
        => obj.ValueKind == JsonValueKind.Object && obj.TryGetProperty(name, out _);

    internal static string? GetStringOrNull(JsonElement obj, string name)
        => obj.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String
            ? v.GetString() : null;

    internal static double? GetDoubleOrNull(JsonElement obj, string name)
        => obj.TryGetProperty(name, out var v)
            ? v.ValueKind switch
            {
                JsonValueKind.Number => v.GetDouble(),
                JsonValueKind.String => double.TryParse(v.GetString(), out var d) ? d : null,
                _ => null,
            }
            : null;

    // Generates plausible sample markers near the user's GPS position for demo purposes
    internal static JsonElement GetStaticFallback(double lat, double lon, string? mode, int max)
    {
        var isService = string.Equals(mode, "Service", StringComparison.OrdinalIgnoreCase);
        var r = new Random(42);
        double Offset() => (r.NextDouble() - 0.5) * 0.1; // ±0.05 degrees ≈ ±5 km

        var items = isService
            ? new[]
            {
                ("Maruti Suzuki Authorised Service",  "Authorised workshop for Maruti Suzuki vehicles",   "Auto Nagar, Industrial Estate"),
                ("Hyundai Service Centre",            "Official Hyundai service and repair centre",        "Dealer Complex, Bypass Road"),
                ("Honda Cars Service Point",          "Honda authorised service workshop",                 "Service Road, Auto Hub"),
                ("TATA Motors Service Centre",        "Authorised TATA Motors workshop",                    "Auto Nagar, Phase 2"),
                ("Mahindra Service Centre",           "Mahindra authorised service centre",                 "NH-48, Service Lane"),
                ("Ford Authorised Workshop",          "Certified Ford vehicle service centre",             "Industrial Area, Workshop Zone"),
                ("Kia Service Centre",                "Kia Motors authorised workshop",                     "Showroom Complex, Outer Ring Road"),
                ("Toyota Service Point",              "Toyota authorised service and repair",              "Auto Mall, IT Expressway"),
            }
            : new[]
            {
                ("Indian Oil Petrol Pump",   "IndianOil fuel station — petrol & diesel",  "Highway Service Lane, Outer Ring Road"),
                ("BPCL Fuel Station",        "Bharat Petroleum — petrol, diesel & CNG",   "Main Road, Near Bus Stand"),
                ("HPCL Petrol Pump",         "Hindustan Petroleum fuel station",           "State Highway 7, Industrial Area"),
                ("Reliance Petroleum",       "Reliance fuel station — full services",      "Industrial Road, Phase 1"),
                ("Essar Fuel Station",       "Essar Oil petrol & diesel point",            "NH-48, Expressway Service Area"),
                ("Shell Fuel Station",       "Shell fuels and lubricants",                 "IT Corridor, Tech Park Junction"),
                ("Indian Oil — Highway",     "IndianOil station on national highway",      "National Highway 44"),
                ("BPCL Speed Fuels",         "BPCL Speed premium fuel station",            "City Centre, Commercial Complex"),
            };

        var arr = new List<JsonElement>();
        foreach (var (name, detail, address) in items.Take(max))
        {
            var mlat = lat + Offset();
            var mlon = lon + Offset();
            arr.Add(JsonSerializer.SerializeToElement(new Dictionary<string, object?>
            {
                ["Name"]       = name,
                ["Details"]    = detail,
                ["OpenTime"]   = isService ? "Mon–Sat: 8:00 AM – 7:00 PM" : "Open 24 Hrs",
                ["Latitude"]   = Math.Round(mlat, 6),
                ["Longitude"]  = Math.Round(mlon, 6),
                ["Address"]    = address,
                ["Type"]       = isService ? "Service Center" : "Fuel Station",
                ["DistanceKm"] = HaversineKm(lat, lon, mlat, mlon),
            }));
        }
        return JsonSerializer.SerializeToElement(new Dictionary<string, object?> { ["markercollections"] = arr });
    }

    private static string? GetString(JsonElement obj, params string[] names)
    {
        if (!obj.TryGetProperty(names[0], out var value)) return null;
        return value.ValueKind == JsonValueKind.String ? value.GetString() : null;
    }

    private static double? GetDouble(JsonElement obj, params string[] names)
    {
        if (names.Length == 2)
        {
            if (!obj.TryGetProperty(names[0], out var nested)
                || nested.ValueKind != JsonValueKind.Object) return null;
            return GetDouble(nested, names[1]);
        }
        if (!obj.TryGetProperty(names[0], out var value)) return null;
        return value.ValueKind switch
        {
            JsonValueKind.Number => value.GetDouble(),
            JsonValueKind.String => double.TryParse(value.GetString(), out var d) ? d : null,
            _ => null,
        };
    }

    private static double HaversineKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371;
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
              + Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180)
              * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }
}