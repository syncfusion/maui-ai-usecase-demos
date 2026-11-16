using eSIMDataManager.ViewModels;

namespace eSIMDataManager;

/// <summary>
/// Shell-free host for the mobile targets. It hosts the mobile page directly and switches
/// tabs in place, so no Shell chrome (navigation bar, flyout header, tab bar) is rendered.
/// </summary>
public sealed class MobileHostPage : ContentPage
{
    private readonly MobilePage mobilePage;

    public MobileHostPage(MainViewModel viewModel)
    {
        Title = string.Empty;
        BackgroundColor = Color.FromArgb("#F6F8FF");
        mobilePage = new MobilePage(viewModel, MobilePage.MobileTab.Dashboard, ShowTab);
        AppNavigation.GoToProfiles = () => ShowTab(MobilePage.MobileTab.Profiles);
        Content = mobilePage;
    }

    private void ShowTab(MobilePage.MobileTab target) => mobilePage.ShowTab(target);
}
