using Microsoft.Extensions.Logging;
using Todo.Services.Infrastructure;
using Todo.Utilities;

namespace Todo;

/// <summary>
/// Application root.  Hosts the global resource dictionaries, the Syncfusion
/// theme dictionary and the <see cref="AppShell"/> used to compose the
/// navigation surface for the app.
///
/// In Task 1-06 (Navigation Setup) the <see cref="AppShell"/> is resolved
/// through the .NET MAUI dependency-injection container so that the
/// <see cref="AppShellViewModel"/> and <see cref="INavigationService"/> can
/// be injected into the Shell hierarchy.  An optional
/// <c>IAuthenticationService</c> is also resolved at startup time so the
/// launch flow can route an already-signed-in user straight to the
/// <see cref="Constants.Routes.TaskList"/> page instead of
/// <see cref="Constants.Routes.Splash"/>.
/// </summary>
public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;
    private readonly INavigationService _navigationService;
    private readonly ILogger<App> _logger;

    /// <summary>
    /// Initialises a new instance of the <see cref="App"/> class.  All
    /// dependencies are resolved through the .NET MAUI service provider
    /// (Task 1-05) so the <see cref="App"/> itself stays a thin composition
    /// root.
    /// </summary>
    /// <param name="shell">The application shell (Task 1-06).</param>
    /// <param name="serviceProvider">
    /// The application service provider, used to resolve optional services
    /// (for example <c>IAuthenticationService</c> when Module 2 ships).
    /// </param>
    /// <param name="navigationService">
    /// Navigation service injected by the DI container.
    /// </param>
    /// <param name="logger">Logger injected by the DI container.</param>
    public App(
        AppShell shell,
        IServiceProvider serviceProvider,
        INavigationService navigationService,
        ILogger<App> logger)
    {
        ArgumentNullException.ThrowIfNull(shell);
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(navigationService);
        ArgumentNullException.ThrowIfNull(logger);

        InitializeComponent();

        _serviceProvider = serviceProvider;
        _navigationService = navigationService;
        _logger = logger;

        MainPage = shell;
    }

    /// <summary>
    /// Creates the main application window on which the
    /// <see cref="AppShell"/> is hosted.
    /// </summary>
    /// <param name="activationState">The activation state, which may be null.</param>
    /// <returns>A new <see cref="Window"/> wrapping the <see cref="AppShell"/>.</returns>
    protected override Window CreateWindow(IActivationState? activationState)
    {
        Window window = base.CreateWindow(activationState);

        // Defer the launch-time navigation until the platform has finished
        // constructing the visual tree, otherwise Shell.Current is null and
        // every route registration attempt throws.
        window.Created += (_, _) => MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Task.Delay(Constants.Timeouts.UiDebounceMilliseconds)
                .ConfigureAwait(true);
            await InitializeNavigationAsync().ConfigureAwait(true);
        });

        return window;
    }

    /// <summary>
    /// Performs the launch-time navigation.  If the user is already
    /// authenticated, the app jumps straight to
    /// <see cref="Constants.Routes.TaskList"/>; otherwise it lands on
    /// <see cref="Constants.Routes.Splash"/>.
    /// </summary>
    private async Task InitializeNavigationAsync()
    {
        try
        {
            _logger.LogInformation("Initialising navigation at startup");

            bool isAuthenticated = await TryCheckAuthenticationAsync().ConfigureAwait(true);

            string target = isAuthenticated
                ? Constants.Routes.TaskList
                : Constants.Routes.Splash;

            // Use the string overload with clearStack=true so that the
            // back gesture cannot return the user to the previous page
            // (for example from the task list back to the splash page).
            await _navigationService.NavigateToAsync(target, clearStack: true)
                .ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Startup navigation failed; falling back to splash");
            await SafeNavigateToSplashAsync().ConfigureAwait(true);
        }
    }

    /// <summary>
    /// Resolves the optional authentication service and asks it whether the
    /// user is signed in.  Returns <see langword="false"/> when the service
    /// is not registered yet (the Authentication module ships later) or
    /// when it throws.
    /// </summary>
    private async Task<bool> TryCheckAuthenticationAsync()
    {
        // The IAuthenticationService interface is introduced by Module 2,
        // so resolve it reflectively to avoid a hard dependency from this
        // foundation task.
        Type? authServiceType = Type.GetType("Todo.Services.Authentication.IAuthenticationService, Todo")
            ?? AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType("Todo.Services.Authentication.IAuthenticationService"))
                .FirstOrDefault(t => t is not null);

        if (authServiceType is null)
        {
            _logger.LogDebug(
                "IAuthenticationService is not implemented yet; treating user as unauthenticated");
            return false;
        }

        object? authService = _serviceProvider.GetService(authServiceType);
        if (authService is null)
        {
            _logger.LogDebug(
                "IAuthenticationService is not registered; treating user as unauthenticated");
            return false;
        }

        System.Reflection.MethodInfo? method = authServiceType.GetMethod("IsAuthenticatedAsync");
        if (method is null)
        {
            return false;
        }

        object? result = method.Invoke(authService, Array.Empty<object>());
        if (result is Task<bool> boolTask)
        {
            return await boolTask.ConfigureAwait(true);
        }

        return false;
    }

    /// <summary>
    /// Last-resort navigation: route to the splash page using a direct
    /// <see cref="Shell"/> call so that the user always sees something.
    /// </summary>
    private static async Task SafeNavigateToSplashAsync()
    {
        if (Shell.Current is not null)
        {
            try
            {
                await Shell.Current.GoToAsync($"//{Constants.Routes.Splash}")
                    .ConfigureAwait(true);
            }
            catch
            {
                // Swallow — we cannot do anything useful at this point.
            }
        }
    }
}
