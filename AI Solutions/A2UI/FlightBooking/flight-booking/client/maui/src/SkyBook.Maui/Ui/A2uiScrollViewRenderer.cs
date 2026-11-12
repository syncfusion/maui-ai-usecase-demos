using Syncfusion.A2UI.Core.BasicCatalog;
using Syncfusion.A2UI.Core.Rendering;
using Syncfusion.Maui.A2UI.Catalog;

namespace SkyBook.Maui.Ui;

/// <summary>
/// A2UI "ScrollView" component renderer.
/// Stage JSON may wrap a subtree with component="ScrollView".
/// If not present in the base catalog, A2UI shows placeholders like "[Loading ...]".
/// </summary>
public sealed class A2uiScrollViewRenderer : IA2uiComponentRenderer
{
    public string ComponentName => "ScrollView";

    public View CreateView(IReadOnlyDictionary<string, object?> props, IBuildChild buildChild, ComponentContext context)
    {
        var scroll = new ScrollView
        {
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
        };

        var children = Kids.Build(props, buildChild);
        if (children.Count == 1)
        {
            scroll.Content = children[0];
        }
        else if (children.Count > 1)
        {
            var stack = new VerticalStackLayout { Spacing = 0, Padding = 0, HorizontalOptions = LayoutOptions.Fill };
            foreach (var c in children) stack.Add(c);
            scroll.Content = stack;
        }

        return scroll;
    }
}
