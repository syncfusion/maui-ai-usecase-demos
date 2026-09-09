namespace MediFlowSample.Controls;

/// <summary>Shared desktop/tablet left navigation, used by every top-level page so the sidebar markup and navigation logic live in one place.</summary>
public partial class AppSidebarView : ContentView
{
    public static readonly BindableProperty ActiveRouteProperty = BindableProperty.Create(
        nameof(ActiveRoute), typeof(string), typeof(AppSidebarView), string.Empty, propertyChanged: OnActiveRouteChanged);

    public AppSidebarView()
    {
        InitializeComponent();
    }

    public string ActiveRoute
    {
        get => (string)GetValue(ActiveRouteProperty);
        set => SetValue(ActiveRouteProperty, value);
    }

    private static void OnActiveRouteChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is AppSidebarView view)
        {
            view.ApplyActiveState((string)newValue);
        }
    }

    private void ApplyActiveState(string route)
    {
        SetItemState(DashboardItem, DashboardIcon, DashboardLabel, route == "dashboard");
        SetItemState(PatientsItem, PatientsIcon, PatientsLabel, route == "patients");
        SetItemState(ScheduleItem, ScheduleIcon, ScheduleLabel, route == "schedule");
        SetItemState(CarePlansItem, CarePlansIcon, CarePlansLabel, route == "careplans");
    }

    private static void SetItemState(Grid item, Label icon, Label label, bool isActive)
    {
        item.BackgroundColor = isActive ? (Color)Application.Current!.Resources["Primary"] : Colors.Transparent;
        var color = isActive ? Colors.White : Color.FromArgb("#CBD5E1");
        icon.TextColor = color;
        label.TextColor = color;
        label.FontFamily = isActive ? "OpenSansSemibold" : null;
    }

    private async void OnDashboardTapped(object? sender, TappedEventArgs e) => await NavigateIfNeeded("dashboard");

    private async void OnPatientsTapped(object? sender, TappedEventArgs e) => await NavigateIfNeeded("patients");

    private async void OnScheduleTapped(object? sender, TappedEventArgs e) => await NavigateIfNeeded("schedule");

    private async void OnCarePlansTapped(object? sender, TappedEventArgs e) => await NavigateIfNeeded("careplans");

    private async Task NavigateIfNeeded(string route)
    {
        if (ActiveRoute != route)
        {
            await Shell.Current.GoToAsync($"//{route}");
        }
    }
}
