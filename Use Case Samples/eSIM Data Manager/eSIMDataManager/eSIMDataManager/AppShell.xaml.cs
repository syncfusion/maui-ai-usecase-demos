using Microsoft.Extensions.DependencyInjection;

namespace eSIMDataManager;

public partial class AppShell : Shell
{
    public AppShell(IServiceProvider services)
    {
        InitializeComponent();

        foreach (var shellContent in Items
                     .OfType<TabBar>()
                     .SelectMany(tabBar => tabBar.Items)
                     .SelectMany(tab => tab.Items))
        {
            shellContent.ContentTemplate = new DataTemplate(() => services.GetRequiredService<MainPage>());
        }
    }
}
