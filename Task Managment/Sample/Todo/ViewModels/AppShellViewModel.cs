using Microsoft.Extensions.Logging;

namespace Todo.ViewModels;

/// <summary>
/// ViewModel for the application shell.  Responsible for top-level state such
/// as the current user and the currently-selected sidebar category.  The full
/// implementation is fleshed out in later tasks — this skeleton exists so the
/// DI container can resolve the singleton in Task 1-05.
/// </summary>
public sealed partial class AppShellViewModel : ViewModelBase
{
    /// <summary>
    /// Initialises a new instance of the <see cref="AppShellViewModel"/>
    /// class.
    /// </summary>
    /// <param name="logger">Logger injected by the DI container.</param>
    public AppShellViewModel(ILogger<AppShellViewModel> logger)
        : base(logger)
    {
        Title = "Todo";
    }
}
