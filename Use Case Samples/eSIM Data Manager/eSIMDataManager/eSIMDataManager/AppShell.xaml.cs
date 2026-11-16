using eSIMDataManager.ViewModels;

namespace eSIMDataManager;

public partial class AppShell : Shell
{
    public AppShell(MainViewModel viewModel)
    {
        InitializeComponent();

        var section = new ShellSection { Route = "main", Title = string.Empty };
#if ANDROID || IOS
        section.Items.Add(CreatePage("dashboard", () => new MobilePage(viewModel, MobilePage.MobileTab.Dashboard)));
        section.Items.Add(CreatePage("profiles", () => new MobilePage(viewModel, MobilePage.MobileTab.Profiles)));
        section.Items.Add(CreatePage("store", () => new MobilePage(viewModel, MobilePage.MobileTab.Store)));
#else
        section.Items.Add(CreatePage("dashboard", () => new DashboardPage(viewModel)));
        section.Items.Add(CreatePage("profiles", () => new ProfilesPage(viewModel)));
        section.Items.Add(CreatePage("store", () => new StorePage(viewModel)));
#endif
        Shell.SetFlyoutBehavior(section, FlyoutBehavior.Disabled);
        Shell.SetNavBarIsVisible(section, false);
        Shell.SetTabBarIsVisible(section, false);

        var shellItem = new ShellItem { Title = string.Empty, Items = { section } };
        Shell.SetFlyoutBehavior(shellItem, FlyoutBehavior.Disabled);
        Shell.SetNavBarIsVisible(shellItem, false);
        Shell.SetTabBarIsVisible(shellItem, false);
        Items.Add(shellItem);
    }

    private static ShellContent CreatePage(string route, Func<Page> pageFactory)
    {
        var content = new ShellContent
        {
            Route = route,
            Title = string.Empty,
            ContentTemplate = new DataTemplate(() => pageFactory())
        };
        Shell.SetFlyoutBehavior(content, FlyoutBehavior.Disabled);
        Shell.SetNavBarIsVisible(content, false);
        Shell.SetTabBarIsVisible(content, false);
        return content;
    }
}
