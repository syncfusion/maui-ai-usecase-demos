using Syncfusion.A2UI.Core.Catalog;
using Syncfusion.A2UI.Core.Rendering;
using Syncfusion.Maui.A2UI.Catalog;
using Syncfusion.Maui.A2UI.Renderers;

namespace SkyBook.Maui.Ui;

/// <summary>
/// The stock Column ignores <c>gap</c>/<c>padding</c> (they only apply to its FlexLayout Row), which made every page cramped.
/// </summary>
public sealed class SkyColumnRenderer : IA2uiComponentRenderer
{
    public string ComponentName => "Column";

    public View CreateView(IReadOnlyDictionary<string, object?> props, IBuildChild buildChild, ComponentContext context)
    {
        var col = new VerticalStackLayout
        {
            Spacing = Kids.Num(props, "gap"),
            Padding = new Thickness(Kids.Num(props, "padding")),
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Start,
        };
        foreach (var kid in Kids.Build(props, buildChild)) col.Add(kid);
        return col;
    }
}

/// <summary>
/// Row as an equal/weighted column grid (React renders these as CSS grid rows: three fields per line, two cards side by side).
/// Collapses to a single column on narrow widths.
/// </summary>
public sealed class SkyRowRenderer : IA2uiComponentRenderer
{
    public const double StackBelow = 560;
    public string ComponentName => "Row";

    public View CreateView(IReadOnlyDictionary<string, object?> props, IBuildChild buildChild, ComponentContext context)
    {
        var gap = Kids.Num(props, "gap");
        var grid = new Grid
        {
            ColumnSpacing = gap, RowSpacing = gap,
            Padding = new Thickness(Kids.Num(props, "padding")),
            HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Start,
        };
        var kids = Kids.Build(props, buildChild);
        // Every field shares the row width (the stock inputs report no weight, which left each column content-sized).
        var weights = kids.Select(k => FlexLayout.GetGrow(k) > 0 ? (double)FlexLayout.GetGrow(k) : 1d).ToList();
        foreach (var k in kids) { k.VerticalOptions = LayoutOptions.Start; k.HorizontalOptions = LayoutOptions.Fill; grid.Add(k); }

        bool? stacked = null;
        void Arrange(bool stack)
        {
            if (stacked == stack) return;
            stacked = stack;
            grid.ColumnDefinitions.Clear(); grid.RowDefinitions.Clear();
            for (var i = 0; i < kids.Count; i++)
            {
                if (stack)
                {
                    grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                    Grid.SetRow(kids[i], i); Grid.SetColumn(kids[i], 0);
                }
                else
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(weights[i], GridUnitType.Star)));
                    Grid.SetRow(kids[i], 0); Grid.SetColumn(kids[i], i);
                }
            }
            if (stack) grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        }
        Arrange(false);
        grid.SizeChanged += (_, _) => { if (grid.Width > 0) Arrange(kids.Count > 1 && grid.Width < StackBelow); };
        return grid;
    }
}

/// <summary>Stock Text renderer + the React type scale/colours (stock caption is italic, h2 is oversized).</summary>
public sealed class SkyTextRenderer : IA2uiComponentRenderer
{
    private readonly TextRenderer _inner = new();
    public string ComponentName => "Text";

    public View CreateView(IReadOnlyDictionary<string, object?> props, IBuildChild buildChild, ComponentContext context)
    {
        var view = _inner.CreateView(props, buildChild, context);
        if (view is not Label l) return view;
        switch (Kids.Prop(props, "variant"))
        {
            case "h1": l.FontSize = 26; l.TextColor = Color.FromArgb("#0f172a"); break;
            case "h2": l.FontSize = 20; l.TextColor = Color.FromArgb("#0f172a"); break;
            case "h3": l.FontSize = 17; l.TextColor = Color.FromArgb("#0f172a"); break;
            case "h4": l.FontSize = 15; l.TextColor = Color.FromArgb("#0f172a"); break;
            case "caption":
                l.FontSize = 13; l.FontAttributes = FontAttributes.None; l.TextColor = Color.FromArgb("#64748b"); break;
            default:
                l.FontSize = 14; l.TextColor = Color.FromArgb("#334155"); break;
        }
        l.LineBreakMode = LineBreakMode.WordWrap;
        return l;
    }
}

/// <summary>Schema for the coloured page title bar (React: SyncfusionAppBar).</summary>
public sealed class SkyAppBarSchema : DeclaredComponentSchema
{
    public SkyAppBarSchema() : base("SkyAppBar",
    [
        FieldInfoBuilder.StaticField("accessibility"),
        FieldInfoBuilder.StaticField("weight"),
        FieldInfoBuilder.StaticField("title"),
        FieldInfoBuilder.StaticField("background"),
        FieldInfoBuilder.StaticField("textColor"),
    ]) { }
}

public sealed class SkyAppBarRenderer : IA2uiComponentRenderer
{
    public string ComponentName => "SkyAppBar";

    public View CreateView(IReadOnlyDictionary<string, object?> props, IBuildChild buildChild, ComponentContext context) =>
        new Border
        {
            StrokeThickness = 0,
            BackgroundColor = Kids.ColorOr(Kids.Prop(props, "background"), "#4f46e5"),
            Padding = new Thickness(20, 14),
            Content = new Label
            {
                Text = Kids.Prop(props, "title"), FontSize = 16, FontAttributes = FontAttributes.Bold,
                TextColor = Kids.ColorOr(Kids.Prop(props, "textColor"), "#ffffff"),
            },
        };
}
