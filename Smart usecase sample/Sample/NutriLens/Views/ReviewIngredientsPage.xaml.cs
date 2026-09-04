using NutriLens.ViewModels;

namespace NutriLens.Views;

public partial class ReviewIngredientsPage : ContentPage
{
    public ReviewIngredientsPage()
    {
        InitializeComponent();
        BindingContext = new ReviewIngredientsViewModel();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ReviewIngredientsViewModel vm)
            vm.HydrateFromPendingReview();
    }
}