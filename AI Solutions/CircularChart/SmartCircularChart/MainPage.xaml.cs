using Microsoft.Maui.Controls;
using Syncfusion.Maui.Toolkit.Charts;

namespace SmartCircularChart
{
    public partial class MainPage : ContentPage
    {
        private readonly CircularChartViewModel viewModel;

        public MainPage()
        {
            InitializeComponent();
            viewModel = new CircularChartViewModel(new AzureAIService());
            BindingContext = viewModel;
            BusyOverlay.IsVisible = false;
        }

        private async void OnGroupChartClicked(object? sender, EventArgs e)
        {
            try
            {
                BusyOverlay.IsVisible = true;
                viewModel.UserPrompt = PromptEntry.Text ?? string.Empty;
                await viewModel.GroupDataAsync();
            }
            finally
            {
                BusyOverlay.IsVisible = false;
            }
        }
    }

    public class LegendExt : ChartLegend
    {
        protected override double GetMaximumSizeCoefficient()
        {
            return 1;
        }
    }
}
