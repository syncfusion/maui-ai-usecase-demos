using eSIMDataManager.ViewModels;
using Syncfusion.Maui.Buttons;
using Syncfusion.Maui.TabView;

namespace eSIMDataManager;

public partial class AppShell : Shell
{
    private readonly SfTabView topTabs;

    public AppShell(MainViewModel viewModel)
    {
        InitializeComponent();
        topTabs = new SfTabView();
        topTabs.Items.Add(new SfTabItem { Header = "Dashboard", Content = new DashboardPage(viewModel) });
        topTabs.Items.Add(new SfTabItem { Header = "My eSIMs", Content = new ProfilesPage(viewModel) });
        topTabs.Items.Add(new SfTabItem { Header = "Explore Plans", Content = new StorePage(viewModel) });
        topTabs.Items.Add(new SfTabItem { Header = "Orders", Content = new OrdersPage() });
        Content = topTabs;
    }
}
