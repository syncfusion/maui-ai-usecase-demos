using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;
using Todo.Common;
using Todo.Features.CustomLists.Services;
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

        // Common infrastructure
        builder.Services.AddSingleton<INavigationService, MauiNavigationService>();

        // Services
        builder.Services.AddSingleton<INoteService, InMemoryNoteService>();
        builder.Services.AddSingleton<IListService, InMemoryListService>();
        builder.Services.AddSingleton<Features.Search.Services.INoteSearchService, Features.Search.Services.InMemoryNoteSearchService>();

        // ViewModels — shared singleton across all four filter pages.
        // PR #37 review issue #13: the modal/dialog pages and their VMs
        // (EditNotePage, ConfirmDialogPage, CreateListDialogPage,
        // RenameListDialogPage, SearchResultsPage, PermanentDeleteDialog)
        // are always created with `new` by their callers, so they do
        // not need (and must not have) DI registrations. The sidebar
        // commands live on NotesListViewModel (issue #9 — the duplicate
        // CustomListsViewModel has been removed).
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
