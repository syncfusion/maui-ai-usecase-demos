using NutriLens.Helpers;
using NutriLens.Models;
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

    private void OnSearchFocused(object? sender, FocusEventArgs e)
    {
        SearchInputLayout.ShowHint = false;
    }

    private void OnSearchUnfocused(object? sender, FocusEventArgs e)
    {
        SearchInputLayout.ShowHint =
            string.IsNullOrWhiteSpace(SearchEntry.Text);
    }

    private async void OnHistoryItemTapped(
        object? sender,
        Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        if (e.DataItem is HistoryItem item && ViewModel is not null)
        {
            await ViewModel.RecentScanTappedCommand.ExecuteAsync(item);
        }
    }

    private async void OnHomeClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new NutriLensDashboardPage());
    }

    private async void OnScanClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new ScanIngredientsPage());
    }

    private async void OnProfileClicked(object? sender, EventArgs e)
    {
        await AppNavigator.GoProfileAsync();
    }

    private async void OnTrendsClicked(object? sender, EventArgs e)
    {
        await AppNavigator.GoTrendAsync();
    }
}