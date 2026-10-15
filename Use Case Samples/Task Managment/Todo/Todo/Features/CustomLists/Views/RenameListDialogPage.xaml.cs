using Todo.Features.CustomLists.ViewModels;

namespace Todo.Features.CustomLists.Views;

public partial class RenameListDialogPage : ContentPage
{
    public RenameListDialogPage(RenameListDialogViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}