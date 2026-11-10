using eSIMDataManager.ViewModels;

namespace eSIMDataManager;

public partial class ProfilesPage : DemoPage
{
    public ProfilesPage(MainViewModel viewModel) : base(viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        InitializePopupHost();
    }
}
