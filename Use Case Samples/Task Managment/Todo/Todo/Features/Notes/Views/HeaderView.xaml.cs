using System.Windows.Input;
using Todo.Features.Notes.ViewModels;

namespace Todo.Features.Notes.Views;

public partial class HeaderView : ContentView
{
    public HeaderView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Forwards a tap on the header search entry to the bound
    /// ViewModel's <see cref="NotesListViewModel.OpenSearchCommand"/>.
    /// The entry is read-only and acts as a button surrogate so the
    /// tap on the pill also opens the Search page (Module-06).
    /// We immediately drop focus so the keyboard does not appear
    /// and the entry does not get a stuck focus highlight after
    /// navigation (Module-08 task 8-2).
    /// </summary>
    private void OnSearchEntryFocused(object? sender, FocusEventArgs e)
    {
        if (sender is Entry entry)
            entry.Unfocus();

        if (BindingContext is not NotesListViewModel vm) return;
        if (vm.OpenSearchCommand is ICommand cmd && cmd.CanExecute(null))
        {
            cmd.Execute(null);
        }
    }
}
