using eSIMDataManager.ViewModels;

namespace eSIMDataManager;

public partial class App : Application
{
    public App(MainViewModel viewModel)
    {
        InitializeComponent();
#if ANDROID || IOS
        MainPage = new MobileHostPage(viewModel);
#else
        MainPage = new AppShell(viewModel);
#endif
    }
}
