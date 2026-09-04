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
            builder.Services.AddSingleton<IIngredientImageExtractionService, IngredientImageExtractionService>();
            builder.Services.AddSingleton<IAzureOpenAIIngredientService, AzureOpenAIIngredientService>();
            builder.Services.AddSingleton<IAzureOpenAIChatService, AzureOpenAIChatService>();
            builder.Services.AddSingleton<IDailyInsightCacheStore, PreferencesDailyInsightCacheStore>();
            builder.Services.AddSingleton<IDailyInsightGenerator, DailyInsightGenerator>();
            builder.Services.AddTransient<NutriLensDashboardViewModel>();
            builder.Services.AddTransient<NutriLensDashboardPage>(); 
            builder.Services.AddSingleton<IAzureOpenAIAnalysisService,AzureOpenAIAnalysisService>();
            builder.Services.AddSingleton<AnalysisSession>();
            builder.Services.AddSingleton<IScanHistoryStore, JsonScanHistoryStore>();
            builder.Services.AddTransient< ScanIngredientsViewModel>();
            builder.Services.AddTransient<TrendsViewModel>();
            builder.Services.AddTransient<TrendsPage>();
            builder.Services.AddTransient<ProfileViewModel>();
            builder.Services.AddTransient<ProfilePage>();
            builder.Services.AddTransient<ScanIngredientsPage>();
            builder.Services.AddTransient<AnalyzeIngredientsResultPage>();
            builder.Services.AddSingleton<IImagePickerService, ImagePickerService>(); 
            builder.Services.AddSingleton<ICombinedScanHistory, CombinedScanHistory>();

            builder.Services.AddSingleton<IScanHistoryStore, JsonScanHistoryStore>();
            builder.Services.AddSingleton<ICombinedScanHistory, CombinedScanHistory>();
            builder.Services.AddSingleton<IUserPreferenceStore, UserPreferenceStore>();
            // NEW: sample/demo product analysis lookup
            builder.Services.AddSingleton<ISampleAnalysisDataService, SampleIngredientAnalysisService>();
#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
