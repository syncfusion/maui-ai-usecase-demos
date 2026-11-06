namespace AIFileSearchTreeView
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // The Syncfusion license is read from the SYNCFUSION_LICENSE_KEY environment
            // variable. Without a valid license, Syncfusion controls render a watermark or
            // trial dialog. Set the variable before launching the app.
            string? licenseKey = Environment.GetEnvironmentVariable("SYNCFUSION_LICENSE_KEY");
            if (!string.IsNullOrWhiteSpace(licenseKey))
            {
                Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(licenseKey);
            }
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}