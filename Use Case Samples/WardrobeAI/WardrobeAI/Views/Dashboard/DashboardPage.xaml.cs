using WardrobeAI.ViewModels;

namespace WardrobeAI.Views.Dashboard;

/// <summary>
/// Dashboard page (SDS Module 2). Renders the /designs/dashboard.html mockup sections:
/// top app bar, greeting, today's AI suggestion, quick actions, AI insight, upcoming plans.
/// Upcoming plans are loaded from the database each time the page appears, so newly-planned
/// outfits show up immediately when the user navigates back from the Plan tab.
/// </summary>
public partial class DashboardPage : ContentPage
{
    private readonly DashboardViewModel _viewModel;

    public DashboardPage(DashboardViewModel viewModel)
    {
        _viewModel = viewModel;
        BindingContext = viewModel;
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Refresh the signed-in user's display name + greeting on every appearance
        // so a freshly-registered or re-onboarded user sees their real name.
        await _viewModel.LoadUserContextAsync();
        // Refresh the live weather chip (location + temp + icon) on every
        // appearance. The VM coalesces concurrent calls so rapid tab
        // switching doesn't fire two parallel location requests.
        await _viewModel.RefreshWeatherAsync();
        await _viewModel.LoadUpcomingPlansAsync();
        // Refresh today's AI-curated outfit suggestion. Uses the latest
        // weather + today's planned occasion + the user's closet. Coalesces
        // concurrent calls via IsOutfitLoading so the Generate New button
        // and OnAppearing can't double-bill Azure OpenAI.
        await _viewModel.RefreshTodaysOutfitAsync();
    }
}
