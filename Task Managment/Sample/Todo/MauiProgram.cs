using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;

namespace Todo;

/// <summary>
/// Bootstraps the .NET MAUI application, configures the Syncfusion MAUI Core
/// runtime, registers the default fonts and (in DEBUG) the debug logging
/// provider. DI service and ViewModel registration will be expanded in
/// Task 1-05 (MauiProgram and DI).
/// </summary>
public static class MauiProgram
{
    /// <summary>
    /// Creates the <see cref="MauiApp"/> used to start the host process.
    /// </summary>
    /// <returns>The configured <see cref="MauiApp"/> instance.</returns>
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureSyncfusionCore()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
