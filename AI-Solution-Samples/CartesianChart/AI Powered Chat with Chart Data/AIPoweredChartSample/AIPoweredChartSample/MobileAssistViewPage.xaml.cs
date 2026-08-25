namespace AIPoweredChartSample;

/// <summary>
/// MobileAssistViewPage is a content page that displays an assist view for mobile platforms.
/// </summary>
[QueryProperty(nameof(ViewModel), "vm")]
public partial class MobileAssistViewPage : ContentPage
{
    /// <summary>
    /// Field to hold the instance of ChartViewModel for the MobileAssistViewPage.
    /// </summary>
    ChartViewModel? chartViewModel;

    /// <summary>
    /// MobileAssistViewPage constructor initializes the components for the page.
    /// </summary>
    public MobileAssistViewPage()
	{
		InitializeComponent();
    }

    /// <summary>
    /// Gets or sets the ViewModel for the MobileAssistViewPage.
    /// </summary>
    public ChartViewModel? ViewModel
    {
        get => chartViewModel;
        set
        {
            chartViewModel = value;
            BindingContext = chartViewModel;
            if (AssistView != null)
            {
                AssistView.IsVisible = true;
            }
        }
    }
}