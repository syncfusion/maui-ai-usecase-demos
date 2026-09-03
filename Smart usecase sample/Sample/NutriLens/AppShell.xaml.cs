using NutriLens.Views;
namespace NutriLens
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("history", typeof(HistoryPage));
            Routing.RegisterRoute("scan", typeof(ScanIngredientsPage));
            Routing.RegisterRoute("breakdown", typeof(DetailBreakdownPage));
            Routing.RegisterRoute("NutriLensDashboardPage", typeof(NutriLensDashboardPage));
        }
    }
}
