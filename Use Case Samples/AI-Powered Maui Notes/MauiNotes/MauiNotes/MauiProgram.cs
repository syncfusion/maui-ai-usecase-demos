using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;
using MauiNotes.Services.AI;
using MauiNotes.Services.Editor;
using MauiNotes.Services.Navigation;
using MauiNotes.Services.Persistence;
using MauiNotes.Services.Theming;
using MauiNotes.ViewModels;
using MauiNotes.Views.Editor;
using MauiNotes.Views.Notes;
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
            string? licenseKey = Environment.GetEnvironmentVariable("SYNCFUSION_LICENSE_KEY");
            if (!string.IsNullOrWhiteSpace(licenseKey) &&
                !string.Equals(licenseKey, SyncfusionLicensePlaceholder, StringComparison.Ordinal))
            {
                Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(licenseKey);
            }

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureSyncfusionCore()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("MauiMaterialAssets.ttf", "MauiMaterialAssets");
                    fonts.AddFont("materialicon-regular.ttf", "MaterialIconRegular");
                    fonts.AddFont("UIFontIcons.ttf", "UIFontIcons");
                    fonts.AddFont("UIKitIcons.ttf", "UIKitIcons");
                });

#if MAUI_DEVFLOW
            builder.AddMauiDevFlowAgent();
#endif

#if DEBUG
            builder.Logging.AddDebug();
#endif

            builder.Services.AddSingleton(new AzureOpenAiOptions());
            builder.Services.AddHttpClient<IAiEditingService, AzureOpenAiEditingService>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(60);
            });
            builder.Services.AddSingleton<INotePersistenceService, JsonNotePersistenceService>();
            builder.Services.AddSingleton<IRichTextEditorAdapter, RichTextEditorAdapter>();
            builder.Services.AddSingleton<INavigationService, ShellNavigationService>();
            builder.Services.AddSingleton<IThemeService, SyncfusionThemeService>();
            builder.Services.AddSingleton<NotesViewModel>();
            builder.Services.AddSingleton<App>();
            builder.Services.AddSingleton<AppShell>();
            builder.Services.AddTransient<NotesPageDesktopUI>();
            builder.Services.AddTransient<NotesPageMobileUI>();
            builder.Services.AddTransient<EditorPageMobileUI>();

            return builder.Build();
        }
    }
}
