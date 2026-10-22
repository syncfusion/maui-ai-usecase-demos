using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;

namespace SmartDailyDecisionCompanion
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            // Uncomment and replace with your Syncfusion license key.
            // Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("YOUR_LICENSE_KEY");

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureSyncfusionCore()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton<IAzureAIService, AzureAIService>();
            builder.Services.AddTransient<MainViewModel>();
            builder.Services.AddTransient<MainPage>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
