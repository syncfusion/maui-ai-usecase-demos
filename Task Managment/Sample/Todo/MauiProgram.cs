using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;
using Todo.Services.Authentication;
using Todo.Services.Infrastructure;
using Todo.Services.Interfaces;
using Todo.Services.Search;
using Todo.Services.Storage;
using Todo.ViewModels;
using Todo.Views.Authentication;
using Todo.Views.Main;
using TaskService = Todo.Services.Task.TaskService;
using ListService = Todo.Services.Task.ListService;

namespace Todo;

/// <summary>
/// Bootstraps the .NET MAUI application.  This file is the single
/// composition root for the entire app and is the only place where concrete
/// types are bound to the .NET MAUI dependency-injection container.
///
/// The configuration is split into focused extension methods
/// (<see cref="AddServices"/>, <see cref="AddViewModels"/>,
/// <see cref="AddPages"/>, <see cref="AddLogging"/>) so the high-level
/// <see cref="CreateMauiApp"/> method reads as a single fluent pipeline.
/// This file also acts as the composition root for the
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
        var builder = MauiApp.CreateBuilder()
            .UseMauiApp<App>()
            .ConfigureSyncfusionCore()
            .AddServices()
            .AddViewModels()
            .AddPages()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.AddLogging();

        RegisterNavigationInfrastructure(builder);
        RegisterShellAndPages(builder);

        return builder.Build();
    }

    /// <summary>
    /// Registers every application service against its interface contract.
    /// Service lifetimes follow the convention documented in Task 1-05:
    /// <list type="bullet">
    ///   <item><description><b>Singleton</b> for stateless / process-wide
    ///     services such as <see cref="ILocalStorageService"/>.</description></item>
    ///   <item><description><b>Scoped</b> for per-request / per-user-action
    ///     services such as <see cref="IAuthenticationService"/> and
    ///     <see cref="ITaskService"/>.</description></item>
    /// </list>
    /// .NET MAUI does not create DI scopes per request in the way ASP.NET
    /// does, so <c>AddScoped</c> effectively behaves like <c>AddSingleton</c>
    /// for the lifetime of the application.  We still use the correct
    /// lifetime names so the registrations are portable to other hosts
    /// (background services, integration tests, etc.).
    /// </summary>
    /// <param name="builder">The <see cref="MauiAppBuilder"/> to extend.</param>
    /// <returns>The same <paramref name="builder"/> for fluent chaining.</returns>
    private static MauiAppBuilder AddServices(this MauiAppBuilder builder)
    {
        // --- Infrastructure -------------------------------------------------
        builder.Services.AddSingleton<INavigationService, NavigationService>();

        // --- Storage --------------------------------------------------------
        builder.Services.AddSingleton<ILocalStorageService, LocalStorageService>();

        // --- Feature services (placeholders — implementations land in
        //     later modules) -----------------------------------------------
        builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
        builder.Services.AddScoped<ITaskService, TaskService>();
        builder.Services.AddScoped<IListService, ListService>();
        builder.Services.AddScoped<ISearchService, SearchService>();

        return builder;
    }

    /// <summary>
    /// Registers every ViewModel.  ViewModels are split into two lifetimes:
    /// <list type="bullet">
    ///   <item><description><b>Singleton</b> for the
    ///     <see cref="AppShellViewModel"/>, which is bound to the
    ///     <see cref="AppShell"/> for the entire app lifetime.</description></item>
    ///   <item><description><b>Transient</b> for page ViewModels, so a fresh
    ///     state is created every time the user navigates to a page.</description></item>
    /// </list>
    /// </summary>
    /// <param name="builder">The <see cref="MauiAppBuilder"/> to extend.</param>
    /// <returns>The same <paramref name="builder"/> for fluent chaining.</returns>
    private static MauiAppBuilder AddViewModels(this MauiAppBuilder builder)
    {
        builder.Services
            .AddSingleton<AppShellViewModel>()
            .AddTransient<SplashPageViewModel>()
            .AddTransient<SignInPageViewModel>()
            .AddTransient<SignUpPageViewModel>()
            .AddTransient<TaskListPageViewModel>()
            .AddTransient<TaskDetailPageViewModel>();

        return builder;
    }

    /// <summary>
    /// Registers every <see cref="Page"/> with the .NET MAUI <see cref="Routing"/>
    /// system so they can be navigated to by route name and registers them with
    /// the DI container so they can be constructor-injected (which the
    /// Shell uses internally to materialise the bound ViewModel).
    /// </summary>
    /// <param name="builder">The <see cref="MauiAppBuilder"/> to extend.</param>
    /// <returns>The same <paramref name="builder"/> for fluent chaining.</returns>
    private static MauiAppBuilder AddPages(this MauiAppBuilder builder)
    {
        // Pages are registered with the DI container as transient so the
        // Shell can construct them via constructor injection (which in turn
        // resolves the bound ViewModel).
        builder.Services
            .AddTransient<SplashPage>()
            .AddTransient<SignInPage>()
            .AddTransient<SignUpPage>()
            .AddTransient<TaskListPage>()
            .AddTransient<TaskDetailPage>();

        // Shell routes — used by INavigationService and Shell.GoToAsync.
        // The full Shell flyout / tab layout that consumes these routes is
        // added in Task 1-06 (Navigation Setup).
        Routing.RegisterRoute("splash", typeof(SplashPage));
        Routing.RegisterRoute("signin", typeof(SignInPage));
        Routing.RegisterRoute("signup", typeof(SignUpPage));
        Routing.RegisterRoute("tasklist", typeof(TaskListPage));
        Routing.RegisterRoute("taskdetail", typeof(TaskDetailPage));

        return builder;
    }

    /// <summary>
    /// Configures logging.  The <c>Debug</c> provider is enabled in
    /// <c>DEBUG</c> builds so the developer console receives the same
    /// <see cref="ILogger{T}"/> diagnostics that the ViewModels emit.
    /// </summary>
    /// <param name="builder">The <see cref="MauiAppBuilder"/> to extend.</param>
    /// <returns>The same <paramref name="builder"/> for fluent chaining.</returns>
    private static MauiAppBuilder AddLogging(this MauiAppBuilder builder)
    {
#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder;
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
