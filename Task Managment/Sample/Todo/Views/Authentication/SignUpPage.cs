using Todo.ViewModels;

namespace Todo.Views.Authentication;

/// <summary>
/// Code-only partial of <see cref="SignUpPage"/> that adds the
/// dependency-injected constructor required by the .NET MAUI Shell.
/// The XAML UI is inflated by <c>InitializeComponent()</c> in
/// <c>SignUpPage.xaml.cs</c> and bound to the injected
/// <see cref="SignUpPageViewModel"/> here.
///
/// Splitting the XAML-loaded UI (<c>SignUpPage.xaml.cs</c>) and the
/// DI-injected constructor (<c>SignUpPage.cs</c>) mirrors the
/// convention already used by <see cref="SplashPage"/> and
/// <see cref="SignInPage"/> so the navigation surface stays uniform
/// across the authentication flow.
/// </summary>
public sealed partial class SignUpPage : ContentPage
{
    /// <summary>
    /// Initialises a new instance of the <see cref="SignUpPage"/>
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
    public SignUpPage(SignUpPageViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        InitializeComponent();
        BindingContext = viewModel;
        Title = viewModel.Title;
    }
}