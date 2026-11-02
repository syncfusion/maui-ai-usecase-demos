using Microsoft.Extensions.DependencyInjection;

namespace TravelPlanner;

public partial class App : Application
{
	private readonly IServiceProvider _services;

	public App(IServiceProvider services)
	{
		_services = services;
		// TODO: Register your Syncfusion license key before shipping.
		// For development: paste your key into the call below.
		// For CI/secure environments: load the key from an environment variable or
		// a secrets store and pass it to RegisterLicense here. Do NOT commit a
		// real key to source control.
		Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(
			Environment.GetEnvironmentVariable("SYNCFUSION_LICENSE_KEY") ?? "ENTER_YOUR_LICENSE_KEY");

		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(_services.GetRequiredService<AppShell>());
	}
}