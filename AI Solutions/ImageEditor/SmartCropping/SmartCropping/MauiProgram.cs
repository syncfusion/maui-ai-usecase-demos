using Microsoft.Extensions.Logging;
using SmartCropping.ViewModel;
using Syncfusion.Maui.Core.Hosting;

namespace SmartCropping;

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

        // One view model per request; the shell picks the desktop or mobile
        // page and resolves it from the same singleton service.
        builder.Services.AddTransient<SmartCroppingViewModel>();

        builder.Services.AddTransient<SmartCropping>();
        builder.Services.AddTransient<SmartCroppingMobileView>();

        builder.Services.AddSingleton<AppShell>();


#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}