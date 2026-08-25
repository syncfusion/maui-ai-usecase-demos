using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;

namespace AIPoweredChartSample
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

            builder.Services.AddSingleton<IAzureAIService, AzureAIService>();
            builder.Services.AddTransient<ChartViewModel>();
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<MobileAssistViewPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            var app = builder.Build();
            ServiceHelper.CurrentServices = app.Services;
            return app;
        }
    }
}
