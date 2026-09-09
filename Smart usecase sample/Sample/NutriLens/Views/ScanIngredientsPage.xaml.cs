using NutriLens.ViewModels;

namespace NutriLens.Views;

public partial class ScanIngredientsPage : ContentPage
{
    private CancellationTokenSource? animationCancellation;
    private ScanIngredientsViewModel? viewModel;

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