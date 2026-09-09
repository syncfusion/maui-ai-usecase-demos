using NutriLens.Views; 

namespace NutriLens;

public partial class MainPage : ContentPage
{ 

    public MainPage()
    {
        InitializeComponent();
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Task.Delay(3000);
        await Navigation.PushAsync(new NutriLensDashboardPage());
    }
}