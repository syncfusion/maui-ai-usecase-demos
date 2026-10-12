using Microsoft.Extensions.Logging;

namespace Todo.ViewModels;

/// <summary>
/// ViewModel for the sign-in page.  The full implementation (binding to
/// <see cref="Todo.Services.Interfaces.IAuthenticationService"/>, navigation
/// to sign-up / main, error handling) is added in Module 2 (Task 2-01).
/// </summary>
public sealed partial class SignInPageViewModel : ViewModelBase
{
    /// <summary>
    /// Initialises a new instance of the <see cref="SignInPageViewModel"/>
    /// class.
    /// </summary>
    /// <param name="logger">Logger injected by the DI container.</param>
    public SignInPageViewModel(ILogger<SignInPageViewModel> logger)
        : base(logger)
    {
        Title = "Sign in";
    }
}
