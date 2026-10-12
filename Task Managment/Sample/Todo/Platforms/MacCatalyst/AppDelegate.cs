using Foundation;

namespace Todo;

/// <summary>
/// The MAUI UI application delegate used as the entry point on Mac Catalyst.
/// It simply hands off to <see cref="MauiProgram"/>.
/// </summary>
[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    /// <inheritdoc />
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
