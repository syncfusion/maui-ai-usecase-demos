using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;
using Todo.Data;
using Todo.Data.Repositories;
using Todo.Models;
using Todo.Services.Storage;
using Todo.Utilities;

namespace Todo;

/// <summary>
/// Bootstraps the .NET MAUI application, configures the Syncfusion MAUI Core
/// runtime, registers the default fonts, the SQLite database context, the
/// generic repositories and the local-storage service.  DI service and
/// ViewModel registration will be expanded in later tasks (1-05+
/// modules).
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

        AddDataServices(builder);
        AddStorageServices(builder);

        return builder.Build();
    }

    /// <summary>
    /// Registers the <see cref="AppDbContext"/> and one
    /// <see cref="IRepository{T}"/> per aggregate root.  The database file
    /// is created lazily the first time the context is resolved.
    /// </summary>
    /// <param name="builder">The host builder.</param>
    private static void AddDataServices(MauiAppBuilder builder)
    {
        // AppDbContext is registered as a singleton so every repository
        // shares the same SQLite connection.
        builder.Services.AddSingleton<AppDbContext>(sp =>
        {
            ILogger<AppDbContext> logger = sp.GetRequiredService<ILogger<AppDbContext>>();
            string databasePath = ResolveDatabasePath();
            return new AppDbContext(databasePath, logger);
        });

        // Generic repositories — one per aggregate root.  Resolved by the
        // services layer (e.g. AuthenticationService, TaskService, ...).
        builder.Services.AddSingleton<IRepository<User>, Repository<User>>();
        builder.Services.AddSingleton<IRepository<TaskList>, Repository<TaskList>>();
        builder.Services.AddSingleton<IRepository<TaskItem>, Repository<TaskItem>>();
    }

    /// <summary>
    /// Registers the SQLite-backed <see cref="ILocalStorageService"/>.
    /// </summary>
    /// <param name="builder">The host builder.</param>
    private static void AddStorageServices(MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<ILocalStorageService, LocalStorageService>();
    }

    /// <summary>
    /// Computes the absolute path of the SQLite database file.  The default
    /// location is <c>FileSystem.AppDataDirectory/todo.db3</c> so the data
    /// stays with the user's profile and is removed automatically when the
    /// app is uninstalled.  When the MAUI host is not available (e.g. in a
    /// unit test), the path falls back to a temp directory.
    /// </summary>
    private static string ResolveDatabasePath()
    {
        try
        {
            string directory = FileSystem.AppDataDirectory;
            return Path.Combine(directory, Constants.Database.FileName);
        }
        catch (InvalidOperationException)
        {
            // FileSystem.AppDataDirectory is not initialised outside of a
            // running MAUI host.  Use the system temp folder as a safe
            // fallback for unit tests / design-time tooling.
            string fallback = Path.Combine(Path.GetTempPath(), Constants.App.Identifier);
            return Path.Combine(fallback, Constants.Database.FileName);
        }
    }
}
