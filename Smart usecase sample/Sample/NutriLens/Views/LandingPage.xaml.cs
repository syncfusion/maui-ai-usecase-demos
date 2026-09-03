using NutriLens.ViewModels;
using NutriLens;
namespace NutriLens.Views
{
    public partial class LandingPage : ContentPage
    {
        public LandingPage()
        {
            InitializeComponent();
            BindingContext = new LandingViewModel();
        }

        //private async void OnGoogleTapped(object? sender, TappedEventArgs e)
        //{
        //    await Navigation.PushAsync(new MainPage());
        //}

        //private async void OnCreateProfileTapped(object? sender, EventArgs e)
        //{
        //    await Navigation.PushAsync(new MainPage());
        //}

        //private async void OnExploreSampleTapped(object? sender, EventArgs e)
        //{
        //    await Navigation.PushAsync(new MainPage());
        //}
    }
}