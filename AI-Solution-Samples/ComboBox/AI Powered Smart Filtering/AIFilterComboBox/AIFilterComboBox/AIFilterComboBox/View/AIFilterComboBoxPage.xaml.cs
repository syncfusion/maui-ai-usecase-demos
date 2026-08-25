using Syncfusion.Maui.Core.Carousel;

namespace AIFilterComboBox.AIFilterComboBox;

public partial class AIFilterComboBoxPage : ContentPage
{
	public AIFilterComboBoxPage()
	{
		InitializeComponent();
        BindingContext = viewModel;
        combobox.FilterBehavior = new ComboBoxCustomFilter(viewModel);
	}

    private void combobox_DropdownOpened(object sender, EventArgs e)
    {
        viewModel.IsLoading = false;
    }

    private void combobox_DropDownClosed(object sender, EventArgs e)
    {
        viewModel.IsLoading = false;
    }
}