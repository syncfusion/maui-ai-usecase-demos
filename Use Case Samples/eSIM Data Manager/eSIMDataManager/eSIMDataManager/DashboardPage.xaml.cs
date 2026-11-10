using eSIMDataManager.ViewModels;

namespace eSIMDataManager;

public partial class DashboardPage : DemoPage
{
    public DashboardPage(MainViewModel viewModel) : base(viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        InitializePopupHost();
    }
}
