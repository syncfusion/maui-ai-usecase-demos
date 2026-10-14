using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;
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

        // Note store is empty on startup by design (Module-02 spec).

        // Services
        builder.Services.AddSingleton<INoteService, InMemoryNoteService>();

        // ViewModels — shared singleton across all four filter pages.
        builder.Services.AddSingleton<NotesListViewModel>();

        // Pages (separate routes for My Notes, Important, Reminder, Bin)
        builder.Services.AddTransient<Features.Notes.Views.MyNotesPage>();
        builder.Services.AddTransient<Features.Important.ImportantPage>();
        builder.Services.AddTransient<Features.Reminder.ReminderPage>();
        builder.Services.AddTransient<Features.Bin.BinPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
