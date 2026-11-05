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

}
