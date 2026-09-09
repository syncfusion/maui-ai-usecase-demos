namespace MediFlowSample.Controls;

/// <summary>Shared mobile bottom navigation bar, used by every top-level page so the markup and navigation logic live in one place.</summary>
public partial class AppBottomNavView : ContentView
{
    public static readonly BindableProperty ActiveRouteProperty = BindableProperty.Create(
        nameof(ActiveRoute), typeof(string), typeof(AppBottomNavView), string.Empty, propertyChanged: OnActiveRouteChanged);

    public AppBottomNavView()
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
        if (bindable is AppBottomNavView view)
        {
            view.ApplyActiveState((string)newValue);
        }
    }

    private void ApplyActiveState(string route)
    {
        SetItemState(DashboardIcon, DashboardLabel, route == "dashboard");
        SetItemState(PatientsIcon, PatientsLabel, route == "patients");
        SetItemState(ScheduleIcon, ScheduleLabel, route == "schedule");
        SetItemState(CarePlansIcon, CarePlansLabel, route == "careplans");
    }

    private static void SetItemState(Label icon, Label label, bool isActive)
    {
        var color = isActive ? (Color)Application.Current!.Resources["Primary"] : (Color)Application.Current!.Resources["Neutral"];
        icon.TextColor = color;
        label.TextColor = color;
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
