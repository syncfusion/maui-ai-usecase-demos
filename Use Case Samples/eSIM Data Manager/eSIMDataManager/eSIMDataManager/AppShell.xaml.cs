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
        var shellItem = new ShellItem();
        shellItem.Items.Add(section);
        Items.Add(shellItem);
    }

    private static ShellContent CreatePage(string title, string route, Func<Page> pageFactory) => new()
    {
        Title = title,
        Route = route,
        ContentTemplate = new DataTemplate(() => pageFactory())
    };
}
