using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NutriLens.Helpers;
using NutriLens.Models;
using NutriLens.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

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
    public partial class ScanResultViewModel : ObservableObject
    {
        private readonly AnalysisSession session;

        //public ProductAnalysis? Analysis => session.Analysis;

        //public ObservableCollection<IngredientAnalysis> Ingredients { get; } = [];

        //public bool IsLoading => session.IsLoading;

        //public bool HasError => session.Error is not null;

        //public string ErrorMessage =>
        //    session.Error?.Message ?? string.Empty;

        public ScanResultViewModel(AnalysisSession session)
        {
            this.session = session;

            Refresh();
        }

        public void Refresh()
        {
            //OnPropertyChanged(nameof(Analysis));
            //OnPropertyChanged(nameof(IsLoading));
            //OnPropertyChanged(nameof(HasError));
            //OnPropertyChanged(nameof(ErrorMessage));

            //Ingredients.Clear();

            //if (Analysis is not null)
            //{
            //    foreach (var ingredient in Analysis.Ingredients)
            //        Ingredients.Add(ingredient);
            //}
        }

        [RelayCommand]
        private Task BackAsync() => AppNavigator.PopAsync();

        //[RelayCommand]
        //private Task AnalyzeAsync() => AppNavigator.GoScoreAsync();
    }

    public partial class ScoreViewModel : ObservableObject
    {
        private readonly AnalysisSession session;

        //public ProductAnalysis? Analysis => session.Analysis;

        //public HealthAssessment Assessment =>
        //    Analysis?.HealthAssessment ?? new HealthAssessment();

        public ObservableCollection<RiskIndicator> Insights { get; } = [];

        public ScoreViewModel(AnalysisSession session)
        {
            this.session = session;

            //if (Analysis is not null)
            //{
            //    foreach (var risk in Assessment.RiskIndicators)
            //        Insights.Add(risk);
            //}
        }

        //public int Score => Assessment.OverallScore;

        //public string Verdict => Assessment.Verdict;

        [RelayCommand]
        private Task BackAsync() => AppNavigator.PopAsync();

        //[RelayCommand]
        //private Task BreakdownAsync() => AppNavigator.GoBreakdownAsync();
    }

    public partial class DetailBreakdownViewModel : ObservableObject
    {
        private readonly AnalysisSession session;

        //public ProductAnalysis? Analysis => session.Analysis;

        //public NutritionInfo Nutrition =>
            //Analysis?.Nutrition ?? new NutritionInfo();

        public ObservableCollection<IngredientAnalysis> Ingredients { get; } = [];

        public DetailBreakdownViewModel(AnalysisSession session)
        {
            this.session = session;

            //if (Analysis is not null)
            //{
            //    foreach (var ingredient in Analysis.Ingredients)
            //        Ingredients.Add(ingredient);
            //}
        }

        //[RelayCommand]
        //private Task BackAsync() => AppNavigator.PopAsync();
    }
     
    public partial class MainViewModel : ObservableObject
    {
        public ObservableCollection<ScanItem> RecentScans { get; } = new();

        public MainViewModel()
        {
            RecentScans.Add(new ScanItem
            {
                Emoji = "🍎",
                Title = "Organic Apple Juice",
                When = "2 hours ago",
                Score = 85,
                Tier = ScoreTier.Excellent
            });
            RecentScans.Add(new ScanItem
            {
                Emoji = "🍫",
                Title = "Protein Nutri-Bar",
                When = "Yesterday",
                Score = 68,
                Tier = ScoreTier.Moderate
            });
        }

        //[RelayCommand]
        //private Task ScanAsync() => AppNavigator.GoScanAsync();

        //[RelayCommand]
        //private Task HistoryAsync() => AppNavigator.GoHistoryAsync();
    }
}