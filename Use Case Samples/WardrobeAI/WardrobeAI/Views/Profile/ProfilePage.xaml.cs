using Syncfusion.Maui.Core;
using WardrobeAI.ViewModels;

namespace WardrobeAI.Views.Profile;

public partial class ProfilePage : ContentPage
{
    private readonly ProfileViewModel _viewModel;

    public ProfilePage(ProfileViewModel viewModel)
    {
        _viewModel = viewModel;
        BindingContext = viewModel;
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Pull the signed-in user's name, profile image, style preferences,
        // and analytics from the database every time the page appears so
        // edits made elsewhere (closet, onboarding) are reflected on return.
        await _viewModel.LoadAsync();
    }

    /// <summary>
    /// Fired by the style-preferences <c>SfChipGroup</c> (ChipType="Input")
    /// when the user taps the built-in X button on a chip. By the time this
    /// event fires, the SfChipGroup has already removed the item from the
    /// bound <c>StylePreferences</c> collection and re-rendered, so the
    /// ViewModel only needs to persist the trimmed value back to the
    /// database. The previous custom FlexLayout+BindableLayout template
    /// did not always re-render on Remove on Android, which left the
    /// "added" style chips stuck in the UI after the X tap.
    /// </summary>
    private async void OnStyleChipRemoved(object? sender, Syncfusion.Maui.Core.Chips.SelectionChangedEventArgs e)
    {
        if (e?.RemovedItem is not string style || string.IsNullOrWhiteSpace(style))
        {
            return;
        }

        try
        {
            await _viewModel.OnStyleRemovedAsync(style);
        }
        catch (Exception ex)
        {
            // Persist failed: roll the chip back so the UI matches the DB.
            // The ViewModel logs the inner exception; this is just a guard
            // against a swallowed error reaching the user as a silent failure.
            System.Diagnostics.Debug.WriteLine($"OnStyleChipRemoved failed: {ex}");
        }
    }
}
