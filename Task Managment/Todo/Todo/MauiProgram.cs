using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;
using Todo.Common;
using Todo.Features.Notes.Services;
using Todo.Features.Notes.ViewModels;

namespace Todo;

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

        // Seed in-memory note store at app startup
        SampleData.EnsureSeeded();

        // Services
        builder.Services.AddSingleton<INoteService, InMemoryNoteService>();

        // ViewModels
        builder.Services.AddSingleton<NotesListViewModel>();

        // Pages
        builder.Services.AddTransient<Features.Notes.Views.NotesPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}