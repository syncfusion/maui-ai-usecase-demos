namespace ESimDataManager;

public partial class App : Application
{
    private readonly AppShell shell;

    public App(AppShell shell)
    {
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(Environment.GetEnvironmentVariable("SYNCFUSION_LICENSE_KEY") ?? "ENTER_YOUR_LICENSE_KEY");
        InitializeComponent();
        this.shell = shell;
    }

    protected override Window CreateWindow(IActivationState? activationState) => new(shell);
}
