using NutriLens.Models;
using NutriLens.ViewModels;

namespace NutriLens.Views;

public partial class NutriLensDashboardPage : ContentPage
{
    public NutriLensDashboardPage(NutriLensDashboardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    public NutriLensDashboardPage()
        : this(new NutriLensDashboardViewModel())
    {
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