using NutriLens.Models;
using NutriLens.Views;
using System.Globalization;

namespace NutriLens.Helpers
{
    public sealed class BoolToColorConverter : IValueConverter
    {
        public Color TrueColor { get; set; } = Colors.Green;
        public Color FalseColor { get; set; } = Colors.Red;

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool b)
                return b ? TrueColor : FalseColor;

            return FalseColor;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
    public class StringToBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => !string.IsNullOrWhiteSpace(value as string);

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
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

        public static Task GoDashboardAsync() => NavigateTopLevelAsync<NutriLensDashboardPage>();
        public static Task GoHistoryAsync() => NavigateTopLevelAsync<HistoryPage>();
        public static Task GoProfileAsync() => NavigateTopLevelAsync<ProfilePage>();
        public static Task GoTrendAsync() => NavigateTopLevelAsync<TrendsPage>();
        public static Task GoScanAsync() => NavigateTopLevelAsync<ScanIngredientsPage>();
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

        private static async Task NavigateTopLevelAsync<T>() where T : Page
        {
            var nav = Nav;
            await nav.PopToRootAsync(false);

            if (nav.NavigationStack.FirstOrDefault() is T)
                return;

            await nav.PushAsync(CreatePage<T>(), false);
        }

        private static Task PushAsync<T>() where T : Page
            => Nav.PushAsync(CreatePage<T>());

        private static T CreatePage<T>() where T : Page
        {
            var services = Application.Current?.Handler?.MauiContext?.Services;
            return services?.GetService<T>()
                ?? Activator.CreateInstance<T>()
                ?? throw new InvalidOperationException($"Unable to create page {typeof(T).Name}.");
        }

        /// <summary>
        /// Pushes the result page for a specific analysis (sample/static or AI).
        /// The result is also stashed on <see cref="AnalysisNavigationData"/> so
        /// any downstream page reads the exact same instance.
        /// </summary>
        public static Task GoAnalyzeIngredientsAsync(IngredientAnalysisResult result)
            => GoAnalyzeIngredientsAsync(result, imagePath: null);

        public static Task GoAnalyzeIngredientsAsync(
            IngredientAnalysisResult result,
            string? imagePath)
        {
            ArgumentNullException.ThrowIfNull(result);

            AnalysisNavigationData.CurrentResult = result;
            AnalysisNavigationData.CurrentImagePath = imagePath;
            return Nav.PushAsync(new AnalyzeIngredientsResultPage(result));
        }
        public static Task<bool> ShowConfirmAsync(string message, string accept, string cancel)
        {
            var page = Application.Current?.Windows.FirstOrDefault()?.Page;

            if (page is null)
                return Task.FromResult(false);

            return page.DisplayAlertAsync("Confirm", message, accept, cancel);
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
            return nav.PushAsync(CreatePage<ReviewIngredientsPage>());
        }
    }
}