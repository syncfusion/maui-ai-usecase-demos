namespace ContextAwareSuggestions
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JBaF5cXmRCf1NpRmNGfV5yckVHYlZVRXxdQk0DNHVRdkdlWXZdcHVXQmFeUEZyX0RWYEw=");
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new ContextAwareSuggestionsPage());
        }
    }
}