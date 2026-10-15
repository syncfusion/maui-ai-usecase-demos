using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;
using Todo.Common;
using Todo.Features.CustomLists.Services;
using Todo.Features.CustomLists.ViewModels;
using Todo.Features.CustomLists.Views;
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

        // Note store is empty on startup by design (Module-02 spec).

        // Common infrastructure
        builder.Services.AddSingleton<INavigationService, MauiNavigationService>();

        // Services
        builder.Services.AddSingleton<INoteService, InMemoryNoteService>();
        builder.Services.AddSingleton<IListService, InMemoryListService>();

        // ViewModels — shared singleton across all four filter pages.
        builder.Services.AddSingleton<NotesListViewModel>();
        builder.Services.AddSingleton<CustomListsViewModel>();
        builder.Services.AddTransient<EditNoteViewModel>();
        builder.Services.AddTransient<ConfirmDialogViewModel>();
        builder.Services.AddTransient<CreateListDialogViewModel>();
        builder.Services.AddTransient<RenameListDialogViewModel>();

        // Pages (separate routes for My Notes, Important, Reminder, Bin)
        builder.Services.AddTransient<Features.Notes.Views.MyNotesPage>();
        builder.Services.AddTransient<Features.Important.ImportantPage>();
        builder.Services.AddTransient<Features.Reminder.ReminderPage>();
        builder.Services.AddTransient<Features.Bin.BinPage>();

        // Modal pages (Module-03 + Module-05)
        builder.Services.AddTransient<EditNotePage>();
        builder.Services.AddTransient<ConfirmDialogPage>();
        builder.Services.AddTransient<CreateListDialogPage>();
        builder.Services.AddTransient<RenameListDialogPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
