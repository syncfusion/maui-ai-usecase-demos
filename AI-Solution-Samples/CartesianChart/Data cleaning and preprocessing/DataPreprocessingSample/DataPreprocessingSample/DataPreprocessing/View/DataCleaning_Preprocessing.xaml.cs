using DataPreprocessingSample;

namespace DataPreprocessingSample;

public partial class DataCleaning_Preprocessing : ContentPage
{
    public DataCleaning_Preprocessing()
    {
        InitializeComponent();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        Chart.Handler?.DisconnectHandler();
    }

    protected override async void OnParentSet()
    {
        base.OnParentSet();

        viewModel.IsBusy = true;
        await Task.Delay(2000);
        await viewModel.LoadCleanedDataAsync();
    }
}