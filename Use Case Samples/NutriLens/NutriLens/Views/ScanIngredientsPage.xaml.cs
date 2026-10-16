using NutriLens.ViewModels;

namespace NutriLens.Views;

public partial class ScanIngredientsPage : ContentPage
{
    public ScanIngredientsPage()
    {
        InitializeComponent();
        BindingContext = new ScanIngredientsViewModel();
    }

    public ScanIngredientsPage(ScanIngredientsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    } 
}