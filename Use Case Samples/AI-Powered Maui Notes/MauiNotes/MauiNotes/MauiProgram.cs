using Microsoft.Extensions.Logging;
using MauiNotes.Services.AI;
using MauiNotes.Services.Persistence;
using MauiNotes.Services.Navigation;
using MauiNotes.Controls;
using MauiNotes.ViewModels;
using MauiNotes.Views.Editor;
using MauiNotes.Views.Notes;
using Syncfusion.Maui.Core.Hosting;
#if MAUI_DEVFLOW
using Microsoft.Maui.DevFlow.Agent;
#endif

namespace MauiNotes
{
    public static class MauiProgram
    {
        private const string SyncfusionLicensePlaceholder = "YOUR_SYNCFUSION_LICENSE_KEY";

        public static MauiApp CreateMauiApp()
        {
            // Register Syncfusion community license (required for v20.2.0.x+).
            string licenseKey = Environment.GetEnvironmentVariable("SYNCFUSION_LICENSE_KEY")
                ?? SyncfusionLicensePlaceholder;
            if (!string.Equals(licenseKey, SyncfusionLicensePlaceholder, StringComparison.Ordinal))
                Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(licenseKey);

            var builder = MauiApp.CreateBuilder();
            builder.Services.AddSingleton(new AzureOpenAiOptions());
            builder.Services.AddSingleton<AiHtmlSanitizer>();
            builder.Services.AddSingleton<AiResponseFormatter>();
            builder.Services.AddHttpClient<IAiEditingService, AzureOpenAiEditingService>(client =>
                client.Timeout = TimeSpan.FromSeconds(60));
            builder.Services.AddSingleton<INotePersistenceService, JsonNotePersistenceService>();
            builder.Services.AddSingleton<INoteNavigationService, ShellNoteNavigationService>();
            builder.Services.AddSingleton<NotesViewModel>();
            builder.Services.AddSingleton<AppShell>();
            builder.Services.AddTransient<NotesPageDesktopUI>();
            builder.Services.AddTransient<NotesPageMobileUI>();
            builder.Services.AddTransient<EditorPageMobileUI>();

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
                    fonts.AddFont("MauiMaterialAssets.ttf", "MauiMaterialAssets");
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
