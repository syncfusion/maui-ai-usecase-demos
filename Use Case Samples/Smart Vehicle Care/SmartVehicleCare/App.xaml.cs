namespace SmartVehicleCare
{
    public partial class App : Application
    {
        private readonly IServiceProvider _services;

        public App(IServiceProvider services)
        {
            // Merge App.xaml's resources (Colors.xaml/Styles.xaml) before any page is constructed —
            // AppShell's pages read StaticResources from them, so they must resolve after this call.
            InitializeComponent();
            _services = services;
        }

        protected override Window CreateWindow(IActivationState? activationState)
            => new Window(_services.GetRequiredService<AppShell>());
    }
}