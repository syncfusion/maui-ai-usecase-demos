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
    /// Cached reference to the bound <see cref="SignInPageViewModel"/>
    /// so the page-lifecycle overrides below can call back into the
    /// ViewModel without re-resolving the <see cref="BindingContext"/>
    /// every time.
    /// </summary>
    private readonly SignInPageViewModel _viewModel;

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
        _viewModel = viewModel;
        Title = viewModel.Title;
    }

    /// <summary>
    /// Loads any remembered e-mail address (Task 2-06 — RememberMe) as
    /// soon as the page is rendered.  Doing this in <c>OnAppearing</c>
    /// rather than the constructor guarantees the binding pipeline is
    /// alive so the <see cref="SignInPageViewModel.Email"/> property
    /// change is reflected on the entry on the very first paint.
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await _viewModel.LoadRememberedStateAsync().ConfigureAwait(true);
        }
        catch (Exception)
        {
            // LoadRememberedStateAsync already swallows the typical
            // SecureStorage exceptions; this catch is the final guard so
            // a programming error never breaks the page-lifecycle.
        }
    }

    /// <summary>
    /// Scrubs the password from process memory when the page is dismissed
    /// (success sign-in, navigation to sign-up, etc.).  Task 2-06 mandates
    /// the password never lingers in process state longer than necessary.
    /// </summary>
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.ClearPassword();
    }
}