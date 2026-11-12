using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using SkyBook.Core;
using SkyBook.Maui.Ui;
using Syncfusion.Maui.A2UI.Hosting;
using Syncfusion.Maui.DataGrid;
using Xunit;

namespace SkyBook.Designs.Tests;

/// <summary>Builds the real MAUI view tree for each page (no device) and checks the structure the screenshots expect.</summary>
public class UiRenderTests
{
    private sealed class Disp : IDispatcher
    {
        public bool IsDispatchRequired => false;
        public IDispatcherTimer CreateTimer() => new Timer();
        public bool Dispatch(Action a) { a(); return true; }
        public bool DispatchDelayed(TimeSpan t, Action a) { a(); return true; }
    }
    private sealed class Timer : IDispatcherTimer
    {
        public TimeSpan Interval { get; set; } public bool IsRepeating { get; set; } public bool IsRunning => false;
        public event EventHandler? Tick { add { } remove { } } public void Start() { } public void Stop() { }
    }
    private sealed class Prov : IDispatcherProvider { public IDispatcher? GetForCurrentThread() => new Disp(); }

    static UiRenderTests() => DispatcherProvider.SetCurrent(new Prov());

    private static IEnumerable<Element> Walk(Element e)
    {
        yield return e;
        var kids = e switch
        {
            Layout l => l.Children.OfType<Element>(),
            ContentView c when c.Content is not null => [c.Content],
            Border b when b.Content is not null => [b.Content],
            _ => e.GetType().GetProperty("Content")?.GetValue(e) is Element x ? [x] : Enumerable.Empty<Element>(),
        };
        foreach (var k in kids) foreach (var d in Walk(k)) yield return d;
    }

    private static async Task<List<Element>> Page(string request)
    {
        var agent = new LocalAgent(f => File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "designs_maui", f)), new HttpClient());
        var r = await agent.SendAsync(request, null);
        var host = SkyHost.Create("syncfusion-maui");
        host.ProcessJson(EnvelopeSanitizer.Sanitize(r.A2uiMessages!).ToJsonString());
        var view = new A2uiSurface { Host = host, Surface = host.Surfaces["workspace"] };
        return Walk(view).ToList();
    }

    [Fact]
    public async Task Search_page_lays_fields_out_in_rows()
    {
        var all = await Page("Show me the flight search form.");
        var grids = all.OfType<Grid>().Where(g => g.Children.Count == 3).ToList();      // trip type / from / to, dates / cabin, names
        Assert.True(grids.Count >= 2);
        Assert.All(grids, g => Assert.Equal(3, g.ColumnDefinitions.Count));
        Assert.Contains(all, e => e.GetType().Name == "SfButton");
    }

    [Fact]
    public async Task Results_page_has_title_bar_cards_and_book_button_per_row()
    {
        var req = """Search {"name":"searchFlights","context":{"tripType":"roundTrip","from":"San Francisco (SFO)","to":"Tokyo (HND)","departureDate":"2026-10-05","returnDate":"2026-10-15","cabinClass":"business","adults":"1","children":"0","firstName":"H","lastName":"T","email":"h@t.jp","passportNumber":"TK1","nationality":"IN"}}""";
        var all = await Page(req);

        Assert.Contains(all.OfType<Label>(), l => l.Text == "Flight Results");      // coloured app bar
        Assert.Contains(all.OfType<Border>(), b => b.Content is Label { Text: "Flight Results" });
        Assert.Contains(all.OfType<Label>(), l => l.Text == "Trip Summary");
        Assert.Contains(all.OfType<Label>(), l => l.Text == "Available Flights");

        var grid = all.OfType<SfDataGrid>().Single();
        var headers = grid.Columns.Select(c => c.HeaderText).ToList();
        Assert.Equal(["Airline", "Flight", "Departure", "Arrival", "Duration", "Stops", "Price", "Book now"], headers);
        Assert.IsType<DataGridTemplateColumn>(grid.Columns[^1]);                     // real button column
        Assert.True(grid.HeightRequest > 0);                                         // sized to rows, page scrolls
    }

    [Fact]
    public async Task Confirmation_page_shows_cards_and_passenger_table()
    {
        var req = """Book {"name":"selectFlightAndProceed","context":{"tripType":"roundTrip","from":"San Francisco (SFO)","to":"Tokyo (HND)","departureDate":"2026-10-05","returnDate":"2026-10-15","cabinClass":"business","adults":"1","children":"0","firstName":"Hiroshi","lastName":"Tanaka","email":"h.tanaka@example.jp","passportNumber":"TK9988776","nationality":"IN","selectedFlight":{"airline":"Japan Air","flight":"JL 001","departure":"11:30 SFO","arrival":"15:00 HND","duration":"11h 30m","stops":"Non-stop","baggage":"2 x 23kg","price":3200}}}""";
        var all = await Page(req);
        var texts = all.OfType<Label>().Select(l => l.Text).ToList();
        Assert.Contains("Booking Details", texts);
        Assert.Contains("Outbound", texts);
        Assert.Contains("Return", texts);
        Assert.Contains("Passenger Information", texts);
        Assert.Contains(texts, t => t.StartsWith("Confirmation Code:"));
        var grid = all.OfType<SfDataGrid>().Single();
        Assert.Equal(["Passenger", "Email", "Nationality/Passport", "Seat/Meal"], grid.Columns.Select(c => c.HeaderText));
        Assert.Equal(2, all.OfType<Grid>().First(g => g.Children.Count == 2).ColumnDefinitions.Count);  // Outbound | Return side by side
    }
}
