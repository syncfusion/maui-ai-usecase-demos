using Microsoft.Extensions.DependencyInjection;

namespace TravelPlanner;

public partial class AppShell : Shell
{
    public AppShell(IServiceProvider services)
    {
        InitializeComponent();
        Items.Add(new ShellContent
        {
            Title = "SyncTravel",
            Route = "MainPage",
            Content = services.GetRequiredService<MainPage>()
        });
    }
}
