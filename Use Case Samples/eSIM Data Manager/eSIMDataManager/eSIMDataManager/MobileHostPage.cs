using eSIMDataManager.ViewModels;

namespace eSIMDataManager;

/// <summary>
/// Shell-free host for the mobile targets. It swaps the visible tab page in place so no
/// Shell chrome (navigation bar, flyout header, tab bar) is rendered on the device.
/// </summary>
public sealed class MobileHostPage : ContentPage
{
    private readonly MainViewModel viewModel;
    private readonly Grid root = new();
    private readonly Frame contentHost = new() { BorderWidth = 0, Padding = 0, BackgroundColor = Colors.Transparent, HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Fill };

    public MobileHostPage(MainViewModel viewModel)
    {
        this.viewModel = viewModel;
        Title = string.Empty;
        BackgroundColor = Color.FromArgb("#F6F8FF");
        root.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        AppNavigation.GoToProfiles = () => ShowTab(MobilePage.MobileTab.Profiles);
        contentHost.Content = CreatePage(MobilePage.MobileTab.Dashboard);
        root.Add(contentHost, 0, 0);
        Content = root;
    }

    private MobilePage CreatePage(MobilePage.MobileTab target) => new(viewModel, target, ShowTab);

    private void ShowTab(MobilePage.MobileTab target) => contentHost.Content = CreatePage(target);
}
