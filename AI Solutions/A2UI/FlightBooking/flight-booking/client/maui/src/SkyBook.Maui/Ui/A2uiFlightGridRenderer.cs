using Syncfusion.A2UI.Core.Rendering;
using Syncfusion.Maui.A2UI.Catalog;
using Syncfusion.Maui.DataGrid;
using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.Json;

namespace SkyBook.Maui.Ui;

/// <summary>
/// Generic replacement for the catalog's "SyncfusionDataGrid" renderer.
/// Columns come from the JSON "columns" array, rows from "dataSource" (any shape).
/// A column whose field is "action" renders a blue "Book now" button.
/// Sorting is never touched, which avoids the NullReferenceException in
/// DataGridHeaderCell.InitializeSortImage.
/// </summary>
public sealed class A2uiFlightGridRenderer : IA2uiComponentRenderer
{
  public string ComponentName => "SyncfusionDataGrid";

  /// <summary>Raised when a row's "Book now" button is pressed.</summary>
  public static Action<IDictionary<string, object>>? BookRequested { get; set; }

  public View CreateView(IReadOnlyDictionary<string, object?> props, IBuildChild buildChild, ComponentContext context)
  {
    var columns = ReadColumns(props.TryGetValue("columns", out var c) ? c : null);
    var rows = new ObservableCollection<Dictionary<string, object?>>(ReadRows(props.TryGetValue("dataSource", out var ds) ? ds : null));
    var hasAction = columns.Any(x => x.Field == "action");
    var selection = props.TryGetValue("selectionMode", out var sm) && string.Equals(Convert.ToString(sm), "none", StringComparison.OrdinalIgnoreCase)
        ? DataGridSelectionMode.None : DataGridSelectionMode.Single;

    var grid = new SfDataGrid
    {
      AutoGenerateColumnsMode = AutoGenerateColumnsMode.None,
      SortingMode = DataGridSortingMode.None,
      SelectionMode = selection,
      ColumnWidthMode = ColumnWidthMode.Fill, // stretch on wide screens; columns' MinimumWidth makes it scroll sideways on phones
      GridLinesVisibility = GridLinesVisibility.Horizontal,
      HeaderGridLinesVisibility = GridLinesVisibility.Horizontal,
      HeaderRowHeight = 48,
      RowHeight = hasAction ? 56 : 48,
      HeightRequest = 48 + Math.Max(rows.Count, 1) * (hasAction ? 56 : 48) + 4,
      HorizontalOptions = LayoutOptions.Fill,
      BackgroundColor = Colors.White,
      DefaultStyle = new DataGridStyle
      {
        HeaderRowBackground = Colors.White,
        HeaderRowTextColor = Color.FromArgb("#374151"),
        HeaderRowFontAttributes = FontAttributes.Bold,
        RowBackground = Colors.White,
        RowTextColor = Color.FromArgb("#4B5563"),
        GridLineColor = Color.FromArgb("#E5E7EB"),
        SelectionBackground = Color.FromArgb("#EEF2FF"),
      },
    };

    var initialized = false;
    grid.HandlerChanged += (_, _) =>
    {
      if (initialized || grid.Handler is null) return;
      initialized = true;
      foreach (var col in columns)
      {
        var column = BuildColumn(col);
        // Each column is at least as wide as its header and longest value, so nothing is cut off.
        // On a wide screen the columns still stretch (Fill); on a phone the grid scrolls sideways.
        column.MinimumWidth = MinWidth(col, rows);
        grid.Columns.Add(column);
      }
      grid.ItemsSource = rows;
    };
    return grid;
  }

  private static double MinWidth(ColumnDef col, IEnumerable<Dictionary<string, object?>> rows)
  {
    if (col.Field == "action") return 130;
    var longest = rows.Select(r => r.TryGetValue(col.Field, out var v) ? Convert.ToString(v, CultureInfo.InvariantCulture)?.Length ?? 0 : 0)
                      .DefaultIfEmpty(0).Max();
    if (!string.IsNullOrEmpty(col.Format)) longest += 2; // "$1,840"
    var chars = Math.Max(longest, col.Header.Length);
    return Math.Clamp(chars * 8.5 + 40, 90, 320);
  }

  private static DataGridColumn BuildColumn(ColumnDef col)
  {
    if (col.Field == "action")
    {
      return new DataGridTemplateColumn
      {
        MappingName = col.Field,
        HeaderText = col.Header,
        AllowSorting = false,
        HeaderTextAlignment = TextAlignment.End,
        CellTemplate = new DataTemplate(() =>
        {
          var btn = new Button
          {
            Text = "Book now",
            BackgroundColor = Color.FromArgb("#2563EB"),
            TextColor = Colors.White,
            FontAttributes = FontAttributes.Bold,
            CornerRadius = 8,
            HeightRequest = 36,
            Padding = new Thickness(14, 0),
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Center,
          };
          btn.Clicked += (s, _) =>
          {
            if ((s as BindableObject)?.BindingContext is IDictionary<string, object?> row)
              BookRequested?.Invoke(row.ToDictionary(k => k.Key, k => k.Value ?? ""));
          };
          return new Grid { Padding = new Thickness(0, 0, 8, 0), Children = { btn } };
        }),
      };
    }

    // SfDataGrid can't map dictionary keys by MappingName, so every column
    // is a template column whose Label binds to the row dictionary indexer.
    var align = col.AlignEnd ? TextAlignment.End : TextAlignment.Start;
    var format = string.IsNullOrEmpty(col.Format) ? null : "{0:" + col.Format + "}";
    return new DataGridTemplateColumn
    {
      MappingName = col.Field,
      HeaderText = col.Header,
      AllowSorting = false,
      HeaderTextAlignment = align,
      CellTemplate = new DataTemplate(() =>
      {
        var label = new Label
        {
          TextColor = Color.FromArgb("#4B5563"),
          FontSize = 14,
          VerticalOptions = LayoutOptions.Center,
          HorizontalTextAlignment = align,
          LineBreakMode = LineBreakMode.NoWrap,
        };
        label.SetBinding(Label.TextProperty, new Binding($"[{col.Field}]", stringFormat: format));
        return new Grid { Padding = new Thickness(16, 0), Children = { label } };
      }),
    };
  }

  // ---------- parsing ----------
  private sealed record ColumnDef(string Field, string Header, string? Format, bool AlignEnd);

  private static List<ColumnDef> ReadColumns(object? source)
  {
    var list = new List<ColumnDef>();
    foreach (var item in Items(source))
    {
      var field = Get(item, "field") ?? "";
      if (field.Length == 0) continue;
      list.Add(new ColumnDef(field, Get(item, "headerText") ?? field, Get(item, "format"),
          string.Equals(Get(item, "textAlign"), "end", StringComparison.OrdinalIgnoreCase)
          || string.Equals(Get(item, "textAlign"), "right", StringComparison.OrdinalIgnoreCase)));
    }
    return list;
  }

  private static IEnumerable<Dictionary<string, object?>> ReadRows(object? source)
  {
    foreach (var item in Items(source))
    {
      var row = new Dictionary<string, object?>(StringComparer.Ordinal);
      foreach (var (k, v) in Fields(item)) row[k] = v;
      yield return row;
    }
  }

  private static IEnumerable<object> Items(object? source)
  {
    if (source is JsonElement je && je.ValueKind == JsonValueKind.Array)
      foreach (var e in je.EnumerateArray()) yield return e;
    else if (source is IEnumerable list and not string)
      foreach (var e in list) if (e is not null) yield return e;
  }

  private static IEnumerable<(string, object?)> Fields(object item)
  {
    if (item is JsonElement e && e.ValueKind == JsonValueKind.Object)
      foreach (var p in e.EnumerateObject()) yield return (p.Name, Scalar(p.Value));
    else if (item is IDictionary<string, object?> d)
      foreach (var kv in d) yield return (kv.Key, kv.Value is JsonElement je ? Scalar(je) : kv.Value);
    else if (item is IDictionary nd)
      foreach (DictionaryEntry kv in nd) yield return (Convert.ToString(kv.Key)!, kv.Value is JsonElement je ? Scalar(je) : kv.Value);
  }

  private static string? Get(object item, string key)
  {
    foreach (var (k, v) in Fields(item))
      if (k == key) return Convert.ToString(v, CultureInfo.InvariantCulture);
    return null;
  }

  private static object? Scalar(JsonElement v) => v.ValueKind switch
  {
    JsonValueKind.Number => v.GetDouble(),
    JsonValueKind.String => v.GetString(),
    JsonValueKind.True => true,
    JsonValueKind.False => false,
    JsonValueKind.Null => null,
    _ => v.ToString(),
  };
}
