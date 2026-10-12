using Microsoft.Extensions.Logging;

namespace Todo.ViewModels;

/// <summary>
/// ViewModel for the splash page that runs at app startup.  The full
/// implementation (loading state, navigation to sign-in / main) is added in
/// Task 1-06 (Navigation Setup) — this skeleton just exposes the
/// <see cref="ViewModelBase"/> lifecycle so the DI container can resolve it
/// today.
/// </summary>
public sealed partial class SplashPageViewModel : ViewModelBase
{
    /// <summary>
    /// Initialises a new instance of the <see cref="SplashPageViewModel"/>
    /// class.
    /// </summary>
    /// <param name="logger">Logger injected by the DI container.</param>
    public SplashPageViewModel(ILogger<SplashPageViewModel> logger)
        : base(logger)
    {
        Title = "Todo";
    }
}
