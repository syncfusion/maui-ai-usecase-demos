using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NutriLens.Helpers; 

namespace NutriLens.ViewModels
{
    public partial class LandingViewModel : ObservableObject
    {
        //[RelayCommand] private Task GoogleAsync() => AppNavigator.GoHomeAsync();
        //[RelayCommand] private Task CreateAsync() => AppNavigator.GoHomeAsync();
        //[RelayCommand] private Task ExploreAsync() => AppNavigator.GoHomeAsync();
    }
    public partial class DetailBreakdownViewModel : ObservableObject
    {
        public int Calories { get; set; } = 130;
        public double TotalFatGrams { get; set; } = 4;
        public double SatFatGrams { get; set; } = 0.5;
        public int SodiumMg { get; set; } = 110;
        public double CarbsGrams { get; set; } = 20;
        public double SugarsGrams { get; set; } = 5;
        public double AddedSugarsGrams { get; set; } = 5;
        public double ProteinGrams { get; set; } = 3;
        public string FullIngredients { get; set; } =
            "Oat base (water, oats), cane sugar, gellan gum, sea salt, natural vanilla flavor, " +
            "calcium carbonate, dipotassium phosphate, vitamin D, riboflavin, vitamin B12.";

        [RelayCommand]
        private Task BackAsync() => AppNavigator.PopAsync();
    }
    
}