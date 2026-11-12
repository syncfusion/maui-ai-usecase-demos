using System.Text.Json.Nodes;
using Xunit;

namespace SkyBook.Core.Tests;

public class ActionSummarizerTests
{
    [Fact]
    public void SearchFlights_matches_react_wording()
    {
        var a = JsonNode.Parse("""{"name":"searchFlights","context":{"from":"JFK","to":"LHR","departureDate":"2026-09-15","adults":2,"children":1}}""")!.AsObject();
        Assert.Equal("Search flights from JFK to LHR on 2026-09-15 for 2 adults, 1 child.", ActionSummarizer.Summarize(a));
    }

    [Fact]
    public void SelectFlight_includes_flight_route_price()
    {
        var a = JsonNode.Parse("""{"name":"selectFlight","context":{"flight":"SJ 201","from":"JFK","to":"LHR","price":"$1,840"}}""")!.AsObject();
        Assert.Equal("Book SJ 201 (JFK → LHR) at $1,840.", ActionSummarizer.Summarize(a));
    }

    [Fact]
    public void Unknown_action_is_humanised() =>
        Assert.Equal("Clear Filters.", ActionSummarizer.Summarize(JsonNode.Parse("""{"name":"clearFilters"}""")!.AsObject()));

    [Fact]
    public void Null_action_falls_back() => Assert.Equal("Submitted your selection.", ActionSummarizer.Summarize(null));
}

public class EnvelopeSanitizerTests
{
    [Fact]
    public void Drops_undefined_children_and_empty_containers()
    {
        var msgs = JsonNode.Parse("""
        [{"version":"v0.9","updateComponents":{"surfaceId":"s","components":[
          {"id":"root","component":"Column","children":["row","a"]},
          {"id":"row","component":"Row","children":["ghost"]},
          {"id":"a","component":"Text","text":"hi"}]}}]
        """)!.AsArray();
        var clean = EnvelopeSanitizer.Sanitize(msgs);
        var comps = clean[0]!["updateComponents"]!["components"]!.AsArray();
        Assert.Equal(2, comps.Count);
        Assert.Equal(["a"], comps[0]!["children"]!.AsArray().Select(x => x!.GetValue<string>()));
    }

    [Fact]
    public void Extracts_last_explanation()
    {
        var msgs = JsonNode.Parse("""[{"updateDataModel":{"path":"/_explanation","value":" Hello "}}]""")!.AsArray();
        Assert.Equal("Hello", EnvelopeSanitizer.ExtractExplanation(msgs));
    }

    [Theory]
    [InlineData("workspace", 1)]
    [InlineData("stage2", 2)]
    [InlineData("flight-results", 2)]
    [InlineData("booking_confirmation", 3)]
    public void Step_detection(string id, int step) => Assert.Equal(step, Stage.StepFromSurfaceId(id));

    [Theory]
    [InlineData("stage1-flight-search.json")]
    [InlineData("stage2-flight-results.json")]
    [InlineData("stage3-booking-confirmation.json")]
    public void Agent_designs_survive_sanitising(string file)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "designs", file);
        var msgs = JsonNode.Parse(File.ReadAllText(path))!.AsArray();
        var before = msgs.First(m => m?["updateComponents"] is not null)!["updateComponents"]!["components"]!.AsArray().Count;
        var clean = EnvelopeSanitizer.Sanitize(msgs);
        var comps = clean.First(m => m?["updateComponents"] is not null)!["updateComponents"]!["components"]!.AsArray();
        Assert.Equal(before, comps.Count);
        Assert.Contains(comps, c => c!["id"]!.GetValue<string>() == "root");
    }
}

public class BookingSessionTests
{
    [Fact]
    public void Select_noise_and_busy_are_ignored()
    {
        var s = new BookingSession();
        Assert.Null(s.BuildActionPrompt("select", new JsonObject(), busy: false));
        Assert.Null(s.BuildActionPrompt("searchFlights", new JsonObject(), busy: true));
    }

    [Fact]
    public void Book_action_merges_trip_inputs_and_selected_flight()
    {
        var s = new BookingSession();
        s.BuildActionPrompt("searchFlights", JsonNode.Parse("""{"from":"JFK","to":"LHR","adults":1}""")!.AsObject(), false);
        var p = s.BuildActionPrompt("selectFlightAndProceed",
            JsonNode.Parse("""{"selectedRow":{"flight":"SJ 201"},"results":[1,2],"nextStep":2}""")!.AsObject(), false)!;
        Assert.StartsWith("The user clicked \"Book now\".", p.AgentPayload);
        Assert.Contains("\"selectedFlight\":{\"flight\":\"SJ 201\"}", p.AgentPayload);
        Assert.Contains("\"from\":\"JFK\"", p.AgentPayload);
        Assert.DoesNotContain("\"results\"", p.AgentPayload);
    }

    [Fact]
    public void Outgoing_text_appends_history_excluding_current()
    {
        var s = new BookingSession();
        s.RecordUserPrompt("first"); s.RecordUserPrompt("second");
        Assert.Equal("second\n\n## Conversation history (earlier user requests, oldest first)\nUser: first", s.BuildOutgoingText("second", false));
    }

    [Fact]
    public void Stage3_starter_adds_booking_intent()
    {
        var s = new BookingSession(); s.RecordUserPrompt(Stage.All[2].StarterPrompt);
        Assert.StartsWith("Show the booking confirmation page (stage 3). ", s.BuildOutgoingText(Stage.All[2].StarterPrompt, false));
    }
}

public class AgentClientTests
{
    [Fact]
    public void Parses_a2a_result()
    {
        var env = JsonNode.Parse("""
        {"result":{"contextId":"c1","status":{"message":{"parts":[{"text":"done"}]}},
         "artifacts":[{"parts":[{"data":{"a2uiEnvelope":[{"version":"v0.9","createSurface":{"surfaceId":"workspace"}}]}}]}]}}
        """);
        var r = AgentClient.Parse(env);
        Assert.Equal("c1", r.ContextId);
        Assert.Equal("done", r.AgentText);
        Assert.Single(r.A2uiMessages!);
    }
}

public class AgentClientSendTests
{
    private sealed class Capture(string reply) : HttpMessageHandler
    {
        public string? Body; public Uri? Url; public long? Length;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage r, CancellationToken ct)
        {
            Url = r.RequestUri; Length = r.Content!.Headers.ContentLength; Body = await r.Content!.ReadAsStringAsync(ct);
            return new HttpResponseMessage { Content = new StringContent(reply) };
        }
    }

    [Fact]
    public async Task Posts_jsonrpc_message_send_with_context_id()
    {
        var h = new Capture("""{"result":{"contextId":"c9","artifacts":[{"parts":[{"data":{"a2uiEnvelope":[{"version":"v0.9"}]}}]}]}}""");
        var reply = await new AgentClient(new HttpClient(h), AgentClient.DefaultUrl).SendAsync("hello", "c1");
        var body = System.Text.Json.Nodes.JsonNode.Parse(h.Body!)!;
        Assert.Equal("http://localhost:10006/", h.Url!.ToString());
        Assert.NotNull(h.Length); // fixed Content-Length, not chunked
        Assert.Equal("2.0", (string)body["jsonrpc"]!);
        Assert.Equal("message/send", (string)body["method"]!);
        Assert.Equal("hello", (string)body["params"]!["message"]!["parts"]![0]!["text"]!);
        Assert.Equal("user", (string)body["params"]!["message"]!["role"]!);
        Assert.Equal("c1", (string)body["params"]!["message"]!["contextId"]!);
        Assert.Equal("c9", reply.ContextId);
        Assert.Single(reply.A2uiMessages!);
    }

    [Fact]
    public async Task Omits_context_id_on_first_message()
    {
        var h = new Capture("{}");
        await new AgentClient(new HttpClient(h), AgentClient.DefaultUrl).SendAsync("hi", null);
        Assert.Null(System.Text.Json.Nodes.JsonNode.Parse(h.Body!)!["params"]!["message"]!["contextId"]);
    }
}

public class ContextResolverTests
{
    [Fact]
    public void Resolves_path_bindings_and_passes_literals()
    {
        var model = new Dictionary<string, object?> { ["/search/from"] = "JFK", ["/search/adults"] = 2 };
        var ctx = new Dictionary<string, object?>
        {
            ["from"] = JsonNode.Parse("""{"path":"/search/from"}"""),
            ["adults"] = System.Text.Json.JsonDocument.Parse("""{"path":"/search/adults"}""").RootElement.Clone(),
            ["nextStep"] = 2,
        };
        var r = ContextResolver.Resolve(ctx, p => model.GetValueOrDefault(p));
        Assert.Equal("JFK", r["from"]!.GetValue<string>());
        Assert.Equal(2, r["adults"]!.GetValue<int>());
        Assert.Equal(2, r["nextStep"]!.GetValue<int>());
    }

    [Fact]
    public void SelectedRow_json_string_becomes_object()
    {
        var ctx = new Dictionary<string, object?> { ["selectedRow"] = JsonNode.Parse("""{"path":"/selectedFlight"}""") };
        var r = ContextResolver.Resolve(ctx, _ => """{"flight":"SJ 201","price":1840}""");
        Assert.Equal("SJ 201", r["selectedRow"]!["flight"]!.GetValue<string>());
    }

    [Fact]
    public void Date_fields_are_normalised_to_iso()
    {
        var ctx = new Dictionary<string, object?> { ["departureDate"] = "2026-09-15", ["returnDate"] = "2026-09-24T00:00:00", ["name"] = "x" };
        var r = ContextResolver.Resolve(ctx, _ => null);
        Assert.Equal("2026-09-15", r["departureDate"]!.GetValue<string>());
        Assert.Equal("2026-09-24", r["returnDate"]!.GetValue<string>());
        Assert.Equal("x", r["name"]!.GetValue<string>());
    }

    [Fact]
    public void Missing_binding_resolves_to_null()
    {
        var ctx = new Dictionary<string, object?> { ["selectedRow"] = JsonNode.Parse("""{"path":"/selectedFlight"}""") };
        Assert.Null(ContextResolver.Resolve(ctx, _ => null)["selectedRow"]);
    }

    [Theory]
    [InlineData("workspace", new[] { "root" }, 1)]
    [InlineData("workspace", new[] { "root", "flightsGrid" }, 2)]
    [InlineData("workspace", new[] { "root", "passengerGrid" }, 3)]
    [InlineData("stage3", new[] { "root" }, 3)]
    public void InferStep_falls_back_to_components(string id, string[] comps, int step) =>
        Assert.Equal(step, Stage.InferStep(id, comps));
}
