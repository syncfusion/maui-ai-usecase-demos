using Todo.Common;
using Todo.Features.Bin.Views;
using Todo.Features.Notes.Models;
using Todo.Features.Notes.ViewModels;

namespace Todo.Features.Notes.Views;

public partial class NotesPage : ContentPage
{
    private readonly NotesListViewModel _viewModel;

    /// <summary>
    /// DI-injected constructor — used at runtime so the shared
    /// <see cref="NotesListViewModel"/> is reused across navigation.
    /// </summary>
    public NotesPage(NotesListViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;

        // Wire the permanent-delete confirmation flow without leaking
        // any view logic into the ViewModel: the VM exposes a hook
        // (Func<Note, Task<bool>>) that this page satisfies by
        // showing the design-matched dialog.
        _viewModel.ConfirmDeletePermanently = ConfirmDeletePermanentlyAsync;
    }

    /// <summary>
    /// Parameter-less constructor used by XAML data templates and the
    /// MAUI design surface. Resolves the shared ViewModel via the
    /// <see cref="ServiceLocator"/>.
    /// </summary>
    public NotesPage() : this(new NotesListViewModel(ServiceLocator.Current))
    {
    }

    private async Task<bool> ConfirmDeletePermanentlyAsync(Note note)
    {
        // Show the design-matched modal dialog and await the user's
        // decision. The dialog already owns its own page so this is
        // a one-line, view-only orchestration.
        return await PermanentDeleteDialog.ShowAsync(Navigation);
    }
}
