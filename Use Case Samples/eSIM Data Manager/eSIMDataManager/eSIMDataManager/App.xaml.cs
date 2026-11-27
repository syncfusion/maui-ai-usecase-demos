namespace eSIMDataManager;

public partial class App : Application
{
    private readonly AppShell appShell;

    public App(AppShell appShell)
    {
        InitializeComponent();
        this.appShell = appShell;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(appShell);
        window.MinimumWidth = 360;
        window.MinimumHeight = 600;
        return window;
    }
}
