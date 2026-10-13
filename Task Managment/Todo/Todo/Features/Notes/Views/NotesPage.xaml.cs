using Todo.Common;
using Todo.Features.Notes.ViewModels;

namespace Todo.Features.Notes.Views;

/// <summary>
/// Single page that hosts the My Notes, Important, Reminder and Bin
/// filter views. All layout, navigation, and command binding lives in
/// the XAML; this code-behind is intentionally empty of business logic
/// and merely wires the page to the shared
/// <see cref="NotesListViewModel"/>.
/// </summary>
public partial class NotesPage : ContentPage
{
    /// <summary>
    /// DI-injected constructor — used at runtime so the shared
    /// <see cref="NotesListViewModel"/> is reused across navigation.
    /// </summary>
    public NotesPage(NotesListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    /// <summary>
    /// Parameter-less constructor used by XAML data templates and the
    /// MAUI design surface. Resolves the shared ViewModel via the
    /// <see cref="ServiceLocator"/>.
    /// </summary>
    public NotesPage() : this(new NotesListViewModel(ServiceLocator.Current))
    {
    }
}
