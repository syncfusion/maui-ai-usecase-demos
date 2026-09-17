using PatientCompanion.Views;

#if WINDOWS
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinAutomationProperties = Microsoft.UI.Xaml.Automation.AutomationProperties;
using WinVisibility = Microsoft.UI.Xaml.Visibility;
#endif

namespace PatientCompanion;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

#if WINDOWS || ANDROID
        Shell.SetTabBarIsVisible(this, false);
#endif

#if WINDOWS
        HandlerChanged += OnWindowsHandlerChanged;
    Navigated += OnWindowsShellNavigated;
#endif

        Routing.RegisterRoute(nameof(AppointmentPage), typeof(AppointmentPage));
    }

#if WINDOWS
    private void OnWindowsHandlerChanged(object? sender, EventArgs e)
    {
        if (Handler?.PlatformView is FrameworkElement platformView)
            platformView.Loaded += OnWindowsPlatformViewLoaded;

        Dispatcher.Dispatch(() =>
        {
            HideWindowsShellNavigationButtons();
            Dispatcher.StartTimer(TimeSpan.FromMilliseconds(250), () =>
            {
                HideWindowsShellNavigationButtons();
                return false;
            });
        });
    }

    private void OnWindowsPlatformViewLoaded(object sender, RoutedEventArgs e)
    {
        HideWindowsShellNavigationButtons();
    }

    private void OnWindowsShellNavigated(object? sender, ShellNavigatedEventArgs e)
    {
        Dispatcher.Dispatch(HideWindowsShellNavigationButtons);
    }

    private void HideWindowsShellNavigationButtons()
    {
        if (Handler?.PlatformView is not FrameworkElement platformView)
            return;

        HideWindowsOverflowButtons(platformView);

        var navigationView = FindNavigationView(platformView);
        if (navigationView == null)
            return;

        navigationView.IsPaneToggleButtonVisible = false;
        navigationView.IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed;
    }

    private static void HideWindowsOverflowButtons(DependencyObject element)
    {
        if (element is CommandBar commandBar)
        {
            var overflowProperty = commandBar.GetType().GetProperty("OverflowButtonVisibility");
            if (overflowProperty?.PropertyType.IsEnum == true)
            {
                var collapsed = Enum.Parse(overflowProperty.PropertyType, "Collapsed");
                overflowProperty.SetValue(commandBar, collapsed);
            }
        }

        if (element is FrameworkElement frameworkElement)
        {
            var name = frameworkElement.Name ?? string.Empty;
            var automationName = WinAutomationProperties.GetName(frameworkElement) ?? string.Empty;
            var helpText = WinAutomationProperties.GetHelpText(frameworkElement) ?? string.Empty;

            if (name.Contains("Overflow", StringComparison.OrdinalIgnoreCase) ||
                name.Contains("More", StringComparison.OrdinalIgnoreCase) ||
                automationName.Contains("More", StringComparison.OrdinalIgnoreCase) ||
                automationName.Contains("Overflow", StringComparison.OrdinalIgnoreCase) ||
                helpText.Contains("More", StringComparison.OrdinalIgnoreCase) ||
                helpText.Contains("Overflow", StringComparison.OrdinalIgnoreCase))
            {
                frameworkElement.Visibility = WinVisibility.Collapsed;
                return;
            }
        }

        var childCount = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(element);
        for (var index = 0; index < childCount; index++)
        {
            HideWindowsOverflowButtons(
                Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChild(element, index));
        }
    }

    private static NavigationView? FindNavigationView(DependencyObject element)
    {
        if (element is NavigationView navigationView)
            return navigationView;

        var childCount = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(element);
        for (var index = 0; index < childCount; index++)
        {
            var child = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChild(element, index);
            var result = FindNavigationView(child);
            if (result != null)
                return result;
        }

        return null;
    }
#endif
}