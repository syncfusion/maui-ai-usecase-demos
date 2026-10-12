using Todo.ViewModels;

namespace Todo.Views.Authentication;

/// <summary>
/// Code-only partial of <see cref="SignInPage"/> that adds the
/// dependency-injected constructor required by the .NET MAUI Shell.
/// The XAML UI is inflated by <c>InitializeComponent()</c> in
/// <c>SignInPage.xaml.cs</c> and bound to the injected
/// <see cref="SignInPageViewModel"/> here.
///
/// Splitting the XAML-loaded UI (<c>SignInPage.xaml.cs</c>) and the
/// DI-injected constructor (<c>SignInPage.cs</c>) mirrors the
/// convention already used by <see cref="SplashPage"/> so the
/// navigation surface stays uniform across the authentication flow.
/// </summary>
public sealed partial class SignInPage : ContentPage
{
    /// <summary>
    /// Initialises a new instance of the <see cref="SignInPage"/>
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
    public SignInPage(SignInPageViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        InitializeComponent();
        BindingContext = viewModel;
        Title = viewModel.Title;
    }
}