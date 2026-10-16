using SmartVehicleCare.Helpers;

namespace SmartVehicleCare.Views;

public partial class SplashPage : ContentPage
{
    private readonly ConstellationDrawable _drawable = new();

    public SplashPage()
    {
        InitializeComponent();
        ConstellationView.Drawable = _drawable;
        ApplyConstellationTheme();

        if (Application.Current != null)
            Application.Current.RequestedThemeChanged += OnThemeChanged;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = RunSplashSequenceAsync();
    }

    private async Task RunSplashSequenceAsync()
    {
        // Brief staged progress so the splash feels animated without delaying startup (~1.2s total)
        InitProgress.Progress = 30;
        await Task.Delay(400);

        InitProgress.Progress = 60;
        await Task.Delay(350);

        InitProgress.Progress = 80;
        await Task.Delay(250);

        InitProgress.Progress = 100;
        await Task.Delay(200);

        // Navigate to welcome
        if (Application.Current != null)
            Application.Current.RequestedThemeChanged -= OnThemeChanged;

        await Shell.Current.GoToAsync("///welcome");
    }

    private void OnThemeChanged(object? sender, AppThemeChangedEventArgs e)
    {
        ApplyConstellationTheme();
        ConstellationView.Invalidate();
    }

    private void ApplyConstellationTheme()
    {
        bool isDark = Application.Current?.RequestedTheme != AppTheme.Light;

        _drawable.LineColor = isDark
            ? Color.FromArgb("#25FFFFFF")
            : Color.FromArgb("#303B82F6");

        _drawable.DotColor = isDark
            ? Color.FromArgb("#40FFFFFF")
            : Color.FromArgb("#453B5BDB");
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        if (Application.Current != null)
            Application.Current.RequestedThemeChanged -= OnThemeChanged;
    }
}
