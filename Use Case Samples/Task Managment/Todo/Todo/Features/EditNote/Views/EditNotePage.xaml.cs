using Todo.Features.EditNote.ViewModels;

namespace Todo.Features.EditNote.Views;

public partial class EditNotePage : ContentPage
{
    public EditNotePage(EditNoteViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
