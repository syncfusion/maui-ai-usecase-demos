using eSIMDataManager.ViewModels;

namespace eSIMDataManager;

public partial class AppShell : Shell
{
    public AppShell(MainViewModel viewModel)
    {
        InitializeComponent();

        var section = new ShellSection { Route = "main" };
        section.Items.Add(CreatePage("Dashboard", "dashboard", () => new DashboardPage(viewModel)));
        section.Items.Add(CreatePage("My eSIMs", "profiles", () => new ProfilesPage(viewModel)));
        section.Items.Add(CreatePage("Store", "store", () => new StorePage(viewModel)));
        Items.Add(new ShellItem { Items = { section } });
    }

    private static ShellContent CreatePage(string title, string route, Func<Page> pageFactory)
    {
        var content = new ShellContent
        {
            Title = title,
            Route = route,
            ContentTemplate = new DataTemplate(() => pageFactory())
        };
        Shell.SetNavBarIsVisible(content, false);
        return content;
    }
}
