using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace RichTextEditorAssistViewSample
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
                    fonts.AddFont("MauiSampleFontIcon.ttf", "MauiSampleFontIcon");
                });

            // DI registrations
            builder.Services.AddSingleton<IAzureAIService, AzureAIService>();

#if DEBUG
            builder.Logging.AddDebug();
#endif
            var app = builder.Build();
            ServiceHelper.CurrentServices = app.Services;
            return app;
        }
    }
}
