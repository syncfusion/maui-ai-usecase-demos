using Microsoft.Extensions.Logging;
using SmartAIDatePicker.AIService;
using SmartAIDatePicker.ViewModels;
using Syncfusion.Maui.Core.Hosting;

namespace SmartAIDatePicker
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
                    fonts.AddFont("MauiMaterialAssets.ttf", "MauiMaterialAssets");
                    fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
                });

            builder.Services.AddSingleton<IAzureOpenAIService, AzureOpenAIService>();
            builder.Services.AddTransient<DatePickerViewModel>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
