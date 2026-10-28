namespace HomeInventoryManager
{
	public partial class AppShell : Shell
	{
		public AppShell()
		{
			InitializeComponent();
			Routing.RegisterRoute("AIAdvisor", typeof(Views.AIAdvisorPage));
		}
	}
}
