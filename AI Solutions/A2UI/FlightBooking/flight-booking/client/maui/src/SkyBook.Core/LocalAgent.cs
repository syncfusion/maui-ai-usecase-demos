using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace SkyBook.Core;

public sealed record AzureOpenAiOptions(string Endpoint, string ApiKey, string Deployment, string ApiVersion = "2024-10-21")
{
  /// <summary>Marker used in <see cref="ApiVersion"/> for OpenAI-compatible endpoints (OpenRouter, OpenAI).</summary>
  public const string OpenAiCompatible = "openai-compatible";

  /// <summary>OpenRouter (same provider as the Python/Blazor agent). <paramref name="model"/> is e.g. "google/gemini-3.1-flash-lite".</summary>
  public static AzureOpenAiOptions OpenRouter(string apiKey, string model) =>
      new("https://openrouter.ai/api/v1", apiKey, model, OpenAiCompatible);

  /// <summary>True for OpenRouter / OpenAI endpoints (Bearer auth, model in body) — detected from the URL too,
  /// so an OpenRouter endpoint put into the Azure config section also works.</summary>
  public bool IsOpenAiCompatible =>
      ApiVersion == OpenAiCompatible
      || Endpoint.Contains("openrouter.ai", StringComparison.OrdinalIgnoreCase)
      || Endpoint.Contains("api.openai.com", StringComparison.OrdinalIgnoreCase);

  /// <summary>True only when endpoint, API key and deployment are all filled in (and the key is not a placeholder).</summary>
  public bool IsConfigured =>
      !string.IsNullOrWhiteSpace(Endpoint) &&
      !string.IsNullOrWhiteSpace(ApiKey) &&
      !string.IsNullOrWhiteSpace(Deployment) &&
      !ApiKey.TrimStart().StartsWith("your-", StringComparison.OrdinalIgnoreCase);
}

	/// <summary>Thrown when the Azure OpenAI endpoint / API key / deployment are missing or still placeholders.</summary>
	public sealed class AiNotConfiguredException() : InvalidOperationException(DefaultMessage)
	{
	public const string DefaultMessage =
	  "AI assistant is not available. Please include the Azure OpenAI credentials (endpoint, API key and deployment name).";
	}

	/// <summary>Thrown when the AI service answered with an error status (e.g. 401 for a wrong key).</summary>
	public sealed class AiRequestException(int statusCode, string detail)
	: InvalidOperationException($"AI request failed ({statusCode}): {detail}")
	{
	public int StatusCode { get; } = statusCode;
	public bool IsUnauthorized => StatusCode is 401 or 403;
	}

/// <summary>Flat trip/traveller inputs (same keys the search form's action context uses).</summary>
public sealed class Trip
{
  public string TripType = "", From = "", To = "", DepartureDate = "", ReturnDate = "", CabinClass = "";
  public int Adults, Children;
  public string FirstName = "", LastName = "", Email = "", PassportNumber = "", Nationality = "";

  public static Trip FromJson(JsonObject? o)
  {
    var t = new Trip();
    if (o is null) return t;
    t.TripType = NormTripType(S(o["tripType"]));
    t.From = S(o["from"]); t.To = S(o["to"]);
    t.DepartureDate = S(o["departureDate"]); t.ReturnDate = S(o["returnDate"]);
    t.CabinClass = NormCabin(S(o["cabinClass"]));
    t.Adults = I(o["adults"]); t.Children = I(o["children"]);
    t.FirstName = S(o["firstName"]); t.LastName = S(o["lastName"]); t.Email = S(o["email"]);
    t.PassportNumber = S(o["passportNumber"]); t.Nationality = S(o["nationality"]);
    return t;
  }

  /// <summary>Copy values from <paramref name="other"/> where it has something (later inputs win).</summary>
  public void Overlay(Trip other)
  {
    if (other.TripType != "") TripType = other.TripType;
    if (other.From != "") From = other.From;
    if (other.To != "") To = other.To;
    if (other.DepartureDate != "") DepartureDate = other.DepartureDate;
    if (other.ReturnDate != "") ReturnDate = other.ReturnDate;
    if (other.CabinClass != "") CabinClass = other.CabinClass;
    if (other.Adults > 0) Adults = other.Adults;
    if (other.Children > 0) Children = other.Children;
    if (other.FirstName != "") FirstName = other.FirstName;
    if (other.LastName != "") LastName = other.LastName;
    if (other.Email != "") Email = other.Email;
    if (other.PassportNumber != "") PassportNumber = other.PassportNumber;
    if (other.Nationality != "") Nationality = other.Nationality;
  }

  public List<string> MissingForSearch()
  {
    var m = new List<string>();
    if (From == "") m.Add("origin (From)");
    if (To == "") m.Add("destination (To)");
    if (DepartureDate == "") m.Add("departure date");
    if (TripType == "") m.Add("trip type");
    if (TripType == "roundTrip" && ReturnDate == "") m.Add("return date");
    if (CabinClass == "") m.Add("cabin class");
    if (Adults < 1) m.Add("number of adults");
    return m;
  }

  public List<string> MissingForBooking()
  {
    var m = MissingForSearch();
    if (FirstName == "") m.Add("traveller first name");
    if (LastName == "") m.Add("traveller last name");
    if (Email == "") m.Add("contact email");
    if (PassportNumber == "") m.Add("passport number");
    if (Nationality == "") m.Add("nationality");
    return m;
  }

  public string TripTypeLabel => TripType switch { "oneWay" => "One Way", "multiCity" => "Multi City", "roundTrip" => "Round Trip", _ => "Round Trip" };
  public string CabinLabel => CabinClass switch { "premiumEconomy" => "Premium Economy", "business" => "Business", "first" => "First Class", _ => "Economy" };

  public static string NormTripType(string s) { s = s.ToLowerInvariant(); return s.Contains("one") ? "oneWay" : s.Contains("multi") ? "multiCity" : s.Contains("round") ? "roundTrip" : ""; }
  public static string NormCabin(string s) { s = s.ToLowerInvariant(); return s.Contains("premium") ? "premiumEconomy" : s.Contains("business") ? "business" : s.Contains("first") ? "first" : s.Contains("econ") ? "economy" : ""; }

  internal static string S(JsonNode? n) => n is JsonValue v ? (v.TryGetValue<string>(out var s) ? s : v.ToJsonString().Trim('"')).Trim() : "";
  internal static int I(JsonNode? n) => int.TryParse(S(n), NumberStyles.Integer, CultureInfo.InvariantCulture, out var i) ? i : 0;
}

/// <summary>
/// The Python agent re-implemented inside the app, so the sample runs with MAUI + Syncfusion only.
/// It applies the same stage gate as <c>system_prompt.md</c> (search -> results -> confirmation) and renders the
/// bundled <c>designs_maui</c> surfaces. Azure OpenAI credentials are REQUIRED: without them every request fails
/// with <see cref="AiNotConfiguredException"/> and nothing is rendered (there is no offline fallback).
/// </summary>
public sealed class LocalAgent(Func<string, Task<string>> loadDesign, HttpClient http, AzureOpenAiOptions? ai = null) : IAgentClient
{
  private const string HistoryMarker = "\n\n## Conversation history";
  private const string ContextMarker = "Context from the user's earlier inputs";
  private readonly Random _rng = new();
  private readonly SemaphoreSlim _verifyLock = new(1, 1);
  private bool _credentialsVerified;

  private AzureOpenAiOptions Ai => ai is { IsConfigured: true } ? ai : throw new AiNotConfiguredException();
  /// <summary>Bookings made in this app session (newest last). Shown on the booking page with the sample rows.</summary>
  private readonly List<(Trip Trip, JsonObject Flight)> _bookings = [];

  public async Task<AgentReply> SendAsync(string text, string? contextId, CancellationToken ct = default)
  {
    // Fail closed: missing credentials must never run the local booking flow.
    _ = Ai;
    // Wrong credentials are caught here too, but the check runs once (not on every prompt / button click).
    await EnsureCredentialsAsync(ct);

    var ctxId = contextId ?? $"local-{Guid.NewGuid():N}"[..14];
    var h = text.IndexOf(HistoryMarker, StringComparison.Ordinal);
    var request = (h >= 0 ? text[..h] : text).Trim();

    var (messages, say) = TryParseAction(request) is { } action
        ? await HandleActionAsync(action)
        : await HandlePromptAsync(request, text, ct);

    return new AgentReply(messages, say, ctxId);
  }

  private async Task EnsureCredentialsAsync(CancellationToken ct)
  {
    if (_credentialsVerified) return;
    await _verifyLock.WaitAsync(ct);
    try
    {
      if (_credentialsVerified) return;
      // Any 2xx answer proves the endpoint, key and deployment work; a failure throws and is retried next time.
      var ping = new JsonObject
      {
        ["messages"] = new JsonArray(new JsonObject { ["role"] = "user", ["content"] = "Reply with OK." }),
        ["temperature"] = 0,
      };
      await PostChatAsync(ping, ct);
      _credentialsVerified = true;
    }
    finally { _verifyLock.Release(); }
  }

  // ------------------------------------------------------------------ surface actions (button clicks)

  private static JsonObject? TryParseAction(string request)
  {
    var i = request.IndexOf("{\"name\"", StringComparison.Ordinal);
    if (i < 0) return null;
    try { return JsonNode.Parse(request[i..]) is JsonObject o && o["name"] is JsonValue && o["context"] is JsonObject ? o : null; }
    catch (System.Text.Json.JsonException) { return null; }
  }

  private async Task<(JsonArray?, string)> HandleActionAsync(JsonObject action)
  {
    var name = Trip.S(action["name"]);
    var ctx = (JsonObject)action["context"]!;
    var trip = Trip.FromJson(ctx);
    if (trip.TripType == "roundTrip"
    && DateTime.TryParse(trip.DepartureDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dep)
    && (!DateTime.TryParse(trip.ReturnDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var rtn) || rtn <= dep))
      trip.ReturnDate = dep.AddDays(7).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    if (name == "searchFlights")
    {
      var missing = trip.MissingForSearch();
      return missing.Count > 0
          ? (await SearchPageAsync(trip, MissingMessage("To search for flights", missing)), MissingMessage("To search for flights", missing))
          : await ResultsAsync(trip, $"Found flights from {trip.From} to {trip.To} on {trip.DepartureDate}. Tap a row, then press Book now.");
    }

    if (Regex.IsMatch(name, "select|book|proceed", RegexOptions.IgnoreCase))
    {
      var chosen = ctx["selectedFlight"] as JsonObject;
      if (chosen is null)
      {
        const string pick = "Please tap the flight you want in the table first, then press Book now.";
        return trip.MissingForSearch().Count > 0 ? (await SearchPageAsync(trip, pick), pick) : await ResultsAsync(trip, pick);
      }
      var missing = trip.MissingForBooking();
      if (missing.Count > 0)
      {
        var msg = MissingMessage("Before I can confirm your booking", missing) + " Please complete the form and search again.";
        return (await SearchPageAsync(trip, msg), msg);
      }
      return await ConfirmationAsync(trip, chosen);
    }

    return (await SearchPageAsync(trip, "Here is the flight search form."), "Here is the flight search form.");
  }

  private static string MissingMessage(string lead, List<string> missing) =>
      $"{lead} I still need: {string.Join(", ", missing)}.";

  // ------------------------------------------------------------------ free text / stage starters

  private async Task<(JsonArray?, string)> HandlePromptAsync(string request, string fullText, CancellationToken ct)
  {
    var lower = request.ToLowerInvariant();
    var trip = Trip.FromJson(ExtractEarlierInputs(request));

    // Stage starters (the welcome cards) open their page directly, as in the React sample.
    if (lower.StartsWith("show me the flight search form"))
      return (await SearchPageAsync(trip, "Here is the flight search form. Fill in your trip details and press Search Flights."),
              "Here is the flight search form. Fill in your trip details and press Search Flights.");
    if (lower.StartsWith("show me the flight results page"))
      return await ResultsAsync(trip.MissingForSearch().Count == 0 ? trip : SampleTrip(),
          "Here are the available flights. Tap a row, then press Book now.");
    if (lower.Contains("booking and confirmation page") || lower.StartsWith("show the booking confirmation page"))
      return _bookings.Count > 0
          ? await ConfirmationAsync(_bookings[^1].Trip, _bookings[^1].Flight, "Here is your latest booking, together with your earlier bookings.", record: false)
          : await ConfirmationAsync(trip.MissingForBooking().Count == 0 ? trip : SampleTrip(withTraveller: true), SampleFlight(),
              "Here is the booking confirmation page.", record: false);

    // Free text: let the model extract (or, when asked, make up) the form values. The stage gate stays in code.
    var parsed = await AskModelAsync(fullText, ct);
    var intent = Trip.S(parsed?["intent"]);
    var say = Trip.S(parsed?["explanation"]);
    var fromModel = Trip.FromJson(parsed?["fields"] as JsonObject);
    trip.Overlay(fromModel);

    if (intent == "chat" || parsed is null)
      return (null, say.Length > 0 ? say : "How can I help with your trip?");
    if (say.Length == 0) say = "I've filled in what I could. Review the form and press Search Flights.";
    return (await SearchPageAsync(trip, say), say);
  }

  private static JsonObject? ExtractEarlierInputs(string request)
  {
    var i = request.IndexOf(ContextMarker, StringComparison.Ordinal);
    if (i < 0) return null;
    var j = request.IndexOf('{', i);
    if (j < 0) return null;
    try { return JsonNode.Parse(request[j..]) as JsonObject; } catch (System.Text.Json.JsonException) { return null; }
  }

  private async Task<JsonObject?> AskModelAsync(string text, CancellationToken ct)
  {
    var system = """
            You are SkyBook, a flight-booking assistant. Today is @TODAY@.
            Read the user's request (and any earlier inputs / history) and reply with ONE JSON object:
            {"intent":"search_form"|"chat","explanation":"<one or two friendly sentences>","fields":{
              "tripType":"roundTrip|oneWay|multiCity","from":"City (IATA)","to":"City (IATA)",
              "departureDate":"YYYY-MM-DD","returnDate":"YYYY-MM-DD","cabinClass":"economy|premiumEconomy|business|first",
              "adults":1,"children":0,"firstName":"","lastName":"","email":"","passportNumber":"","nationality":""}}
            Rules: use intent "search_form" for anything about flights/booking, "chat" for small talk or questions.
            Put in "fields" ONLY what the user stated or what is in the earlier inputs; leave everything else out.
            EXCEPTION: if the user asks you to fill details "on your own"/"yourself"/"anything", invent plausible values for ALL fields.
            Never claim a booking is confirmed. In "explanation" name what the user still has to fill in.
            """.Replace("@TODAY@", DateTime.Now.ToString("yyyy-MM-dd"));
    var body = new JsonObject
    {
      ["messages"] = new JsonArray(
            new JsonObject { ["role"] = "system", ["content"] = system },
            new JsonObject { ["role"] = "user", ["content"] = text }),
      ["temperature"] = 0,
      ["response_format"] = new JsonObject { ["type"] = "json_object" },
    };
    var content = await PostChatAsync(body, ct);
    if (content is null) return null;
    try { return JsonNode.Parse(content) as JsonObject; }
    catch (System.Text.Json.JsonException) { return null; }   // model answered in plain text: treated as a chat reply
  }

  /// <summary>Sends one chat-completions request to Azure OpenAI / OpenRouter and returns the message content.</summary>
  private async Task<string?> PostChatAsync(JsonObject body, CancellationToken ct)
  {
    var cfg = Ai;
    var apiKey = cfg.ApiKey.Trim();
    var endpoint = cfg.Endpoint.Trim();
    var deployment = cfg.Deployment.Trim();

    string url;
    if (cfg.IsOpenAiCompatible)
    {
      body["model"] = deployment;                           // OpenRouter / OpenAI: model goes in the body
      var baseUrl = endpoint.TrimEnd('/');
      if (baseUrl.Contains("openrouter.ai", StringComparison.OrdinalIgnoreCase) && !baseUrl.EndsWith("/api/v1", StringComparison.OrdinalIgnoreCase))
        baseUrl = "https://openrouter.ai/api/v1";
      url = $"{baseUrl}/chat/completions";
    }
    else
      url = $"{endpoint.TrimEnd('/')}/openai/deployments/{Uri.EscapeDataString(deployment)}/chat/completions?api-version={cfg.ApiVersion}";

    using var req = new HttpRequestMessage(HttpMethod.Post, url) { Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json") };
    if (cfg.IsOpenAiCompatible)
    {
      req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
      req.Headers.Add("X-Title", "SkyBook MAUI");           // optional, shows the app name on OpenRouter
    }
    else
      req.Headers.Add("api-key", apiKey);

    using var res = await http.SendAsync(req, ct);
    var payload = await res.Content.ReadAsStringAsync(ct);
    if (!res.IsSuccessStatusCode)
      throw new AiRequestException((int)res.StatusCode, Truncate(payload, 200));
    return JsonNode.Parse(payload)?["choices"]?[0]?["message"]?["content"]?.GetValue<string>();
  }

  private static string Truncate(string s, int n) => s.Length <= n ? s : s[..n] + "…";

  // ------------------------------------------------------------------ page builders (bundled designs + data)

  private async Task<JsonArray> SearchPageAsync(Trip t, string say)
  {
    var d = await LoadAsync("stage1-flight-search.json");
    // The date pickers show a date even before the user touches them; store the same value so Search sees it.
    if (t.DepartureDate == "") t.DepartureDate = DateTime.Today.AddDays(14).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    if (t.ReturnDate == "") t.ReturnDate = DateTime.Today.AddDays(21).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    SetData(d, "/", new JsonObject
    {
      ["search"] = new JsonObject
      {
        ["tripType"] = t.TripTypeLabel,  // dropdowns select by their display text
        ["from"] = t.From,
        ["to"] = t.To,
        ["departureDate"] = t.DepartureDate,
        ["returnDate"] = t.ReturnDate,
        ["cabinClass"] = t.CabinLabel,
        ["passengers"] = new JsonObject { ["adults"] = Math.Max(1, t.Adults), ["children"] = t.Children },
      },
      ["passenger"] = new JsonObject
      {
        ["firstName"] = t.FirstName,
        ["lastName"] = t.LastName,
        ["email"] = t.Email,
        ["passportNumber"] = t.PassportNumber,
        ["nationality"] = NationalityLabel(t.Nationality),
      },
    });
    // Dropdowns: also give the selected position and text directly on the component
    SelectOption(d, "tripType", t.TripTypeLabel);
    SelectOption(d, "cabinClass", t.CabinLabel);
    SelectOption(d, "nationality", NationalityLabel(t.Nationality));
    return Explain(d, say);
  }

  /// <summary>Sets selectedIndex / selectedItem / text on a combo box so it shows the value even if the
  /// renderer ignores the bound value.</summary>
  private static void SelectOption(JsonArray d, string id, string text)
  {
    foreach (var m in d)
      foreach (var c in (m?["updateComponents"]?["components"] as JsonArray ?? []).OfType<JsonObject>())
      {
        if (Trip.S(c["id"]) != id || c["options"] is not JsonArray opts) continue;
        var i = -1;
        for (var n = 0; n < opts.Count; n++)
        {
          var o = opts[n];
          if (string.Equals(Trip.S(o?["text"]), text, StringComparison.OrdinalIgnoreCase)
              || string.Equals(Trip.S(o?["value"]), text, StringComparison.OrdinalIgnoreCase)) { i = n; break; }
        }
        if (i < 0) continue;
        c["selectedIndex"] = i;
        c["selectedItem"] = opts[i]!.DeepClone();
        c["text"] = Trip.S(opts[i]!["text"]);
      }
  }

  private async Task<(JsonArray, string)> ResultsAsync(Trip t, string say)
  {
    var d = await LoadAsync("stage2-flight-results.json");
    var ret = t.TripType == "roundTrip" && t.ReturnDate != "" ? $" • Return: {t.ReturnDate}" : "";
    SetData(d, "/stage1", new JsonObject
    {
      ["tripType"] = t.TripTypeLabel,
      ["from"] = t.From,
      ["to"] = t.To,
      ["departureDate"] = t.DepartureDate,
      ["returnDate"] = t.ReturnDate,
      ["cabinClass"] = t.CabinLabel,
      ["passengers"] = new JsonObject { ["adults"] = t.Adults, ["children"] = t.Children, ["infants"] = 0 },
      ["preferredAirline"] = "Any",
      ["nonstopOnly"] = false,
    });
    SetData(d, "/summaryText",
        $"{t.TripTypeLabel} • {t.From} → {t.To}\nDeparture: {t.DepartureDate}{ret}\nCabin: {t.CabinLabel} • Adults: {t.Adults} • Children: {t.Children} • Infants: 0\nPreferred Airline: Any • Non-stop Only: No");

    var (fromCode, toCode) = (Iata(t.From), Iata(t.To));
    foreach (var path in new[] { "/allFlights", "/filteredFlights" })
    {
      if (FindData(d, path) is not JsonArray flights) continue;
      foreach (var f in flights.OfType<JsonObject>())
      {
        f["departure"] = Regex.Replace(Trip.S(f["departure"]), "[A-Z]{3}$", fromCode);
        f["arrival"] = Regex.Replace(Trip.S(f["arrival"]), "[A-Z]{3}$", toCode);
      }
    }
    // Push the real trip into the components (the design may have static sample values inlined)
    SetText(d, "summaryText",
        $"{t.TripTypeLabel} • {t.From} → {t.To} • Departure: {t.DepartureDate}{ret} • Cabin: {t.CabinLabel} • Adults: {t.Adults} • Children: {t.Children}");
    SetProp(d, "flightsGrid", "dataSource", FindData(d, "/filteredFlights"));
    return (Explain(d, say), say);
  }

  private Task<(JsonArray, string)> ConfirmationAsync(Trip t, JsonObject chosen) =>
      ConfirmationAsync(t, chosen, $"Your booking is confirmed on {Trip.S(chosen["airline"])} {Trip.S(chosen["flight"])}. Your e-ticket has been issued.", record: true);

  private async Task<(JsonArray, string)> ConfirmationAsync(Trip t, JsonObject chosen, string say, bool record)
  {
    if (record) _bookings.Add((t, (JsonObject)chosen.DeepClone()));
    var d = await LoadAsync("stage3-booking-confirmation.json");
    var code = new string(Enumerable.Range(0, 6).Select(_ => "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"[_rng.Next(32)]).ToArray());
    var pax = $"{t.Adults} Adult{(t.Adults == 1 ? "" : "s")}{(t.Children > 0 ? $", {t.Children} Child{(t.Children == 1 ? "" : "ren")}" : "")}";
    var dates = t.TripType == "roundTrip" && t.ReturnDate != "" ? $"{t.DepartureDate} – {t.ReturnDate}" : t.DepartureDate;
    var arrow = t.TripType == "roundTrip" ? "↔" : "→";
    var (fromCode, toCode) = (Iata(t.From), Iata(t.To));

    SetText(d, "heroSub", $"Confirmation Code: {code} • {t.TripTypeLabel} • {t.From} {arrow} {t.To} • {pax} • {dates}");
    SetText(d, "outboundSub", $"{Trip.S(chosen["airline"])} • Confirmed • {Trip.S(chosen["flight"])}");
    SetText(d, "outboundText",
        $"{Trip.S(chosen["departure"])} → {Trip.S(chosen["arrival"])}\n{Trip.S(chosen["stops"])} • {Trip.S(chosen["duration"])}\nBaggage: {Trip.S(chosen["baggage"])}\nStatus: Ticketed");
    if (t.TripType == "roundTrip")
    {
      SetText(d, "returnSub", $"{Trip.S(chosen["airline"])} • Confirmed");
      SetText(d, "returnText", $"{toCode} → {fromCode} on {t.ReturnDate}\nStatus: Ticketed");
    }
    else
    {
      SetText(d, "returnSub", "One-way booking");
      SetText(d, "returnText", "No return flight on this booking.");
    }
    // Passenger list = this booking + earlier bookings (newest first) + the design's sample rows
    var passengers = new JsonArray();
    var rows = record || _bookings.Count == 0 ? new List<(Trip, JsonObject)> { (t, chosen) } : [];
    rows.AddRange(_bookings.AsEnumerable().Reverse().Where(b => !ReferenceEquals(b.Trip, t)).Select(b => (b.Trip, b.Flight)));
    if (!record && _bookings.Count > 0) rows.Insert(0, (t, chosen));
    foreach (var (bt, bf) in rows)
      passengers.Add(new JsonObject
      {
        ["passenger"] = $"{bt.FirstName} {bt.LastName}".Trim(),
        ["email"] = bt.Email,
        ["passport"] = $"{NationalityLabel(bt.Nationality)} / {bt.PassportNumber}",
        ["seatMeal"] = $"{Trip.S(bf["flight"])} • {bt.CabinLabel}",
      });
    if (FindData(await LoadAsync("stage3-booking-confirmation.json"), "/passengers") is JsonArray sample)
      foreach (var r in sample) passengers.Add(r?.DeepClone());
    SetData(d, "/passengers", passengers);
    SetProp(d, "passengerGrid", "dataSource", passengers);
    SetText(d, "successText",
        $"✓ Booking confirmed successfully! Your e-ticket has been issued and sent to {t.Email}.");
    return (Explain(d, say), say);
  }

  // ------------------------------------------------------------------ helpers

  private async Task<JsonArray> LoadAsync(string file) => JsonNode.Parse(await loadDesign(file))!.AsArray();

  private static JsonArray Explain(JsonArray d, string say)
  {
    d.Add(new JsonObject
    {
      ["version"] = "v0.9",
      ["updateDataModel"] = new JsonObject { ["surfaceId"] = "workspace", ["path"] = "/_explanation", ["value"] = say },
    });
    return DataBeforeComponents(d);
  }

  /// <summary>
  /// Order: createSurface -> updateDataModel -> updateComponents.
  /// The MAUI controls read their bound value when they are created, so the data (prefilled form values,
  /// dates, dropdown selections) must already be in the model at that moment.
  /// </summary>
  private static JsonArray DataBeforeComponents(JsonArray d)
  {
    var msgs = d.Where(m => m is not null).Select(m => m!.DeepClone()).ToList();
    int Rank(JsonNode m) => m["createSurface"] is not null ? 0 : m["updateDataModel"] is not null ? 1 : m["updateComponents"] is not null ? 2 : 3;
    var ordered = msgs.Select((m, i) => (m, i)).OrderBy(x => Rank(x.m)).ThenBy(x => x.i).Select(x => x.m).ToArray();
    return new JsonArray(ordered);
  }

  private static JsonObject? DataMsg(JsonArray d, string path) =>
      d.Select(m => m?["updateDataModel"] as JsonObject).FirstOrDefault(u => u is not null && Trip.S(u["path"]) == path);

  private static JsonNode? FindData(JsonArray d, string path) => DataMsg(d, path)?["value"];

  private static void SetData(JsonArray d, string path, JsonNode value)
  {
    if (DataMsg(d, path) is { } u) u["value"] = value;
    else d.Add(new JsonObject { ["version"] = "v0.9", ["updateDataModel"] = new JsonObject { ["surfaceId"] = "workspace", ["path"] = path, ["value"] = value } });
  }

  private static void SetText(JsonArray d, string componentId, string text)
  {
    foreach (var m in d)
      foreach (var c in (m?["updateComponents"]?["components"] as JsonArray ?? []).OfType<JsonObject>())
        if (Trip.S(c["id"]) == componentId) c["text"] = text;
  }

  private static void SetProp(JsonArray d, string componentId, string prop, JsonNode? value)
  {
    foreach (var m in d)
      foreach (var c in (m?["updateComponents"]?["components"] as JsonArray ?? []).OfType<JsonObject>())
        if (Trip.S(c["id"]) == componentId) c[prop] = value?.DeepClone();
  }

  /// <summary>Maps a nationality code (IN, US, ...) to the dropdown's display text.</summary>
  public static string NationalityLabel(string n) => n.ToUpperInvariant() switch
  {
    "IN" => "India",
    "SG" => "Singapore",
    "US" => "United States",
    "UK" or "GB" => "United Kingdom",
    "UAE" or "AE" => "United Arab Emirates",
    _ => n,
  };

  public static string Iata(string place)
  {
    var m = Regex.Match(place, @"\(([A-Za-z]{3})\)");
    if (m.Success) return m.Groups[1].Value.ToUpperInvariant();
    var letters = new string(place.Where(char.IsLetter).ToArray());
    return letters.Length >= 3 ? letters[..3].ToUpperInvariant() : "---";
  }

  private static Trip SampleTrip(bool withTraveller = false) => new()
  {
    TripType = "roundTrip",
    From = "New York (JFK)",
    To = "London (LHR)",
    DepartureDate = DateTime.Today.AddDays(30).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
    ReturnDate = DateTime.Today.AddDays(39).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
    CabinClass = "business",
    Adults = 2,
    Children = 1,
    FirstName = withTraveller ? "Alexandra" : "",
    LastName = withTraveller ? "Reed" : "",
    Email = withTraveller ? "alexandra.reed@enterpriseair.com" : "",
    PassportNumber = withTraveller ? "XK92PL781" : "",
    Nationality = withTraveller ? "US" : "",
  };

  private static JsonObject SampleFlight() => new()
  {
    ["airline"] = "SkyJet Airways",
    ["flight"] = "SJ 201",
    ["departure"] = "08:15 JFK",
    ["arrival"] = "20:05 LHR",
    ["duration"] = "6h 50m",
    ["stops"] = "Non-stop",
    ["baggage"] = "2 x 23kg",
  };
}
