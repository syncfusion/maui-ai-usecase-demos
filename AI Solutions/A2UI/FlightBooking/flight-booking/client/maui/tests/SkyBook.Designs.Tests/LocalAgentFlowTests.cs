using SkyBook.Core;
using Syncfusion.A2UI.Core.BasicCatalog;
using Syncfusion.Maui.A2UI.Hosting;
using Syncfusion.Maui.A2UI.Markdown;
using Syncfusion.Maui.A2UI.SyncfusionComponents;
using Xunit;

namespace SkyBook.Designs.Tests;

/// <summary>Drives the in-app agent through search -> results -> confirmation and renders each reply in the real SurfaceHost.</summary>
public class LocalAgentFlowTests
{
    private static SurfaceHost NewHost() => new(
        [SyncfusionComponentRegistry.CombineCatalog(SyncfusionA2uiAppBuilderExtensions.DefaultCatalogId, null, BasicFunctions.All)],
        SyncfusionComponentRegistry.CombinedRenderers(), new BasicMarkdownRenderer());

    private static void Render(SurfaceHost host, AgentReply r)
    {
        foreach (var id in host.Surfaces.Keys.ToList())
            host.ProcessJson($"[{{\"version\":\"v0.9\",\"deleteSurface\":{{\"surfaceId\":\"{id}\"}}}}]");
        host.ProcessJson(EnvelopeSanitizer.Sanitize(r.A2uiMessages!).ToJsonString());
    }

    [Fact]
    public async Task Full_flow_renders_each_stage()
    {
        var agent = new LocalAgent(f => File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "designs_maui", f)), new HttpClient());
        var host = NewHost();

        var r = await agent.SendAsync("Show me the flight search form.", null);
        Render(host, r);
        Assert.Equal(1, Stage.InferStep("workspace", host.Surfaces["workspace"].ComponentsModel.Keys));

        r = await agent.SendAsync("""{"name":"searchFlights","context":{"tripType":"oneWay","from":"Chennai (MAA)","to":"Bangalore (BLR)","departureDate":"2026-10-01","cabinClass":"economy","adults":"1","children":"0","firstName":"A","lastName":"B","email":"a@b.c","passportNumber":"P1","nationality":"IN"}}""", r.ContextId);
        Render(host, r);
        Assert.Equal(2, Stage.InferStep("workspace", host.Surfaces["workspace"].ComponentsModel.Keys));

        r = await agent.SendAsync("""Book {"name":"selectFlightAndProceed","context":{"tripType":"oneWay","from":"Chennai (MAA)","to":"Bangalore (BLR)","departureDate":"2026-10-01","cabinClass":"economy","adults":"1","children":"0","firstName":"A","lastName":"B","email":"a@b.c","passportNumber":"P1","nationality":"IN","selectedFlight":{"airline":"SkyJet Airways","flight":"SJ 201","departure":"08:15 MAA","arrival":"10:00 BLR","stops":"Non-stop","duration":"1h","baggage":"1 x 23kg"}}}""", r.ContextId);
        Render(host, r);
        Assert.Equal(3, Stage.InferStep("workspace", host.Surfaces["workspace"].ComponentsModel.Keys));
    }
}
