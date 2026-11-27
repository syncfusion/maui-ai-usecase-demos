using DataForm_FormGenerator.Configuration;
using Microsoft.Extensions.Configuration;
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

            // Credentials (Endpoint / DeploymentName / ApiKey) come from the
            // "AzureOpenAI" section of the embedded appsettings.json. They are
            // validated on startup and on every generation; when invalid the
            // app shows the invalid key-details message and does NOT render
            // the form.
            builder.Services.AddSingleton(LoadAzureOpenAiOptions());
            builder.Services.AddSingleton<IAiSchemaService, AzureOpenAiSchemaService>();
            builder.Services.AddTransient<EducationFormViewModel>();


#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

        private static AzureOpenAiOptions LoadAzureOpenAiOptions()
        {
            var assembly = typeof(MauiProgram).Assembly;
            var resourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith("appsettings.json", StringComparison.OrdinalIgnoreCase));
            if (resourceName is null)
            {
                return new AzureOpenAiOptions();
            }

            using var stream = assembly.GetManifestResourceStream(resourceName)!;
            var configuration = new ConfigurationBuilder().AddJsonStream(stream).Build();
            var section = configuration.GetSection("AzureOpenAI");

            var options = new AzureOpenAiOptions();
            options.Endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")?.Trim() ?? string.Empty;
            options.ApiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY")?.Trim() ?? string.Empty;
            options.DeploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME")?.Trim() ?? string.Empty;
            return options;
        }
    }
}
