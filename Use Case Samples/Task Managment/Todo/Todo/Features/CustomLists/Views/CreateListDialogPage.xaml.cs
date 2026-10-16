using Todo.Features.CustomLists.ViewModels;

namespace Todo.Features.CustomLists.Views;

public partial class CreateListDialogPage : ContentPage
{
    public CreateListDialogPage(CreateListDialogViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}