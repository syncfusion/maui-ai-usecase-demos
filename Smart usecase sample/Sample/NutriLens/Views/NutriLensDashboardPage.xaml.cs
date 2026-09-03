using NutriLens.ViewModels;

namespace NutriLens.Views;

public partial class NutriLensDashboardPage : ContentPage
{
    public NutriLensDashboardPage(
        NutriLensDashboardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    public NutriLensDashboardPage()
        : this(new NutriLensDashboardViewModel())
    {
    }
}