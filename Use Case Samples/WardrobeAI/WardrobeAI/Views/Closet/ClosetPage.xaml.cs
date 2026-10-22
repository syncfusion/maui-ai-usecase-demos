using WardrobeAI.ViewModels;

namespace WardrobeAI.Views.Closet;

public partial class ClosetPage : ContentPage
{
    private readonly ClosetViewModel _viewModel;

    public ClosetPage(ClosetViewModel viewModel)
    {
        _viewModel = viewModel;
        BindingContext = viewModel;
        InitializeComponent();

        // Initialize once the layout has a real size.
        ClothingFlex.SizeChanged += ClothingFlexOnSizeChanged;
    }

    private void ClothingFlexOnSizeChanged(object? sender, EventArgs e)
    {
        // Use the FlexLayout's measured width so the cards grow/shrink
        // as the user resizes the screen.
        _viewModel.UpdateClosetCardSizes(ClothingFlex.Width);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Ensure we have at least one sizing pass.
        _viewModel.UpdateClosetCardSizes(ClothingFlex.Width);

        // Re-load on every appear so newly added items (from the popup) and
        // any background changes surface on returning to the tab. The VM is
        // idempotent — LoadClothingAsync clears + repopulates the collection.
        await _viewModel.LoadClothingAsync();
    }

    /// <summary>
    /// Triggered by the top-of-page <c>SfChipGroup</c> filter row when the
    /// user picks a category. The two-way <c>SelectedItem</c> binding keeps
    /// <see cref="ClosetViewModel.SelectedFilter"/> in sync; calling the
    /// existing <c>SelectFilterCommand</c> here makes sure the same
    /// code path that used to run for the SfButton-based chips still
    /// runs (toggles <c>IsSelected</c> on the DTOs and refreshes
    /// <c>FilteredClothing</c>).
    /// </summary>
    private void OnFilterSelectionChanged(object? sender, Syncfusion.Maui.Core.Chips.SelectionChangedEventArgs e)
    {
        if (_viewModel.SelectFilterCommand.CanExecute(null))
        {
            _viewModel.SelectFilterCommand.Execute(null);
        }
    }

    private async void OnPointerEntered(object sender, PointerEventArgs e)
    {
        if (sender is Image image)
        {
            await image.ScaleToAsync(1.1, 150, Easing.CubicOut);
        }
    }

    private async void OnPointerExited(object sender, PointerEventArgs e)
    {
        if (sender is Image image)
        {
            await image.ScaleToAsync(1.0, 150, Easing.CubicOut);
        }
    }

    private async void ImageTapped(object sender, TappedEventArgs e)
    {
        if (sender is Image image)
        {
            await image.ScaleToAsync(1.15, 150, Easing.CubicOut);
            await Task.Delay(300);
            await image.ScaleToAsync(1.0, 150, Easing.CubicIn);
        }
    }
}
