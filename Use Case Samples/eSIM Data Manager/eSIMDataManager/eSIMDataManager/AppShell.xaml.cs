using eSIMDataManager.ViewModels;

namespace eSIMDataManager;

public partial class AppShell : Shell
{
    public AppShell(MainViewModel viewModel)
    {
        InitializeComponent();

        var tabBar = new TabBar();
        tabBar.Items.Add(CreateTab("Dashboard", "dashboard", () => new DashboardPage(viewModel)));
        tabBar.Items.Add(CreateTab("My eSIMs", "profiles", () => new ProfilesPage(viewModel)));
        tabBar.Items.Add(CreateTab("Explore Plans", "store", () => new StorePage(viewModel)));
        tabBar.Items.Add(CreateTab("Orders", "orders", () => new OrdersPage()));
        Items.Add(tabBar);
    }

    private static ShellContent CreateTab(string title, string route, Func<Page> pageFactory) => new()
    {
        Title = title,
        Route = route,
        ContentTemplate = new DataTemplate(() => pageFactory())
    };
}
