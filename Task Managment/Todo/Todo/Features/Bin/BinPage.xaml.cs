using Todo.Common;
using Todo.Features.Notes.ViewModels;
using Todo.Features.Notes.Views;

namespace Todo.Features.Bin;

/// <summary>
/// Shows soft-deleted notes. Reuses the shared
/// <see cref="NotesListViewModel"/> via the central layout.
/// </summary>
public partial class BinPage : NotesFilterPageBase
{
    public BinPage(NotesListViewModel viewModel)
        : base(viewModel, NotesListCategory.Bin)
    {
        InitializeComponent();
    }
}
