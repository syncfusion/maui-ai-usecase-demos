using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WardrobeAI.Data;
using WardrobeAI.Services;

namespace WardrobeAI
{
    public partial class App : Application
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<App>? _logger;

        public App(IServiceProvider services)
        {
            // Syncfusion license is registered in MauiProgram.cs from the
            // SYNCFUSION_LICENSE_KEY environment variable so the key never
            // has to be committed to source control.
            // TODO: Set the SYNCFUSION_LICENSE_KEY environment variable before
            // running the app. See README.md → "Setup" for instructions.
            _services = services;
            _logger = services.GetService<ILoggerFactory>()?.CreateLogger<App>();

            // Lock the entire app to the Light theme so every page, control and
            // component renders with the same warm-navy palette regardless of the
            // OS / system theme. The dark-only color tokens in Colors.xaml are
            // kept for completeness but are no longer referenced from XAML.
            UserAppTheme = AppTheme.Light;
            if (Application.Current is not null)
            {
                Application.Current.UserAppTheme = AppTheme.Light;
            }

            InitializeComponent();
        }
        protected override Window CreateWindow(IActivationState? activationState)
        {
            var shell = _services.GetRequiredService<AppShell>();
            var window = new Window(shell);

            // Run post-shell startup (DB schema initialization + default user seed + initial
            // navigation) on Window.Created, not Shell.Loaded.
            //
            // On Android, Shell.Loaded fires *before* the Shell is attached to its
            // Window, so Shell.Current.GoToAsync throws InvalidOperationException
            // ("navigation service has not been initialized"). Because the previous
            // handler was async void with no try/catch, that exception escaped
            // unobserved and the Android process was killed with exit code 0
            // (no useful stack trace). Window.Created is the documented MAUI hook
            // for post-Shell-init work and runs after the Shell is fully realized.
            //
            // P7: the auth → onboarding → main gate was removed. The app now boots
            // straight into the SfTabView host with the seeded default user
            // (AppConstants.DefaultUserName).
            window.Created += OnWindowCreated;

            return window;
        }

        private async void OnWindowCreated(object? sender, EventArgs e)
        {
            try
            {
                // The schema is defined in WardrobeDbContext.OnModelCreating and this
                // project does not contain EF migrations. EnsureCreated creates the
                // complete SQLite schema on first launch; MigrateAsync alone would
                // leave the database without the Users table when no migrations exist.
                using (var scope = _services.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<WardrobeDbContext>();
                    await db.Database.EnsureCreatedAsync();
                }

                // 2. Seed (or restore) the default user. Every view model resolves
                //    the active user through IUserContext; this call must complete
                //    before any of them call GetCurrentUserId.
                var userContext = _services.GetRequiredService<IUserContext>();
                await userContext.EnsureUserAsync();

                // 3. Land on the right starting route. AppShell.xaml.cs still
                //    registers //main (SfTabView host) and //main/desktop
                //    (the persistent desktop split-view host) so the
                //    historically-tabbed code paths continue to resolve.
                await Task.Yield();
                if (Shell.Current is not null)
                {
                    var mainRoute = Helpers.IdiomExtensions.IsDesktop
                        ? "//main/desktop"
                        : "//main";
                    await Shell.Current.GoToAsync(mainRoute);
                }
            }
            catch (Exception ex)
            {
                // Never let a startup failure escape this handler — on Android that
                // would terminate the process and look like an unexplained exit.
                _logger?.LogError(ex, "App startup failed.");
            }
        }
    }
}
