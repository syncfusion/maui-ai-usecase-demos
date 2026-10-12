using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;
using Todo.Services.Infrastructure;
using Todo.ViewModels;
using Todo.Views.Authentication;
using Todo.Views.Main;

namespace Todo;

/// <summary>
/// Bootstraps the .NET MAUI application, configures the Syncfusion MAUI Core
/// runtime, registers the default fonts and (in DEBUG) the debug logging
/// provider.  This file also acts as the composition root for the
/// navigation infrastructure introduced in Task 1-06 (Navigation Setup).
/// </summary>
public static class MauiProgram
{
    /// <summary>
    /// Creates the <see cref="MauiApp"/> used to start the host process.
    /// </summary>
    /// <returns>The configured <see cref="MauiApp"/> instance.</returns>
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureSyncfusionCore()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        RegisterNavigationInfrastructure(builder);
        RegisterShellAndPages(builder);

        return builder.Build();
    }

    /// <summary>
    /// Registers the navigation service and the Shell-level ViewModel.
    /// Both are singletons because they hold process-wide state.
    /// </summary>
    /// <param name="builder">The host builder being configured.</param>
    private static void RegisterNavigationInfrastructure(MauiAppBuilder builder)
    {
        // NavigationService is a singleton: it is stateless other than the
        // injected logger, but the ViewModel layer depends on it through
        // constructor injection, so every resolved instance must be the
        // same one for the application lifetime.
        builder.Services.AddSingleton<INavigationService, NavigationService>();

        // AppShellViewModel owns the Shell-level commands (back, sign-out,
        // …) and is bound to the Shell's BindingContext.
        builder.Services.AddSingleton<AppShellViewModel>();
    }

    /// <summary>
    /// Registers the <see cref="AppShell"/> and every page that can be
    /// reached through the navigation service.  Transient lifetime is used
    /// for pages so the navigation stack can keep a fresh instance per
    /// visit (and so DI can inject a ViewModel into each page).
    /// </summary>
    /// <param name="builder">The host builder being configured.</param>
    private static void RegisterShellAndPages(MauiAppBuilder builder)
    {
        builder.Services.AddTransient<AppShell>();

        // Authentication flow.
        builder.Services.AddTransient<SplashPage>();
        builder.Services.AddTransient<SignInPage>();
        builder.Services.AddTransient<SignUpPage>();
        builder.Services.AddTransient<ForgotPasswordPage>();

        // Main app flow.
        builder.Services.AddTransient<TaskListPage>();
        builder.Services.AddTransient<TaskDetailPage>();
        builder.Services.AddTransient<EditTaskPage>();
        builder.Services.AddTransient<SearchResultsPage>();
        builder.Services.AddTransient<ProfilePage>();
        builder.Services.AddTransient<SettingsPage>();
    }
}
