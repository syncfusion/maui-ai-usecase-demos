using DataForm_FormGenerator.Configuration;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;

namespace DataForm_FormGenerator
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

            // Credentials (Endpoint / DeploymentName / ApiKey) are configured
            // directly in AzureOpenAiOptions. They are validated on every
            // "Generate Form" call; when invalid the app shows the invalid
            // key-details message and does NOT render the form.
            builder.Services.AddSingleton(new AzureOpenAiOptions());
            builder.Services.AddSingleton<IAiSchemaService, AzureOpenAiSchemaService>();
            builder.Services.AddTransient<EducationFormViewModel>();


#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
