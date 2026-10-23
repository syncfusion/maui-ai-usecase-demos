using Microsoft.Extensions.Logging;
using SmartCropping.ViewModel;
using Syncfusion.Maui.Core.Hosting;

namespace SmartCropping
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureSyncfusionCore()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton<ISmartCropService, SmartCropService>();

            builder.Services.AddTransient<SmartCroppingViewModel>();
            builder.Services.AddTransient<SmartCropping>();
            builder.Services.AddSingleton<AppShell>();


#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
