using PatientCompanion.ViewModels;
using Syncfusion.Maui.Core;

namespace PatientCompanion.Views;

public partial class MedicationsPage : ContentPage
{
    public MedicationsPage()
        : this(App.Resolve<MedicationsViewModel>())
    {
    }

    public MedicationsPage(MedicationsViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }
}