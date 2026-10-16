using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using NutriLens.Helpers;

namespace NutriLens.Views;

public partial class DesktopHeaderNavigation : ContentView
{
    public static readonly BindableProperty CurrentPageProperty =
        BindableProperty.Create(
            nameof(CurrentPage),
            typeof(string),
            typeof(DesktopHeaderNavigation),
            "Dashboard",
            propertyChanged: OnCurrentPageChanged);

    public static readonly BindableProperty IsDesktopVisibleProperty =
        BindableProperty.Create(
            nameof(IsDesktopVisible),
            typeof(bool),
            typeof(DesktopHeaderNavigation),
            false);

    public string CurrentPage
    {
        get => (string)GetValue(CurrentPageProperty);
        set => SetValue(CurrentPageProperty, value);
    }

    public bool IsDesktopVisible
    {
        get => (bool)GetValue(IsDesktopVisibleProperty);
        private set => SetValue(IsDesktopVisibleProperty, value);
    }

    public ICommand NavigateDashboardCommand { get; }
    public ICommand NavigateHistoryCommand { get; }
    public ICommand NavigateTrendsCommand { get; }
    public ICommand NavigateProfileCommand { get; }

    public DesktopHeaderNavigation()
    {
        InitializeComponent();

        NavigateDashboardCommand =
            new AsyncRelayCommand(NavigateDashboardAsync);

        NavigateHistoryCommand =
            new AsyncRelayCommand(NavigateHistoryAsync);

        NavigateTrendsCommand =
            new AsyncRelayCommand(NavigateTrendsAsync);

        NavigateProfileCommand =
            new AsyncRelayCommand(NavigateProfileAsync);

        BindingContext = this;

        IsDesktopVisible =
            DeviceInfo.Platform == DevicePlatform.WinUI ||
            DeviceInfo.Platform == DevicePlatform.MacCatalyst;

        UpdateSelectedButton();
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        IsDesktopVisible =
            DeviceInfo.Platform == DevicePlatform.WinUI ||
            DeviceInfo.Platform == DevicePlatform.MacCatalyst;
    }

    private static void OnCurrentPageChanged(
        BindableObject bindable,
        object oldValue,
        object newValue)
    {
        if (bindable is DesktopHeaderNavigation navigation)
        {
            navigation.UpdateSelectedButton();
        }
    }

    private void UpdateSelectedButton()
    {
        if (DashboardButton is null ||
            HistoryButton is null ||
            TrendsButton is null ||
            ProfileButton is null)
        {
            return;
        }

        if (Resources["DesktopNavigationButtonStyle"] is not Style normalStyle ||
            Resources["SelectedDesktopNavigationButtonStyle"] is not Style selectedStyle)
        {
            return;
        }

        DashboardButton.Style = normalStyle;
        HistoryButton.Style = normalStyle;
        TrendsButton.Style = normalStyle;
        ProfileButton.Style = normalStyle;

        switch (CurrentPage)
        {
            case "History":
                HistoryButton.Style = selectedStyle;
                break;

            case "Trends":
                TrendsButton.Style = selectedStyle;
                break;

            case "Profile":
                ProfileButton.Style = selectedStyle;
                break;

            case "Dashboard":
            default:
                DashboardButton.Style = selectedStyle;
                break;
        }
    }

    private static Task NavigateDashboardAsync()
    {
        return AppNavigator.GoDashboardAsync();
    }

    private static Task NavigateHistoryAsync()
    {
        return AppNavigator.GoHistoryAsync();
    }

    private static Task NavigateTrendsAsync()
    {
        return AppNavigator.GoTrendAsync();
    }

    private static Task NavigateProfileAsync()
    {
        return AppNavigator.GoProfileAsync();
    }
}