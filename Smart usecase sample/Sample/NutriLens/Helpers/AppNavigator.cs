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
    /// Backed by the NavigationPage set as the root Window page (no Shell).
    /// </summary>
    public static class AppNavigator
    {
        private static NavigationPage? RootNavigationPage =>
            Application.Current?.Windows.FirstOrDefault()?.Page as NavigationPage;

        private static INavigation Nav =>
            RootNavigationPage?.Navigation
            ?? throw new InvalidOperationException(
                "NavigationPage is not set as the root page.");

        public static Task GoDashboardAsync() => PushAsync<NutriLensDashboardPage>();
        public static Task GoHistoryAsync() => PushAsync<HistoryPage>();
        public static Task GoProfileAsync() => PushAsync<ProfilePage>();
        public static Task GoTrendAsync() => PushAsync<TrendsPage>();
        public static Task GoScanAsync() => PushAsync<ScanIngredientsPage>();
        public static Task GoAnalyzeIngredientsAsync() => PushAsync<AnalyzeIngredientsResultPage>();

        public static Task PopAsync() => Nav.PopAsync();

        /// <summary>Null-safe alert used from ViewModels (keeps MVVM clean).</summary>
        public static Task ShowAlertAsync(string title, string message, string cancel)
        {
            var page = Application.Current?.Windows.FirstOrDefault()?.Page;

            if (page is null)
                return Task.CompletedTask;

            return page.DisplayAlertAsync(title, message, cancel);
        }

        private static Task PushAsync<T>() where T : Page, new()
        {
            var nav = Nav;
            if (nav.NavigationStack.LastOrDefault() is T)
                return Task.CompletedTask;
            return nav.PushAsync(new T());
        }

        /// <summary>
        /// Pushes the result page for a specific analysis (sample/static or AI).
        /// The result is also stashed on <see cref="AnalysisNavigationData"/> so
        /// any downstream page reads the exact same instance.
        /// </summary>
        public static Task GoAnalyzeIngredientsAsync(IngredientAnalysisResult result)
        {
            ArgumentNullException.ThrowIfNull(result);

            AnalysisNavigationData.CurrentResult = result;
            return Nav.PushAsync(new AnalyzeIngredientsResultPage(result));
        }
        /// <summary>
        /// Pushes the Review page, carrying the extracted OCR text + source
        /// image via the read-once <see cref="AnalysisNavigationData.PendingReview"/> slot.
        /// </summary>
        public static Task GoReviewIngredientsAsync(string extractedText, FileResult image)
        {
            if (string.IsNullOrWhiteSpace(extractedText))
                throw new ArgumentException("Extracted text is required.", nameof(extractedText));
            ArgumentNullException.ThrowIfNull(image);

            AnalysisNavigationData.PendingReview =
                new PendingReviewData { ExtractedText = extractedText, Image = image };

            var nav = Nav;
            if (nav.NavigationStack.LastOrDefault() is ReviewIngredientsPage)
                return Task.CompletedTask;
            return nav.PushAsync(new ReviewIngredientsPage());
        }
    }
}