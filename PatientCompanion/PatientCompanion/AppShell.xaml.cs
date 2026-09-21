using PatientCompanion.Views;

// Windows-specific Shell customization is temporarily disabled while the
// shared AppHeaderView is validated across platforms.

namespace PatientCompanion;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

#if ANDROID || WINDOWS
        Shell.SetTabBarIsVisible(this, false);
#endif

        Routing.RegisterRoute(nameof(AppointmentPage), typeof(AppointmentPage));
    }
}