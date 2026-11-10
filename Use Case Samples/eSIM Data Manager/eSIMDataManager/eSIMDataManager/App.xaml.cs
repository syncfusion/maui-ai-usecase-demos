using eSIMDataManager.ViewModels;

namespace eSIMDataManager;

public partial class App : Application
{
    public App(MainViewModel viewModel)
    {
        InitializeComponent();
        MainPage = new NavigationPage(new DashboardPage(viewModel));
    }
}
