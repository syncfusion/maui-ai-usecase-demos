using eSIMDataManager.ViewModels;

namespace eSIMDataManager;

public partial class AppShell : Shell
{
    public AppShell(MainViewModel viewModel)
    {
        InitializeComponent();

        var section = new ShellSection { Route = "main" };
        section.Items.Add(CreatePage("My eSIMs", "profiles", () => new ProfilesPage(viewModel)));
        section.Items.Add(CreatePage("Store", "store", () => new StorePage(viewModel)));
        Shell.SetNavBarIsVisible(section, false);
        var item = new ShellItem { Items = { section } };
        Shell.SetFlyoutBehavior(item, FlyoutBehavior.Disabled);
        Shell.SetNavBarIsVisible(item, false);
        Items.Add(item);
        Shell.SetFlyoutBehavior(this, FlyoutBehavior.Disabled);
        Shell.SetNavBarIsVisible(this, false);
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
