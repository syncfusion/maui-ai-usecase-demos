using Todo.Features.Dialogs.ViewModels;

namespace Todo.Features.Dialogs.Views;

public partial class ConfirmDialogPage : ContentPage
{
    public ConfirmDialogPage(ConfirmDialogViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
