using NutriLens.Models;
using NutriLens.Views;
using System.Globalization;

namespace NutriLens.Helpers
{
    public class StringToBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => !string.IsNullOrWhiteSpace(value as string);

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
    public class TierToIconConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is ScoreTier tier ? tier switch
            {
                ScoreTier.Excellent => "✔",
                ScoreTier.Moderate => "⚠",
                ScoreTier.Poor => "⚠",
                _ => "•"
            } : "•";

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
    /// <summary>
    /// Centralized navigation so Commands remain View-agnostic.
    /// Backed by the NavigationPage set as App.MainPage (no Shell).
    /// </summary>
    public static class AppNavigator
    {
        private static INavigation Nav =>
            (Application.Current?.MainPage as NavigationPage)?.Navigation
            ?? throw new InvalidOperationException("NavigationPage is not set as MainPage.");

        public static Task GoDashboardAsync() => PushAsync<NutriLensDashboardPage>();
        public static Task GoHistoryAsync() => PushAsync<HistoryPage>();
        public static Task GoProfileAsync() => PushAsync<ProfilePage>(); 
        public static Task GoTrendAsync() => PushAsync<TrendsPage>();
        public static Task GoScanAsync() => PushAsync<ScanIngredientsPage>();
        public static Task GoDetailBreakdownAsync() => PushAsync<DetailBreakdownPage>();
        public static Task GoAnalyzeIngredientsAsync() => PushAsync<AnalyzeIngredientsResultPage>();

        public static Task GoBreakdownAsync() => PushAsync<DetailBreakdownPage>();
        public static Task PopAsync() => Nav.PopAsync();

        private static Task PushAsync<T>() where T : Page, new()
        {
            var nav = Nav;
            if (nav.NavigationStack.LastOrDefault() is T)
                return Task.CompletedTask;
            return nav.PushAsync(new T());
        }
    }
}