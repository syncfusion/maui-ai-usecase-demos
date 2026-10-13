namespace Todo.Features.Notes.Views.Templates;

/// <summary>
/// Reusable sidebar navigation view. The view is intentionally free of
/// business logic; its bindings drive the shared
/// <see cref="Todo.Features.Notes.ViewModels.NotesListViewModel"/>.
/// </summary>
public partial class SidebarView : ContentView
{
    public SidebarView()
    {
        InitializeComponent();
    }
}
