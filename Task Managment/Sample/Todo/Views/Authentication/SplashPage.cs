using Microsoft.Extensions.DependencyInjection;
using Todo.ViewModels;

namespace Todo.Views.Authentication;

/// <summary>
/// Code-only partial of <see cref="SplashPage"/> that drives the
/// authentication launch flow.
///
/// <para>
/// The page is the very first content the Shell renders (see
/// <see cref="AppShell"/>).  The .NET MAUI Shell inflates its initial
/// <see cref="ShellContent"/> via the XAML <c>DataTemplate</c>
/// mechanism, which calls the parameterless constructor and bypasses
/// the dependency-injection container.  Later navigations (sign-out,
/// token-restored flow, …) re-create the page through DI and use the
/// other constructor.  Both paths converge in
/// <see cref="ResolveViewModel"/>: the DI-injected instance is used when
/// present, otherwise the ViewModel is resolved from the platform
/// service provider so the launch flow still fires.
/// </para>
///
/// <para>
/// Splitting the XAML-loaded UI (<c>SplashPage.xaml.cs</c>) and the
/// DI-injected constructor (<c>SplashPage.cs</c>) mirrors the convention
/// already used by <see cref="SignInPage"/> and <see cref="SignUpPage"/>
/// so the navigation surface stays uniform across the authentication
/// flow.
/// </para>
/// </summary>
public sealed partial class SplashPage : ContentPage
{
    private readonly SplashPageViewModel? _injectedViewModel;
    private SplashPageViewModel? _resolvedViewModel;

    /// <summary>
    /// Initialises a new instance of the <see cref="SplashPage"/>
    /// class.  Used by the .NET MAUI Shell when the page is resolved
    /// through <c>Shell.GoToAsync(...)</c>.
    /// </summary>
    /// <param name="viewModel">
    /// ViewModel injected by the DI container.  Resolved through the
    /// .NET MAUI service provider registered in
    /// <c>MauiProgram.AddPages</c>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="viewModel"/> is <see langword="null"/>.
    /// </exception>
    public SplashPage(SplashPageViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        InitializeComponent();
        _injectedViewModel = viewModel;
        BindingContext = viewModel;
        Title = viewModel.Title;
    }

    /// <summary>
    /// Resolves the <see cref="SplashPageViewModel"/> used to drive the
    /// launch flow.  Prefers the DI-injected instance when one is present;
    /// otherwise falls back to the platform service provider so the splash
    /// can still run even when Shell loaded the page through a parameterless
    /// constructor (the path taken by the initial <c>ShellContent</c>).
    /// </summary>
    private SplashPageViewModel ResolveViewModel()
    {
        if (_injectedViewModel is not null)
        {
            return _injectedViewModel;
        }

        if (_resolvedViewModel is not null)
        {
            return _resolvedViewModel;
        }

        IServiceProvider services = IPlatformApplication.Current?.Services
            ?? throw new InvalidOperationException(
                "Unable to resolve the SplashPageViewModel because no MAUI service provider is available.");

        SplashPageViewModel resolved = services.GetRequiredService<SplashPageViewModel>();
        _resolvedViewModel = resolved;
        BindingContext = resolved;
        Title = resolved.Title;
        return resolved;
    }

    /// <summary>
    /// Triggers the authentication check as soon as the page becomes
    /// visible.  Doing this in <see cref="Page.OnAppearing"/> (rather
    /// than in the constructor) guarantees that the platform visual
    /// tree is ready and that <c>Shell.Current</c> is available for
    /// navigation.  Idempotent: subsequent appearances (for example
    /// when the user backs out of an unauthenticated area) are no-ops
    /// because the ViewModel itself guards against re-entry.
    /// </summary>
    protected override void OnAppearing()
    {
        base.OnAppearing();

        SplashPageViewModel viewModel = ResolveViewModel();
        viewModel.StartAsync();
    }
}
