using Syncfusion.A2UI.Core.Rendering;
using Syncfusion.Maui.A2UI.Catalog;

namespace SkyBook.Maui.Ui;

/// <summary>
/// Wraps the existing "Text" renderer (SkyTextRenderer) and adds colour:
///  - the booking success line ("✓ Booking confirmed…") is shown in bold green;
///  - any Text with a "textColor" property (if the catalog passes it through) uses that colour.
/// "Text" is a catalog component, so no new schema is needed (a new component name shows "[Loading …]").
/// </summary>
public sealed class ColoredTextRenderer(IA2uiComponentRenderer inner) : IA2uiComponentRenderer
{
  public string ComponentName => inner.ComponentName; // "Text"

  public View CreateView(IReadOnlyDictionary<string, object?> props, IBuildChild buildChild, ComponentContext context)
  {
    var view = inner.CreateView(props, buildChild, context);
    var label = view as Label ?? (view as IVisualTreeElement)?.GetVisualTreeDescendants().OfType<Label>().FirstOrDefault();
    if (label is null) return view;

    if (props.TryGetValue("textColor", out var c) && Convert.ToString(c)?.Trim('"') is { Length: > 0 } hex)
    {
      try { label.TextColor = Color.FromArgb(hex); } catch (ArgumentException) { }
    }

    void Apply()
    {
      if (label.Text?.StartsWith("✓ Booking confirmed", StringComparison.Ordinal) == true)
      {
        label.TextColor = Color.FromArgb("#15803D");
        label.FontAttributes = FontAttributes.Bold;
      }
    }
    Apply();
    label.PropertyChanged += (_, e) => { if (e.PropertyName == Label.TextProperty.PropertyName) Apply(); };
    return view;
  }
}
