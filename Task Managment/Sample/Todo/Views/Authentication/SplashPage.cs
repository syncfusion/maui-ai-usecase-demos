using Todo.ViewModels;

namespace Todo.Views.Authentication;

/// <summary>
/// Code-only partial of <see cref="SplashPage"/> that adds the
/// dependency-injected constructor required by the .NET MAUI
/// Shell.  The XAML UI is inflated by <c>InitializeComponent()</c> in
/// <c>SplashPage.xaml.cs</c> and bound to the injected
/// <see cref="SplashPageViewModel"/> here.
///
/// Splitting the XAML-loaded UI (<c>SplashPage.xaml.cs</c>) and the
/// DI-injected constructor (<c>SplashPage.cs</c>) mirrors the convention
/// already used by <see cref="SignInPage"/> and <see cref="SignUpPage"/>
/// so the navigation surface stays uniform across the authentication
/// flow.
/// </summary>
public sealed partial class SplashPage : ContentPage
{
    private readonly SplashPageViewModel _viewModel;

    /// <summary>
    /// Initialises a new instance of the <see cref="SplashPage"/>
    /// class.
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
        _viewModel = viewModel;
        BindingContext = viewModel;
        Title = viewModel.Title;
    }

    /// <summary>
    /// Triggers the authentication check as soon as the page becomes
    /// visible.  Doing this in <see cref="Page.OnAppearing"/> (rather
    /// than in the constructor) guarantees that the platform visual
    /// tree is ready and that <c>Shell.Current</c> is available for
    /// navigation.
    /// </summary>
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.StartAsync();
    }
}
