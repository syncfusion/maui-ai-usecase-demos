using NutriLens.Models;
using NutriLens.ViewModels;

namespace NutriLens.Views;

public partial class NutriLensDashboardPage : ContentPage
{
    private const double DesktopWidthBreakpoint = 900;

    private bool isDesktopLayout;

    public NutriLensDashboardPage(NutriLensDashboardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        ApplyResponsiveLayout();
    }

    public NutriLensDashboardPage()
        : this(new NutriLensDashboardViewModel())
    {
    }

    private void OnPageSizeChanged(object? sender, EventArgs e)
    {
        ApplyResponsiveLayout();
    }

    private void ApplyResponsiveLayout()
    {
        if (ResponsiveCardsGrid is null)
            return;

        var shouldUseDesktopLayout =
            DeviceInfo.Platform == DevicePlatform.WinUI ||
            DeviceInfo.Platform == DevicePlatform.MacCatalyst ||
            Width > DesktopWidthBreakpoint;

        if (shouldUseDesktopLayout == isDesktopLayout)
            return;

        isDesktopLayout = shouldUseDesktopLayout;

        if (shouldUseDesktopLayout)
        {
            ApplyDesktopCardLayout();
        }
        else
        {
            ApplyMobileCardLayout();
        }
    }

    private void ApplyDesktopCardLayout()
    {
        ResponsiveCardsGrid.RowDefinitions.Clear();
        ResponsiveCardsGrid.RowDefinitions.Add(
            new RowDefinition { Height = GridLength.Auto });

        ResponsiveCardsGrid.ColumnDefinitions.Clear();
        ResponsiveCardsGrid.ColumnDefinitions.Add(
            new ColumnDefinition
            {
                Width = new GridLength(2, GridUnitType.Star)
            });
        ResponsiveCardsGrid.ColumnDefinitions.Add(
            new ColumnDefinition
            {
                Width = new GridLength(2, GridUnitType.Star)
            });

        Grid.SetRow(ScanCard, 0);
        Grid.SetColumn(ScanCard, 0);

        Grid.SetRow(DailyInsightCard, 0);
        Grid.SetColumn(DailyInsightCard, 1);

        ScanCard.VerticalOptions = LayoutOptions.Fill;
        DailyInsightCard.VerticalOptions = LayoutOptions.Fill;
    }

    private void ApplyMobileCardLayout()
    {
        ResponsiveCardsGrid.RowDefinitions.Clear();
        ResponsiveCardsGrid.RowDefinitions.Add(
            new RowDefinition { Height = GridLength.Auto });
        ResponsiveCardsGrid.RowDefinitions.Add(
            new RowDefinition { Height = GridLength.Auto });

        ResponsiveCardsGrid.ColumnDefinitions.Clear();
        ResponsiveCardsGrid.ColumnDefinitions.Add(
            new ColumnDefinition
            {
                Width = GridLength.Star
            });

        Grid.SetRow(ScanCard, 0);
        Grid.SetColumn(ScanCard, 0);

        Grid.SetRow(DailyInsightCard, 1);
        Grid.SetColumn(DailyInsightCard, 0);

        ScanCard.VerticalOptions = LayoutOptions.Fill;
        DailyInsightCard.VerticalOptions = LayoutOptions.Fill;
    }

    private async void OnRecentScanTapped(
        object? sender,
        Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        if (e.DataItem is RecentScanItem item
            && BindingContext is NutriLensDashboardViewModel vm)
        {
            await vm.RecentScanTappedCommand.ExecuteAsync(item);
        }
    }
}