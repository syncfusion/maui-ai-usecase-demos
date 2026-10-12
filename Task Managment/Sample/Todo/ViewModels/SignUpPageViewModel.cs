using Microsoft.Extensions.Logging;

namespace Todo.ViewModels;

/// <summary>
/// ViewModel for the sign-up page.  The full implementation is added in
/// Module 2 (Task 2-01) — this skeleton exists so the DI container can
/// resolve the transient in Task 1-05.
/// </summary>
public sealed partial class SignUpPageViewModel : ViewModelBase
{
    /// <summary>
    /// Initialises a new instance of the <see cref="SignUpPageViewModel"/>
    /// class.
    /// </summary>
    /// <param name="logger">Logger injected by the DI container.</param>
    public SignUpPageViewModel(ILogger<SignUpPageViewModel> logger)
        : base(logger)
    {
        Title = "Create account";
    }
}
