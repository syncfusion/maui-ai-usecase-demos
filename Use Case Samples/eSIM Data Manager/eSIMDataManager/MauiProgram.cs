using ESimDataManager.ViewModels;
using Syncfusion.Maui.Core.Hosting;

namespace ESimDataManager;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>().ConfigureSyncfusionCore();
        builder.Services.AddSingleton<EsimStore>();
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddSingleton<MainPage>();
#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
}
