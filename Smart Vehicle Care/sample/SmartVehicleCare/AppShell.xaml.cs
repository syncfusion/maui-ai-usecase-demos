using SmartVehicleCare.Views;

namespace SmartVehicleCare
{
    public partial class AppShell : Shell
    {
        public AppShell(SplashPage splashPage, WelcomePage welcomePage, MainPage mainPage)
        {
            InitializeComponent();
            SplashShellContent.Content = splashPage;
            WelcomeShellContent.Content = welcomePage;
            MainShellContent.Content = mainPage;
        }
    }
}
