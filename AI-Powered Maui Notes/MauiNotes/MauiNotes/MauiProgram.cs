using Microsoft.Extensions.Logging;
using MauiNotes.Services.AI;
using MauiNotes.Services.Persistence;
using MauiNotes.Controls;
using MauiNotes.ViewModels;
using Syncfusion.Maui.Core.Hosting;
#if MAUI_DEVFLOW
using Microsoft.Maui.DevFlow.Agent;
#endif

namespace MauiNotes
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            // Register Syncfusion community license (required for v20.2.0.x+).
            // The key is supplied via the SYNCFUSION_LICENSE_KEY environment variable.
            string licenseKey = Environment.GetEnvironmentVariable("SYNCFUSION_LICENSE_KEY") ?? string.Empty;
            if (string.IsNullOrWhiteSpace(licenseKey))
            {
                throw new InvalidOperationException(
                    "SYNCFUSION_LICENSE_KEY environment variable is not set. A Syncfusion license key " +
                    "(required for Syncfusion v20.2.0.x and later) must be provided before the app starts, " +
                    "e.g. as a run configuration / launchSettings.json environment variable.");
            }

            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(licenseKey);

            var builder = MauiApp.CreateBuilder();
            builder.Services.AddSingleton(new AzureOpenAiOptions());
            builder.Services.AddSingleton(new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(60)
            });
            builder.Services.AddSingleton<IAiEditingService, AzureOpenAiEditingService>();
            builder.Services.AddSingleton<INotePersistenceService, JsonNotePersistenceService>();
            builder.Services.AddSingleton<NotesViewModel>();

            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("BorderlessEntry", (handler, view) =>
            {
                if (view is not BorderlessEntry)
                    return;

#if WINDOWS
                handler.PlatformView.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
                handler.PlatformView.BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Microsoft.UI.Colors.Transparent);
#elif ANDROID
                handler.PlatformView.Background = null;
#elif IOS || MACCATALYST
                handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
#endif
            });

            builder
                .UseMauiApp<App>()
                .ConfigureSyncfusionCore()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("MauiMaterialAssets.ttf", "MauiMaterialAssets");
                    fonts.AddFont("materialicon-regular.ttf", "MaterialIconRegular");
                });

#if MAUI_DEVFLOW
            builder.AddMauiDevFlowAgent();
#endif

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
