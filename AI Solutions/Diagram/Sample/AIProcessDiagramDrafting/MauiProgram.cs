using AIProcessDiagramDrafting.Services;
using AIProcessDiagramDrafting.ViewModels;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;

namespace AIProcessDiagramDrafting;

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

        builder.Services.AddSingleton<IWorkflowExtractionService>(_ => CreateWorkflowExtractionService());
        builder.Services.AddSingleton<WorkflowResponseValidator>();
        builder.Services.AddSingleton<DiagramDraftBuilder>();
        builder.Services.AddSingleton<ProcessDiagramViewModel>();
        builder.Services.AddSingleton<ProcessDiagramPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    private static IWorkflowExtractionService CreateWorkflowExtractionService()
    {
        var fakeMode = Environment.GetEnvironmentVariable("AI_PROCESS_DIAGRAM_FAKE_MODE");
        if (!string.IsNullOrWhiteSpace(fakeMode))
        {
            return new FakeWorkflowExtractionService(fakeMode);
        }

        if (HasAzureOpenAIConfiguration())
        {
            return new AzureOpenAIWorkflowExtractionService();
        }

        return new FakeWorkflowExtractionService();
    }

    private static bool HasAzureOpenAIConfiguration()
    {
        var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
        var deployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT");
        var apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");

        return !string.IsNullOrWhiteSpace(endpoint)
            && !string.IsNullOrWhiteSpace(deployment)
            && !string.IsNullOrWhiteSpace(apiKey);
    }
}
