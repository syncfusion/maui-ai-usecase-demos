namespace AIPoweredChartSample
{
    using Microsoft.Extensions.DependencyInjection;

    public partial class MainPage : ContentPage
    {
        /// <summary>
        /// Field to hold the instance of ChartViewModel.
        /// </summary>
        readonly ChartViewModel chartViewModel;

        /// <summary>
        /// MainPage constructor initializes the components and sets up the data context for the page.
        /// </summary>
        public MainPage()
        {
            InitializeComponent();
            chartViewModel = ServiceHelper.Services.GetRequiredService<ChartViewModel>();
            this.BindingContext = chartViewModel;
        }

        /// <summary>
        /// Handles the click event for the AI button. On Android and iOS, it navigates to a dedicated page showing the AssistView. On other platforms, it displays the AssistView in a side panel.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnAIButtonClicked(object sender, EventArgs e)
        {
#if ANDROID || (IOS && !MACCATALYST)
            // On Android and iOS, navigate to a dedicated page showing the AssistView
            var navParams = new Dictionary<string, object>
            {
                { "vm", chartViewModel }
            };
            await Shell.Current.GoToAsync(nameof(MobileAssistViewPage), true, navParams);
#else
            Grid.SetColumnSpan(chartView, 1);
            assistViewGrid.IsVisible = true;
            AiButton.IsVisible = false;
            headerView.IsVisible = true;
            AssistView.IsVisible = true;
#endif
        }

        /// <summary>
        /// Handles the click event for the close button, hiding the header and assist views.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCloseButtonClicked(object sender, EventArgs e)
        {
            Grid.SetColumnSpan(chartView, 2);
            this.chartViewModel.AssistItems.Clear();
            assistViewGrid.IsVisible = false;
            headerView.IsVisible = false;
            AssistView.IsVisible = false;
            AiButton.IsVisible = true;
        }
    }
}