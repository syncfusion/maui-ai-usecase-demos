using Syncfusion.A2UI.Core.BasicCatalog;
using Syncfusion.Maui.A2UI.Catalog;
using Syncfusion.Maui.A2UI.Hosting;
using Syncfusion.Maui.A2UI.Markdown;
using Syncfusion.Maui.A2UI.SyncfusionComponents;

namespace SkyBook.Maui.Ui;

/// <summary>Builds the A2UI <see cref="SurfaceHost"/> with the stock Syncfusion catalog plus SkyBook's layout/grid refinements.</summary>
public static class SkyHost
{
    /// <summary>Raised when the "Book now" button of a flight row is pressed (row = the grid row's fields).</summary>
    public static event Action<IDictionary<string, object>>? BookRequested;

  public static SurfaceHost Create(string catalogId)
  {
    var catalog = SyncfusionComponentRegistry.CombineCatalog(catalogId, [new SkyAppBarSchema()], BasicFunctions.All);
    var renderers = new Dictionary<string, IA2uiComponentRenderer>(SyncfusionComponentRegistry.CombinedRenderers(), StringComparer.Ordinal);

    A2uiFlightGridRenderer.BookRequested = row => BookRequested?.Invoke(row);

    foreach (var r in new IA2uiComponentRenderer[]
    {
        new SkyColumnRenderer(), new SkyRowRenderer(), new SkyTextRenderer(), new SkyAppBarRenderer(),
        new A2uiFlightGridRenderer(),
        new SkyCardRenderer(),
        new ColoredTextRenderer(new SkyTextRenderer()),   // ← add this line
        new A2uiScrollViewRenderer(),
    })
      renderers[r.ComponentName] = r;

    return new SurfaceHost([catalog], renderers, new BasicMarkdownRenderer());
  }
}
