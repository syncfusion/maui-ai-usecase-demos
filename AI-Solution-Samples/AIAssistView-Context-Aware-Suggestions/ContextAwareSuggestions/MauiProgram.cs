using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;

namespace ContextAwareSuggestions
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

            builder.Services.AddSingleton<IAzureAIService,ContextAwareAzureAIService>();

            builder.Services.AddTransient<ContextAwareSuggestionsViewModel>();
            builder.Services.AddTransient<ContextAwareSuggestions.ContextAwareSuggestionsPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
