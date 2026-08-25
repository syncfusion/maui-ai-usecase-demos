namespace AIPoweredChartSample
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(MobileAssistViewPage), typeof(MobileAssistViewPage));
        }
    }
}
