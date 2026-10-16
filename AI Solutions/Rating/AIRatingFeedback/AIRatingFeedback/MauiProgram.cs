using AIRatingFeedback.Services;
using AIRatingFeedback.ViewModels;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;
using Syncfusion.Maui.Core.Hosting;

#if WINDOWS
using Microsoft.UI.Xaml.Media;
using Thickness = Microsoft.UI.Xaml.Thickness;
#endif

namespace AIRatingFeedback;

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

        builder.Services.AddSingleton<OpenAIService>();
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<MainPage>();

#if WINDOWS
        EditorHandler.Mapper.AppendToMapping("RemoveBorder", (handler, view) =>
        {
            handler.PlatformView.BorderThickness = new Thickness(0);
            handler.PlatformView.Padding = new Thickness(0);
            handler.PlatformView.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
            handler.PlatformView.Resources["TextControlBorderThemeThickness"] = new Thickness(0);
            handler.PlatformView.Resources["TextControlBorderThemeThicknessFocused"] = new Thickness(0);
        });
#endif

#if ANDROID
        EditorHandler.Mapper.AppendToMapping("RemoveUnderline", (handler, view) =>
        {
            handler.PlatformView.Background = null;
        });
#endif

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        // Validate Azure OpenAI configuration on startup so the user knows
        // immediately why AI features are not responding.
        var openAIService = app.Services.GetRequiredService<OpenAIService>();
        if (!openAIService.IsConfigured)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                var page = Application.Current?.Windows[0].Page;
                if (page is not null)
                {
                    await page.DisplayAlertAsync(
                        "Configuration Required",
                        "Azure OpenAI credentials not configured.\n\n" +
                        "Set these environment variables before launching the app:\n" +
                        "• AZURE_OPENAI_ENDPOINT\n" +
                        "• AZURE_OPENAI_DEPLOYMENT_NAME\n" +
                        "• AZURE_OPENAI_API_KEY\n\n" +
                        "AI rating will be unavailable until credentials are configured.",
                        "OK");
                }
            });
        }

        return app;
    }
}