using NutriLens.Helpers;
using NutriLens.ViewModels;

namespace NutriLens.Views;

public partial class TrendsPage : ContentPage
{
    public TrendsPage()
    {
        InitializeComponent();

        BindingContext =
            new TrendsViewModel();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Recompute trends from the saved scan history every time the
        // page appears, so new saved scans update the chart automatically.
        if (BindingContext is TrendsViewModel vm)
        {
            await vm.LoadAsync();
        }
    }

    // OnHomeClicked / OnHistoryClicked / OnScanClicked / OnProfileClicked
    // stay exactly as they are today.
    private async void OnHomeClicked(
        object? sender,
        EventArgs e)
    {
        await AppNavigator.GoDashboardAsync();
    }

    private async void OnHistoryClicked(
        object? sender,
        EventArgs e)
    {
        await AppNavigator.GoHistoryAsync();
    }

    private async void OnScanClicked(
        object? sender,
        TappedEventArgs e)
    {
        await AppNavigator.GoScanAsync();
    }

    private async void OnProfileClicked(
        object? sender,
        EventArgs e)
    {
        await AppNavigator.GoProfileAsync();
    }
}