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
            SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JAaF1cXmhIfkx1WmFZfVhgdVRMZVpbQHBPMyBoS35RcEVqWH9eeHVQR2VeVExzVEFZ");

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureSyncfusionCore()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
                    // TODO: Add Inter-Regular.ttf and Inter-SemiBold.ttf to Resources/Fonts for PDS typography compliance
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
