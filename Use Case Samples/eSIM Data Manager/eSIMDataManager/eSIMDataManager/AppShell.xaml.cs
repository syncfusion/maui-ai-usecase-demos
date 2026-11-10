using eSIMDataManager.ViewModels;

namespace eSIMDataManager;

public partial class AppShell : Shell
{
    public AppShell(MainViewModel viewModel)
    {
        InitializeComponent();

        foreach (var shellContent in Items
                     .OfType<TabBar>()
                     .SelectMany(tabBar => tabBar.Items)
                     .SelectMany(tab => tab.Items))
        {
            shellContent.ContentTemplate = new DataTemplate(() => new MainPage(viewModel));
        }
    }
}
