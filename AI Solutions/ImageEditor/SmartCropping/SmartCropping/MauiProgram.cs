using Microsoft.Extensions.Logging;
using SmartCropping.ViewModel;
using Syncfusion.Maui.Core.Hosting;

namespace SmartCropping;

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

        builder.Services.AddSingleton<ISmartCropService>(serviceProvider =>
        {
            // Switch between the Azure OpenAI implementation and the new
            // Azure AI Vision implementation at compile time. Flip this
            // constant to compare the two engines against the same image
            // without touching the view or the view model.
            const bool UseAzureVision = true;

            if (UseAzureVision)
            {
                string? visionEndpoint = Environment.GetEnvironmentVariable("AZURE_VISION_ENDPOINT");
                string? visionKey = Environment.GetEnvironmentVariable("AZURE_VISION_KEY");

                if (string.IsNullOrWhiteSpace(visionEndpoint) || string.IsNullOrWhiteSpace(visionKey))
                {
                    // Fall back to the OpenAI implementation until credentials
                    // are configured so the sample still runs.
                    return new SmartCropService();
                }

                return new AzureVisionSmartCropService(visionEndpoint!, visionKey!);
            }

            return new SmartCropService();
        });

        // One view model per request; the shell picks the desktop or mobile
        // page and resolves it from the same singleton service.
        builder.Services.AddTransient<SmartCroppingViewModel>();

        builder.Services.AddTransient<SmartCropping>();
        builder.Services.AddTransient<SmartCroppingMobileView>();

        builder.Services.AddSingleton<AppShell>();


#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}