using Syncfusion.A2UI.Core.Rendering;
using Syncfusion.Maui.A2UI.Catalog;
using Syncfusion.Maui.A2UI.SyncfusionComponents;
using Syncfusion.Maui.Buttons;
using Syncfusion.Maui.DataGrid;

namespace SkyBook.Maui.Ui;

/// <summary>
/// Stock SyncfusionDataGrid + the React look (clean header, horizontal rules, fills the card) and a real
/// "Book now" button in the <c>action</c> column (React: one button per flight row).
/// </summary>
public sealed class SkyDataGridRenderer(Action<IDictionary<string, object>> onBook) : IA2uiComponentRenderer
{
    private readonly SyncfusionDataGridRenderer _inner = new();
    public string ComponentName => "SyncfusionDataGrid";

    public View CreateView(IReadOnlyDictionary<string, object?> props, IBuildChild buildChild, ComponentContext context)
    {
        var view = _inner.CreateView(props, buildChild, context);
        if (view is Layout layout && layout.Children.OfType<SfDataGrid>().FirstOrDefault() is { } grid)
        {
            Style(grid);
            ReplaceActionColumn(grid);
            // The page scrolls, so the grid must be exactly as tall as its rows (React: height "auto").
            var rows = (grid.ItemsSource as System.Collections.ICollection)?.Count ?? 0;
            grid.HeightRequest = grid.HeaderRowHeight + Math.Max(rows, 1) * grid.RowHeight + 2;
        }
        return view;
    }

    private static void Style(SfDataGrid grid)
    {
        grid.RowHeight = 48;
        grid.HeaderRowHeight = 48;
        grid.ColumnWidthMode = ColumnWidthMode.Fill;
        grid.GridLinesVisibility = GridLinesVisibility.Horizontal;
        grid.HeaderGridLinesVisibility = GridLinesVisibility.Horizontal;
        grid.DefaultStyle = new DataGridStyle
        {
            HeaderRowBackground = new SolidColorBrush(Colors.White),
            HeaderRowTextColor = Color.FromArgb("#475569"),
            HeaderRowFontAttributes = FontAttributes.Bold,
            HeaderRowFontSize = 13,
            RowBackground = new SolidColorBrush(Colors.White),
            RowTextColor = Color.FromArgb("#0f172a"),
            RowFontSize = 13,
            GridLineColor = Color.FromArgb("#e2e8f0"),
            HeaderGridLineColor = Color.FromArgb("#e2e8f0"),
        };
    }

    private void ReplaceActionColumn(SfDataGrid grid)
    {
        var index = -1;
        for (var i = 0; i < grid.Columns.Count; i++)
            if (grid.Columns[i].MappingName == "[action]") { index = i; break; }
        if (index < 0) return;

        var header = grid.Columns[index].HeaderText;
        grid.Columns[index] = new DataGridTemplateColumn
        {
            MappingName = "[action]",
            HeaderText = header,
            AllowSorting = false,
            CellTemplate = new DataTemplate(() =>
            {
                var button = new SfButton
                {
                    Text = "Book now", FontSize = 13, FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.White, Background = new SolidColorBrush(Color.FromArgb("#2563eb")),
                    CornerRadius = 6, HeightRequest = 32, WidthRequest = 92,
                    HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center,
                };
                button.Clicked += (s, _) =>
                {
                    if (((BindableObject)s!).BindingContext is IDictionary<string, object> row) onBook(row);
                };
                return button;
            }),
        };
    }
}
