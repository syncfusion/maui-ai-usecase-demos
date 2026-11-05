using Microsoft.Extensions.DependencyInjection;

namespace LLM_Routing
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            UserAppTheme = AppTheme.Light;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell())
            {
                Title = "LLM Router"
            };
        }
    }
}