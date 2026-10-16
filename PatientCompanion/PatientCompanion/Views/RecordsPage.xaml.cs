using PatientCompanion.ViewModels;

namespace PatientCompanion.Views;

public partial class RecordsPage : ContentPage
{
    public RecordsPage()
        : this(App.Resolve<RecordsViewModel>())
    {
    }

    public RecordsPage(RecordsViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }
}