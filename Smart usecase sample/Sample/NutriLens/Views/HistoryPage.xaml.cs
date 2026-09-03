using NutriLens.Helpers;
using NutriLens.ViewModels;

namespace NutriLens.Views;

public partial class HistoryPage : ContentPage
{
    private HistoryViewModel? ViewModel => BindingContext as HistoryViewModel;

    public HistoryPage()
    {
        InitializeComponent();

        BindingContext = new HistoryViewModel();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Refresh from the store every time the page appears so a scan
        // saved moments ago shows up immediately — no restart required.
        if (ViewModel is not null)
        {
            await ViewModel.LoadAsync();
        }
    }

    private async void OnHomeClicked(
        object? sender,
        EventArgs e)
    {
        await Navigation.PushAsync(
            new NutriLensDashboardPage());
    }

    private async void OnScanClicked(
        object? sender,
        EventArgs e)
    {
        await Navigation.PushAsync(
            new ScanIngredientsPage());
    }

    private async void OnProfileClicked(
        object? sender,
        EventArgs e)
    {
        await AppNavigator.GoProfileAsync();
    }

    private async void OnTrendsClicked(
        object? sender,
        EventArgs e)
    {
        await AppNavigator.GoTrendAsync();
    }

    private async void OnSelectClicked(
        object? sender,
        TappedEventArgs e)
    {
        await DisplayAlert(
            "Select",
            "Selection mode is not available yet.",
            "OK");
    }
}