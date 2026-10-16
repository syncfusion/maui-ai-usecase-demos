using Todo.Common;
using Todo.Features.Notes.ViewModels;

namespace Todo.Features.Notes.Views;

/// <summary>
/// Default landing page — shows the user's notes. Built on top of the
/// shared <see cref="NotesFilterPageBase"/> so it reuses the same
/// <see cref="NotesListViewModel"/>, header, sidebar, and content view
/// as the other filter pages.
/// </summary>
public partial class MyNotesPage : NotesFilterPageBase
{
    public MyNotesPage(NotesListViewModel viewModel)
        : base(viewModel, NotesListCategory.MyNotes)
    {
        InitializeComponent();
    }
}
