using eSIMDataManager.ViewModels;
using Syncfusion.Maui.TabView;

namespace eSIMDataManager;

public partial class AppShell : Shell
{
    public AppShell(MainViewModel viewModel)
    {
        InitializeComponent();
        TopTabs.Items[0].Content = new DashboardPage(viewModel);
        TopTabs.Items[1].Content = new ProfilesPage(viewModel);
        TopTabs.Items[2].Content = new StorePage(viewModel);
    }

    private void TopTabs_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        Title = TopTabs.Items[e.NewIndex].Header?.ToString();
        if (e.NewIndex == 3) TopTabs.Items[3].Content = new OrdersPage();
    }
}
