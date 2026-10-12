namespace Todo;

/// <summary>
/// Application root. Hosts the global resource dictionaries, the Syncfusion
/// theme dictionary and the <see cref="AppShell"/> used to compose the
/// navigation surface for the app.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class.
    /// </summary>
    public App()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Creates the main application window on which the <see cref="AppShell"/>
    /// is hosted.
    /// </summary>
    /// <param name="activationState">The activation state, which may be null.</param>
    /// <returns>A new <see cref="Window"/> wrapping the <see cref="AppShell"/>.</returns>
    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}
