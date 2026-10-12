using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;

namespace Todo;

/// <summary>
/// Application root.  Hosts the global resource dictionaries, the Syncfusion
/// theme dictionary and the <see cref="AppShell"/> used to compose the
/// navigation surface for the app.
///
/// The launch flow is driven by the <see cref="AppShell"/>'s initial
/// <see cref="ShellContent"/> (the <see cref="Views.Authentication.SplashPage"/>)
/// which inspects the persisted authentication state and forwards the user
/// to either the sign-in screen or the main task list.  This class therefore
/// only needs to wire the Shell into the <see cref="Application.MainPage"/>
/// and hand the resolved services to the Shell hierarchy.
/// </summary>
public partial class App : Application
{
    private readonly ILogger<App> _logger;

    /// <summary>
    /// Initialises a new instance of the <see cref="App"/> class.  All
    /// dependencies are resolved through the .NET MAUI service provider
    /// (Task 1-05) so the <see cref="App"/> itself stays a thin composition
    /// root.
    /// </summary>
    /// <param name="shell">The application shell (Task 1-06).</param>
    /// <param name="logger">Logger injected by the DI container.</param>
    public App(
        AppShell shell,
        ILogger<App> logger)
    {
        ArgumentNullException.ThrowIfNull(shell);
        ArgumentNullException.ThrowIfNull(logger);

        InitializeComponent();

        _logger = logger;

        MainPage = shell;
    }

    /// <summary>
    /// Creates the main application window on which the
    /// <see cref="AppShell"/> is hosted.  The Shell's initial content is
    /// the <see cref="Views.Authentication.SplashPage"/> which kicks off
    /// the launch-time session restore the first time it appears.
    /// </summary>
    /// <param name="activationState">The activation state, which may be null.</param>
    /// <returns>A new <see cref="Window"/> wrapping the <see cref="AppShell"/>.</returns>
    protected override Window CreateWindow(IActivationState? activationState)
    {
        Window window = base.CreateWindow(activationState);

        _logger.LogInformation("Application window created; splash page will handle launch navigation");
        return window;
    }
}