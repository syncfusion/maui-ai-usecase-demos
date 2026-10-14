using Todo.Common;
using Todo.Features.Notes.ViewModels;
using Todo.Features.Notes.Views;

namespace Todo.Features.Reminder;

/// <summary>
/// Shows only notes with a reminder date. Reuses the shared
/// <see cref="NotesListViewModel"/> via the central layout.
/// </summary>
public partial class ReminderPage : NotesFilterPageBase
{
    public ReminderPage(NotesListViewModel viewModel)
        : base(viewModel, NotesListCategory.Reminder)
    {
        InitializeComponent();
    }
}
