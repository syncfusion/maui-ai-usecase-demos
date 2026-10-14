using Todo.Common;
using Todo.Features.Notes.ViewModels;
using Todo.Features.Notes.Views;

namespace Todo.Features.Important;

/// <summary>
/// Shows only notes flagged as Important. Reuses the shared
/// <see cref="NotesListViewModel"/> via the central layout.
/// </summary>
public partial class ImportantPage : NotesFilterPageBase
{
    public ImportantPage(NotesListViewModel viewModel)
        : base(viewModel, NotesListCategory.Important)
    {
        InitializeComponent();
    }
}
