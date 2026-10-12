using Todo.ViewModels;
using Todo.Views.Authentication;
using Todo.Views.Main;
using Todo.Utilities;

namespace Todo;

/// <summary>
/// Application shell.  Hosts the <see cref="SplashPage"/> as its initial
/// <see cref="ShellContent"/> and registers every other page as a Shell
/// route so that the <see cref="INavigationService"/> can reach them
/// through <c>Shell.GoToAsync</c>.
///
/// Route registration is performed in the constructor (rather than in
/// <c>MauiProgram</c>) because <c>Routing.RegisterRoute</c> requires the
/// Shell instance to be alive — by the time the Shell is constructed the
/// DI container has already resolved every service and every page type.
/// </summary>
public partial class AppShell : Shell
{
    /// <summary>
    /// Initialises a new instance of the <see cref="AppShell"/> class,
    /// loads the XAML and registers every navigation route.
    /// </summary>
    /// <param name="shellViewModel">
    /// The ViewModel bound to the Shell.  Resolved by the DI container.
    /// </param>
    public AppShell(AppShellViewModel shellViewModel)
    {
        ArgumentNullException.ThrowIfNull(shellViewModel);

        InitializeComponent();

        // Bind the ViewModel so the [RelayCommand]s exposed by it are
        // available to the Shell chrome (back button, flyout items, etc.).
        BindingContext = shellViewModel;

        RegisterRoutes();

        // Refresh the cached "can go back" state every time the Shell
        // completes a navigation.  This keeps the bound command in sync
        // with the actual navigation stack.
        Navigated += (_, _) => shellViewModel.RefreshNavigationState();
    }

    /// <summary>
    /// Registers every page-type-to-route-key mapping with the Shell's
    /// global routing table.  Both the Shell's
    /// <c>Routing.RegisterRoute(route, type)</c> and the local
    /// <see cref="RegisteredPages"/> map are populated so that the
    /// strongly-typed <c>NavigationService.NavigateToAsync&lt;TPage&gt;</c>
    /// overload can resolve routes at runtime.
    /// </summary>
    private static void RegisterRoutes()
    {
        // Authentication flow.
        RegisterRoute(Constants.Routes.Splash,           typeof(SplashPage));
        RegisterRoute(Constants.Routes.SignIn,           typeof(SignInPage));
        RegisterRoute(Constants.Routes.SignUp,           typeof(SignUpPage));
        RegisterRoute(Constants.Routes.ForgotPassword,   typeof(ForgotPasswordPage));

        // Main app flow.
        RegisterRoute(Constants.Routes.TaskList,         typeof(TaskListPage));
        RegisterRoute(Constants.Routes.TaskDetail,       typeof(TaskDetailPage));
        RegisterRoute(Constants.Routes.EditTask,         typeof(EditTaskPage));
        RegisterRoute(Constants.Routes.Search,           typeof(SearchResultsPage));
        RegisterRoute(Constants.Routes.Profile,          typeof(ProfilePage));
        RegisterRoute(Constants.Routes.Settings,         typeof(SettingsPage));
    }

    /// <summary>
    /// Registers a single page-type / route-key pair with both the Shell
    /// router and the <see cref="RegisteredPages"/> reverse map.
    /// </summary>
    /// <param name="route">The Shell route key.</param>
    /// <param name="pageType">The page type that the route resolves to.</param>
    private static void RegisterRoute(string route, Type pageType)
    {
        Routing.RegisterRoute(route, pageType);
        RegisteredPages.Register(pageType, route);
    }
}
