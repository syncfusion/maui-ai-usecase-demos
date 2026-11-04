using AIProcessDiagramDrafting.Services;
using AIProcessDiagramDrafting.ViewModels;
using Microsoft.Extensions.Logging;

namespace AIProcessDiagramDrafting;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton<IWorkflowExtractionService, WorkflowExtractionService>();
        builder.Services.AddSingleton<WorkflowResponseValidator>();
        builder.Services.AddSingleton<DiagramDraftBuilder>();
        builder.Services.AddSingleton<ProcessDiagramViewModel>();
        builder.Services.AddSingleton<ProcessDiagramPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
