using ListView_IntentGrouping.Services;
using ListView_IntentGrouping.ViewModels;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;

namespace ListView_IntentGrouping
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
                    fonts.AddFont("MauiMaterialAssets.ttf", "MaterialAssets");
                });

            if (AzureBaseService.IsConfigured)
            {
                builder.Services.AddSingleton<IIntentAnalyzer>(
                    sp => AzureOpenAIIntentAnalyzer.Create(
                        sp.GetService<ILogger<AzureOpenAIIntentAnalyzer>>()));
            }
            else
            {
                builder.Services.AddSingleton<IIntentAnalyzer, IntentAnalyzerService>();
            }

            builder.Services.AddSingleton<IntentAnalyzerService>();
            builder.Services.AddSingleton<MainPageViewModel>();
            builder.Services.AddSingleton<MainPage>();


#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
