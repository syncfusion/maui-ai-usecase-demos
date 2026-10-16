using PatientCompanion.Views;

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