namespace TravelPlanner;

public partial class App : Application
{
	public App()
	{
		// TODO: Register your Syncfusion license key before shipping.
		// For development: paste your key into the call below.
		// For CI/secure environments: load the key from an environment variable or
		// a secrets store and pass it to RegisterLicense here. Do NOT commit a
		// real key to source control.
		Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(
			Environment.GetEnvironmentVariable("SYNCFUSION_LICENSE_KEY") ?? "ENTER_YOUR_LICENSE_KEY");

		InitializeComponent();
		Application.Current.UserAppTheme = AppTheme.Light;
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}
}