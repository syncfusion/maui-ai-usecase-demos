using Xunit;
using System.Text.Json;
using Syncfusion.A2UI.Core.BasicCatalog;
using Syncfusion.Maui.A2UI.Hosting;
using Syncfusion.Maui.A2UI.Markdown;
using Syncfusion.Maui.A2UI.SyncfusionComponents;

namespace SkyBook.Designs.Tests;

public class DesignTests
{
    private const string CatalogId = SyncfusionA2uiAppBuilderExtensions.DefaultCatalogId;

    private static SurfaceHost NewHost() => new(
        [SyncfusionComponentRegistry.CombineCatalog(CatalogId, null, BasicFunctions.All)],
        SyncfusionComponentRegistry.CombinedRenderers(), new BasicMarkdownRenderer());

    public static TheoryData<string> Files => new(
        Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "designs_maui"), "*.json").Select(Path.GetFileName)!);

    [Fact]
    public void All_three_stages_present() => Assert.Equal(3, Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "designs_maui"), "*.json").Length);

    [Theory, MemberData(nameof(Files))]
    public void Design_loads_into_surface_host(string file)
    {
        var json = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "designs_maui", file));
        Assert.Equal(CatalogId, JsonDocument.Parse(json).RootElement[0].GetProperty("createSurface").GetProperty("catalogId").GetString());

        using var host = NewHost();
        host.ProcessJson(json);

        var surface = Assert.Single(host.Surfaces).Value;
        Assert.Equal("workspace", surface.Id);
        Assert.True(surface.ComponentsModel.Count > 0);
        Assert.NotNull(surface.ComponentsModel["root"]);
    }

    [Fact]
    public async Task Search_button_dispatches_searchFlights()
    {
        using var host = NewHost();
        host.ProcessJson(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "designs_maui", "stage1-flight-search.json")));
        var surface = host.Surfaces["workspace"];
        var onClick = surface.ComponentsModel["searchBtn"]!.Properties["onClick"];
        var ctx = new Syncfusion.A2UI.Core.Rendering.ComponentContext(surface, "searchBtn", "/");
        await ctx.DispatchActionAsync(onClick);
        var action = Assert.Single(host.ConsumeActions());
        Assert.Equal("searchFlights", action.Name);
    }

    [Fact]
    public void Results_expose_flights_and_book_button()
    {
        using var host = NewHost();
        host.ProcessJson(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "designs_maui", "stage2-flight-results.json")));
        var surface = host.Surfaces["workspace"];
        Assert.NotNull(surface.ComponentsModel["flightsGrid"]);
        Assert.NotNull(surface.DataModel.Get("/filteredFlights"));
    }
}
