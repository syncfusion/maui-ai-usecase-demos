using Microsoft.Maui.Controls;

namespace StockForecastingSample;

public partial class ChartGettingStarted : ContentPage
{
	public ChartGettingStarted()
	{
#if ANDROID || IOS
        this.Content = new StockAndroid();
#else
        this.Content = new StockSystem();
#endif
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
    }
}