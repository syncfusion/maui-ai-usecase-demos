using System.Net;
using System.Text.Json.Nodes;
using SkyBook.Core;
using Xunit;

namespace SkyBook.Core.Tests;

public class LocalAgentTests
{
    private static readonly string Designs = Path.Combine(AppContext.BaseDirectory, "designs");
    private static Task<string> Load(string f) => File.ReadAllTextAsync(Path.Combine(Designs, f));

    private sealed class FakeAzure(string content) : HttpMessageHandler
    {
        public string? Url, Key;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage r, CancellationToken ct)
        {
            Url = r.RequestUri!.ToString(); Key = r.Headers.GetValues("api-key").Single();
            var body = new JsonObject { ["choices"] = new JsonArray(new JsonObject { ["message"] = new JsonObject { ["content"] = content } }) };
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body.ToJsonString()) });
        }
    }

    private static LocalAgent Agent(HttpMessageHandler? h = null, bool ai = false) =>
        new(Load, new HttpClient(h ?? new FakeAzure("{}")), ai ? new AzureOpenAiOptions("https://x.openai.azure.com/", "k", "gpt-4o") : null);

    private static string Data(JsonArray m, string path) =>
        m.Select(x => x?["updateDataModel"]).First(u => u is not null && (string?)u["path"] == path)!["value"]!.ToJsonString();

    private const string FullSearch = """{"name":"searchFlights","context":{"tripType":"roundTrip","from":"Chennai (MAA)","to":"Bangalore (BLR)","departureDate":"2026-10-01","returnDate":"2026-10-05","cabinClass":"economy","adults":"1","children":"0","firstName":"A","lastName":"B","email":"a@b.c","passportNumber":"P1","nationality":"IN"}}""";

    [Fact]
    public async Task Search_starter_opens_search_page()
    {
        var r = await Agent().SendAsync("Show me the flight search form.", null);
        Assert.True(EnvelopeSanitizer.HasCreateSurface(r.A2uiMessages!));
        Assert.Contains("searchBtn", r.A2uiMessages!.ToJsonString());
        Assert.NotNull(r.ContextId);
    }

    [Fact]
    public async Task Results_starter_opens_results_page() =>
        Assert.Contains("flightsGrid", (await Agent().SendAsync("Show me the flight results page.", null)).A2uiMessages!.ToJsonString());

    [Fact]
    public async Task Confirmation_starter_opens_confirmation_page() =>
        Assert.Contains("passengerGrid", (await Agent().SendAsync("Show me the booking and Confirmation page.", null)).A2uiMessages!.ToJsonString());

    [Fact]
    public async Task Complete_search_action_goes_to_results_with_real_inputs()
    {
        var r = await Agent().SendAsync(FullSearch, null);
        Assert.Contains("flightsGrid", r.A2uiMessages!.ToJsonString());
        Assert.Contains("Chennai (MAA)", Data(r.A2uiMessages!, "/summaryText"));
        Assert.Contains("Bangalore (BLR)", Data(r.A2uiMessages!, "/summaryText"));
        Assert.Contains("MAA", Data(r.A2uiMessages!, "/filteredFlights"));
        Assert.Contains("BLR", Data(r.A2uiMessages!, "/filteredFlights"));
    }

    [Fact]
    public async Task Incomplete_search_action_stays_on_search_and_names_missing_fields()
    {
        var r = await Agent().SendAsync("""{"name":"searchFlights","context":{"from":"Chennai","adults":"1"}}""", null);
        Assert.Contains("searchBtn", r.A2uiMessages!.ToJsonString());
        Assert.Contains("destination", r.AgentText);
        Assert.Contains("Chennai", Data(r.A2uiMessages!, "/"));
    }

    [Fact]
    public async Task Book_without_chosen_flight_stays_on_results()
    {
        var r = await Agent().SendAsync("""The user clicked "Book now". {"name":"selectFlightAndProceed","context":{"tripType":"roundTrip","from":"A (AAA)","to":"B (BBB)","departureDate":"2026-10-01","returnDate":"2026-10-05","cabinClass":"economy","adults":"1"}}""", null);
        Assert.Contains("flightsGrid", r.A2uiMessages!.ToJsonString());
        Assert.Contains("tap the flight", r.AgentText);
    }

    [Fact]
    public async Task Book_with_flight_and_traveller_confirms_with_exact_inputs()
    {
        var ctx = JsonNode.Parse(FullSearch)!["context"]!.AsObject().DeepClone().AsObject();
        ctx["selectedFlight"] = new JsonObject { ["airline"] = "SkyJet Airways", ["flight"] = "SJ 201", ["departure"] = "08:15 MAA", ["arrival"] = "10:00 BLR", ["stops"] = "Non-stop", ["duration"] = "1h", ["baggage"] = "1 x 23kg" };
        var action = new JsonObject { ["name"] = "selectFlightAndProceed", ["context"] = ctx };
        var r = await Agent().SendAsync($"The user clicked \"Book now\". {action.ToJsonString()}", null);
        Assert.Contains("passengerGrid", r.A2uiMessages!.ToJsonString());
        Assert.Contains("A B", Data(r.A2uiMessages!, "/passengers"));
        Assert.Contains("SJ 201", r.A2uiMessages!.ToJsonString());
        Assert.Contains("confirmed", r.AgentText);
    }

    [Fact]
    public async Task Free_text_without_azure_explains_and_shows_search_form()
    {
        var r = await Agent().SendAsync("find me a flight", null);
        Assert.Contains("Azure OpenAI", r.AgentText);
        Assert.Contains("searchBtn", r.A2uiMessages!.ToJsonString());
    }

    [Fact]
    public async Task Free_text_with_azure_prefills_the_form_and_calls_azure_correctly()
    {
        var fake = new FakeAzure("""{"intent":"search_form","explanation":"Filled Chennai to Bangalore.","fields":{"from":"Chennai (MAA)","to":"Bangalore (BLR)","adults":1}}""");
        var r = await Agent(fake, ai: true).SendAsync("flight chennai to bangalore", null);
        Assert.Equal("https://x.openai.azure.com/openai/deployments/gpt-4o/chat/completions?api-version=2024-10-21", fake.Url);
        Assert.Equal("k", fake.Key);
        Assert.Equal("Filled Chennai to Bangalore.", r.AgentText);
        Assert.Contains("Chennai (MAA)", Data(r.A2uiMessages!, "/"));
    }

    [Fact]
    public async Task Chat_intent_returns_text_only()
    {
        var r = await Agent(new FakeAzure("""{"intent":"chat","explanation":"Hello!","fields":{}}"""), ai: true).SendAsync("hi", null);
        Assert.Null(r.A2uiMessages);
        Assert.Equal("Hello!", r.AgentText);
    }
}
