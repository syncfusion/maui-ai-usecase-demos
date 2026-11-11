using Microsoft.Maui.Controls.Shapes;
using Syncfusion.A2UI.Core.BasicCatalog;
using Syncfusion.A2UI.Core.Rendering;
using Syncfusion.Maui.A2UI.Catalog;

namespace SkyBook.Maui.Ui;

/// <summary>
/// Replaces the catalog's "SyncfusionCardView" with a plain bordered card:
/// white background, 1px #E2E8F0 border, rounded corners, no tint, no shadow, no clipping.
/// Cards placed in the same Row stretch to the same height.
/// The card's content comes from "children" (or "content" via a children alias set in the JSON).
/// </summary>
public sealed class SkyCardRenderer : IA2uiComponentRenderer
{
  public string ComponentName => "SyncfusionCardView";

  public View CreateView(IReadOnlyDictionary<string, object?> props, IBuildChild buildChild, ComponentContext context)
  {
    var radius = Num(props, "cornerRadius", 12);
    var border = new Border
    {
      BackgroundColor = Colors.White,
      Stroke = Color.FromArgb(Str(props, "borderColor") ?? "#E2E8F0"),
      StrokeThickness = Num(props, "borderWidth", 1),
      StrokeShape = new RoundRectangle { CornerRadius = radius },
      Padding = 0,
      HorizontalOptions = LayoutOptions.Fill,
      VerticalOptions = LayoutOptions.Fill, // equal height inside a Row
    };

    // The card schema only keeps "content" (a child id); reuse Kids.Build by handing it that id as "children".
    var children = Kids.Build(props, buildChild);
    if (children.Count == 0 && props.TryGetValue("content", out var content) && content is not null)
    {
      var id = content is System.Text.Json.JsonElement je && je.ValueKind == System.Text.Json.JsonValueKind.String
          ? je.GetString()! : Convert.ToString(content)!.Trim('"');
      foreach (object list in new object[]
      {
                new List<object?> { content },   // the resolver's own node reference — try this first
                new object[] { content },
                new[] { id },
                System.Text.Json.JsonDocument.Parse($"[\"{id}\"]").RootElement,
      })
      {
        children = Kids.Build(new Dictionary<string, object?>(props) { ["children"] = list }, buildChild);
        // Skip the resolver's error placeholder ("Unresolved child reference ...") and try the next form
        var failed = children.Any(v => v is Label l && l.Text?.StartsWith("Unresolved", StringComparison.Ordinal) == true);
        if (children.Count > 0 && !failed) break;
      }
    }
    if (children.Count == 1) border.Content = children[0];
    else if (children.Count > 1)
    {
      var stack = new VerticalStackLayout();
      foreach (var c in children) stack.Add(c);
      border.Content = stack;
    }
    // Equal heights: cards that share a parent (e.g. Outbound + Return in one Row) grow to the tallest one.
    border.StyleId = "SkyCard";
    border.SizeChanged += (_, _) => EqualizeSiblings(border);
    return border;
  }

  private static double AbsY(VisualElement v)
  {
    double y = 0;
    for (Element? e = v; e is VisualElement ve; e = e.Parent) y += ve.Y;
    return y;
  }

  private static void EqualizeSiblings(Border card)
  {
    // The Row renderer may wrap each child in its own cell, so look one level up as well.
    var row = card.Parent is Border or ContentView ? card.Parent?.Parent : card.Parent;
    if (row is not IVisualTreeElement tree) return;
    var cards = tree.GetVisualTreeDescendants().OfType<Border>().Where(b => b.StyleId == "SkyCard" && b.Height > 0).ToList();
    if (cards.Count < 2) return;
    // Only side-by-side cards (same top) are equalised; stacked cards on a phone keep their own height.
    if (cards.Select(b => Math.Round(AbsY(b))).Distinct().Count() > 1)
    {
      foreach (var b in cards) if (b.MinimumHeightRequest > 0) b.MinimumHeightRequest = -1;
      return;
    }
    var max = cards.Max(b => Math.Max(b.Height, b.MinimumHeightRequest));
    foreach (var b in cards)
      if (Math.Abs(b.MinimumHeightRequest - max) > 0.5 && b.Height < max - 0.5)
        b.MinimumHeightRequest = max;
  }

  private static double Num(IReadOnlyDictionary<string, object?> p, string k, double d) =>
      p.TryGetValue(k, out var v) && double.TryParse(Convert.ToString(v, System.Globalization.CultureInfo.InvariantCulture),
          System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var x) ? x : d;

  private static string? Str(IReadOnlyDictionary<string, object?> p, string k) =>
      p.TryGetValue(k, out var v) ? Convert.ToString(v)?.Trim('"') : null;
}
