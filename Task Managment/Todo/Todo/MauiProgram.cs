using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;
using Todo.Common;
using Todo.Features.Dialogs.ViewModels;
using Todo.Features.Dialogs.Views;
using Todo.Features.EditNote.ViewModels;
using Todo.Features.EditNote.Views;
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

        // Note store seeds the canonical tasks (per images/important.png,
        // images/remainder.png and images/bin.png) so Important, Reminder
        // and Bin show the design content on first launch, while My Notes
        // stays empty (the seeded notes are soft-deleted). See SampleData
        // and InMemoryNoteService for details.

        // Common infrastructure
        builder.Services.AddSingleton<INavigationService, MauiNavigationService>();

        // Services
        builder.Services.AddSingleton<INoteService, InMemoryNoteService>();

        // ViewModels — shared singleton across all four filter pages.
        builder.Services.AddSingleton<NotesListViewModel>();
        builder.Services.AddTransient<EditNoteViewModel>();
        builder.Services.AddTransient<ConfirmDialogViewModel>();

        // Pages (separate routes for My Notes, Important, Reminder, Bin)
        builder.Services.AddTransient<Features.Notes.Views.MyNotesPage>();
        builder.Services.AddTransient<Features.Important.ImportantPage>();
        builder.Services.AddTransient<Features.Reminder.ReminderPage>();
        builder.Services.AddTransient<Features.Bin.BinPage>();

        // Modal pages (Module-03)
        builder.Services.AddTransient<EditNotePage>();
        builder.Services.AddTransient<ConfirmDialogPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
