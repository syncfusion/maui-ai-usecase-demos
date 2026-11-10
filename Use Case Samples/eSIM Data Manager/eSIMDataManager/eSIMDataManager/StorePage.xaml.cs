using eSIMDataManager.ViewModels;

namespace eSIMDataManager;

public partial class StorePage : DemoPage
{
    public StorePage(MainViewModel viewModel) : base(viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        InitializePopupHost();
    }
}
