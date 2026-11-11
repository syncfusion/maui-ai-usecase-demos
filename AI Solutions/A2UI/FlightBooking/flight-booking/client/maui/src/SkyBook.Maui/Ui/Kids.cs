using System.Globalization;
using Syncfusion.Maui.A2UI.Catalog;
using Syncfusion.Maui.A2UI.Internals;

namespace SkyBook.Maui.Ui;

/// <summary>Small helpers shared by the custom A2UI renderers (the stock helpers are internal to Syncfusion.Maui.A2UI).</summary>
internal static class Kids
{
    public static List<View> Build(IReadOnlyDictionary<string, object?> props, IBuildChild build)
    {
        var list = new List<View>();
        if (!props.TryGetValue("children", out var value) || value is null) return list;
        IEnumerable<object?> items = value is string
            ? [value]
            : value is System.Collections.IEnumerable e ? e.Cast<object?>() : [value];
        foreach (var item in items)
        {
            View? view = item switch
            {
                MauiNodeRef r => build.Build(r.Id, r.BasePath),
                string s when s.Length > 0 => build.Build(s),
                IDictionary<string, object> d when d.TryGetValue("id", out var id) && id is string sid =>
                    build.Build(sid, d.TryGetValue("basePath", out var bp) ? bp as string : null),
                _ => null,
            };
            if (view is not null) list.Add(view);
        }
        return list;
    }

    /// <summary>Number or CSS length ("16", 16, "16px") -> double.</summary>
    public static double Num(IReadOnlyDictionary<string, object?> props, string key, double fallback = 0)
    {
        if (!props.TryGetValue(key, out var v) || v is null) return fallback;
        var s = Str(v).Trim();
        if (s.EndsWith("px", StringComparison.OrdinalIgnoreCase)) s = s[..^2];
        return double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : fallback;
    }

    public static string Str(object? v) => v switch
    {
        null => "",
        string s => s,
        IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
        _ => v.ToString() ?? "",
    };

    public static string Prop(IReadOnlyDictionary<string, object?> props, string key) =>
        props.TryGetValue(key, out var v) ? Str(v) : "";

    public static Color ColorOr(string value, string fallback)
    {
        try { return Color.FromArgb(string.IsNullOrWhiteSpace(value) ? fallback : value); }
        catch (FormatException) { return Color.FromArgb(fallback); }
    }
}
