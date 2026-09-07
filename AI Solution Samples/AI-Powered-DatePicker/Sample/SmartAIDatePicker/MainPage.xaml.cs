using SmartAIDatePicker.ViewModels;

namespace SmartAIDatePicker;

public partial class MainPage : ContentPage
{
    public MainPage(DatePickerViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
