using Todo.ViewModels;

namespace Todo.Views.Authentication;

/// <summary>
/// Code-only partial of <see cref="ForgotPasswordPage"/> that adds the
/// dependency-injected constructor required by the .NET MAUI Shell.
/// The XAML UI is inflated by <c>InitializeComponent()</c> in
/// <c>ForgotPasswordPage.xaml.cs</c> and bound to the injected
/// <see cref="ForgotPasswordPageViewModel"/> here.
///
/// <para>
/// Splitting the XAML-loaded UI (<c>ForgotPasswordPage.xaml.cs</c>) and
/// the DI-injected constructor (<c>ForgotPasswordPage.cs</c>) mirrors
/// the convention already used by <see cref="SplashPage"/>,
/// <see cref="SignInPage"/> and <see cref="SignUpPage"/> so the
/// navigation surface stays uniform across the authentication flow.
/// </para>
/// </summary>
public sealed partial class ForgotPasswordPage : ContentPage
{
    /// <summary>
    /// Initialises a new instance of the <see cref="ForgotPasswordPage"/>
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
    public ForgotPasswordPage(ForgotPasswordPageViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        InitializeComponent();
        BindingContext = viewModel;
        Title = viewModel.Title;
    }
}
