using Microsoft.Extensions.Logging;
using NutriLens.Models;
using NutriLens.Services;
using NutriLens.ViewModels;
using NutriLens.Views;
using Syncfusion.Maui.Core.Hosting;

namespace NutriLens
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
                    fonts.AddFont(
                    "MaterialIcons-Regular.ttf",
                    "MaterialIconsRegular");
                });

            // AI services
            builder.Services.AddSingleton<IIngredientImageExtractionService, IngredientImageExtractionService>();
            builder.Services.AddSingleton<IAzureOpenAIIngredientService, AzureOpenAIIngredientService>();
            builder.Services.AddSingleton<IAzureOpenAIChatService, AzureOpenAIChatService>();

            // Daily insight pipeline
            builder.Services.AddSingleton<IDailyInsightCacheStore, PreferencesDailyInsightCacheStore>();
            builder.Services.AddSingleton<IDailyInsightGenerator, DailyInsightGenerator>();

            // Persistence + history
            builder.Services.AddSingleton<IScanHistoryStore, JsonScanHistoryStore>();
            builder.Services.AddSingleton<ICombinedScanHistory, CombinedScanHistory>();
            builder.Services.AddSingleton<IUserPreferenceStore, UserPreferenceStore>();

            // Sample / demo product analysis lookup
            builder.Services.AddSingleton<ISampleAnalysisDataService, SampleIngredientAnalysisService>();

            // Camera / file picker (uses WinRT gallery on unpackaged Windows)
            builder.Services.AddSingleton<IImagePickerService, ImagePickerService>();

            // Pages + their view models
            builder.Services.AddTransient<NutriLensDashboardViewModel>();
            builder.Services.AddTransient<NutriLensDashboardPage>();
            builder.Services.AddTransient<ScanIngredientsViewModel>();
            builder.Services.AddTransient<ScanIngredientsPage>();
            builder.Services.AddTransient<ReviewIngredientsViewModel>();
            builder.Services.AddTransient<ReviewIngredientsPage>();
            builder.Services.AddTransient<AnalyzeIngredientsResultPage>();
            builder.Services.AddTransient<HistoryViewModel>();
            builder.Services.AddTransient<HistoryPage>();
            builder.Services.AddTransient<TrendsViewModel>();
            builder.Services.AddTransient<TrendsPage>();
            builder.Services.AddTransient<ProfileViewModel>();
            builder.Services.AddTransient<ProfilePage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
