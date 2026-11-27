using Syncfusion.Maui.Core.Hosting;
using eSIMDataManager.Services;
using eSIMDataManager.ViewModels;

namespace eSIMDataManager;

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
                fonts.AddFont("Inter.ttf", "Inter");
                fonts.AddFont("MauiMaterialAssets.ttf", "MaterialAssets");
                fonts.AddFont("UiKitIcons.ttf", "UiKitIcons");
            });

        // Core singletons
        builder.Services.AddSingleton<EsimStore>();
        builder.Services.AddSingleton<IDialogService, DialogService>();
        builder.Services.AddSingleton<INavigationService, NavigationService>();

        // ViewModels
        builder.Services.AddSingleton<TopUpViewModel>();
        builder.Services.AddSingleton<DashboardViewModel>();
        builder.Services.AddSingleton<ProfilesViewModel>();
        builder.Services.AddSingleton<StoreViewModel>();
        builder.Services.AddSingleton<MainViewModel>();

        // Views and Shell
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<ProfilesPage>();
        builder.Services.AddTransient<StorePage>();

        return builder.Build();
    }
}
