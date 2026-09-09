namespace MediFlowSample
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Register routes for navigation
            Routing.RegisterRoute("splash", typeof(Views.SplashPage));
            Routing.RegisterRoute("dashboard", typeof(Views.DashboardPage));
            Routing.RegisterRoute("patients", typeof(Views.PatientsPage));
            Routing.RegisterRoute("schedule", typeof(Views.AppointmentsPage));
        }
    }
}
