using AIFileSearchTreeView.Services;
using AIFileSearchTreeView.ViewModel;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;

namespace AIFileSearchTreeView
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
                    fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
                });

            // Services
            builder.Services.AddSingleton<ISemanticSearchService, SemanticSearchService>();

            // ViewModels
            builder.Services.AddSingleton<SalesFileViewModel>();

            // Views
            builder.Services.AddTransient<MainPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            var app = builder.Build();

            ServiceHelper.CurrentServices = app.Services;

            // Validate Azure OpenAI credentials on startup so the user knows
            // immediately why AI features are not responding.
            var semanticService = app.Services.GetRequiredService<ISemanticSearchService>();
            if (!semanticService.IsCredentialValid)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    var page = Application.Current?.Windows[0].Page;
                    if (page != null)
                    {
                        await page.DisplayAlertAsync(
                            "Configuration Required",
                            "Azure OpenAI credentials not configured.\n\n" +
                            "Set these environment variables before launching the app:\n" +
                            "• AZURE_OPENAI_ENDPOINT\n" +
                            "• AZURE_OPENAI_API_KEY\n" +
                            "• AZURE_OPENAI_DEPLOYMENT_NAME\n\n" +
                            "AI search will be unavailable until credentials are configured.",
                            "OK");
                    }
                });
            }

            return app;
        }
    }
}