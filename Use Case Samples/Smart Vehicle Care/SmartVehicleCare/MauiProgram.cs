using Microsoft.Extensions.Logging;
using SmartVehicleCare.Views;
using SmartVehicleCare.ViewModels;
using SmartVehicleCare.Services;
using Syncfusion.Maui.Core.Hosting;
using Syncfusion.Licensing;

namespace SmartVehicleCare
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            // The Syncfusion license is read from the SYNCFUSION_LICENSE_KEY environment
            // variable. Without a valid license, Syncfusion controls render a watermark or
            // trial dialog. Set the variable before launching the app.
            string? licenseKey = Environment.GetEnvironmentVariable("SYNCFUSION_LICENSE_KEY");
            if (!string.IsNullOrWhiteSpace(licenseKey))
                SyncfusionLicenseProvider.RegisterLicense(licenseKey);

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureSyncfusionCore()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
                });

            // Register the in-memory demo store as the single source of truth.
            builder.Services.AddSingleton<VehicleDataService>(_ => VehicleDataService.Instance);

            builder.Services.AddSingleton<WelcomeViewModel>();
            builder.Services.AddSingleton<AddVehicleViewModel>();
            builder.Services.AddSingleton<AddServiceViewModel>();
            builder.Services.AddSingleton<AddScheduleViewModel>();
            builder.Services.AddSingleton<AddFuelViewModel>();
            builder.Services.AddSingleton<MainViewModel>();
            builder.Services.AddSingleton<VehicleCenterViewModel>();
            builder.Services.AddSingleton<AIAssistViewModel>();
            builder.Services.AddSingleton<SettingsViewModel>();
            builder.Services.AddSingleton<SplashPage>();
            builder.Services.AddSingleton<WelcomePage>();
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddSingleton<AppShell>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
