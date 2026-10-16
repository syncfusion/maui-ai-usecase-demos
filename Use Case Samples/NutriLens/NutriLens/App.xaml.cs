using Microsoft.Extensions.DependencyInjection;
using NutriLens.Views;

namespace NutriLens
{
    public partial class App : Application
    {
            private readonly IServiceProvider services;

            public App(IServiceProvider services)
        {
            InitializeComponent();
                this.services = services;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new NavigationPage(
                services.GetRequiredService<NutriLensDashboardPage>()));
        }
    }
}