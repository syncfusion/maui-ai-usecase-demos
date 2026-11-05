using WardrobeAI.ViewModels;

namespace WardrobeAI.Views.Shopping;

public partial class ShoppingPage : ContentPage
{
    private readonly ShoppingViewModel _viewModel;

    public ShoppingPage(ShoppingViewModel viewModel)
    {
        _viewModel = viewModel;
        BindingContext = viewModel;
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Pull closet + budget + style preferences and rebuild the page on
        // every visit so edits made in the Closet or Profile tabs are
        // reflected when the user comes back here.
        try
        {
            await _viewModel.LoadAsync();
        }
        catch (Exception ex)
        {
            // OnAppearing swallows exceptions; log so we don't crash on a
            // transient DB failure.
            System.Diagnostics.Debug.WriteLine($"ShoppingPage.OnAppearing failed: {ex}");
        }
    }

}
