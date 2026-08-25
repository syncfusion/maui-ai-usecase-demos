using AIFilterAutocomplete.AIFilterAutocomplete;
using Syncfusion.Maui.Core.Carousel;

namespace AIFilterAutocomplete.AIFilterAutocomplete.View;

public partial class AIFilterAutocompletePage : ContentPage
{
	public AIFilterAutocompletePage()
	{
		InitializeComponent();
        BindingContext = viewModel;
        autoComplete.FilterBehavior = new AutocompleteFilterBehavior(viewModel);
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