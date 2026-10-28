using Microsoft.Extensions.DependencyInjection;

namespace SmartCropping
{
    public partial class App : Application
    {
        private readonly IServiceProvider serviceProvider;

        public App(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            InitializeComponent();
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("YOUR_LICENSE_KEY");
        }

        protected override Window CreateWindow(IActivationState? activationState) =>
            new(serviceProvider.GetRequiredService<AppShell>());
    }
}
