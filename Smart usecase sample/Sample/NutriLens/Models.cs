using System.Windows.Input;

namespace NutriLens.Models
{
    using System.Text.Json.Serialization;

    public sealed class ImagePickerService : IImagePickerService
    {
        public async Task<FileResult?> CapturePhotoAsync(
            CancellationToken cancellationToken = default)
        {
            // FIX 1: On unpackaged Windows apps (WindowsPackageType=None),
            // MediaPicker.CapturePhotoAsync launches WinRT CameraCaptureUI,
            // which crashes the process with 0xC0000005 because it requires
            // package identity. Fall back to the gallery picker on WinUI.
            if (DeviceInfo.Platform == DevicePlatform.WinUI)
                return await PickPhotoAsync(cancellationToken);

            if (!MediaPicker.Default.IsCaptureSupported)
                return await PickPhotoAsync(cancellationToken);

            var photo = await MediaPicker.Default.CapturePhotoAsync(
                new MediaPickerOptions { Title = "Capture food label" });

            return await CopyToAppCacheAsync(photo);
        }

        public async Task<FileResult?> PickPhotoAsync(
            CancellationToken cancellationToken = default)
        {
            var photo = await MediaPicker.Default.PickPhotoAsync(
                new MediaPickerOptions { Title = "Select food label" });

            return await CopyToAppCacheAsync(photo);
        }

        // FIX 2: MAUI deletes the temp capture files. Copy to a location WE
        // control so SelectedImageHolder.Current never references a deleted file.
        private static async Task<FileResult?> CopyToAppCacheAsync(FileResult? file)
        {
            if (file is null)
                return null;

            var targetDir = Path.Combine(
                FileSystem.CacheDirectory, "captures");
            Directory.CreateDirectory(targetDir);

            var target = Path.Combine(
                targetDir,
                $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}");

            await using var source = await file.OpenReadAsync();
            await using var destination = File.Create(target);
            await source.CopyToAsync(destination);

            return new FileResult(target);
        }
    }
    public interface IImagePickerService
    {
        Task<FileResult?> CapturePhotoAsync(CancellationToken cancellationToken = default);
        Task<FileResult?> PickPhotoAsync(CancellationToken cancellationToken = default);
    }

    public sealed class ProductAnalysis
    {
        [JsonPropertyName("productName")]
        public string ProductName { get; set; } = "Unknown product";

        [JsonPropertyName("ingredientSummary")]
        public string IngredientSummary { get; set; } = string.Empty;

        [JsonPropertyName("fullIngredients")]
        public string FullIngredients { get; set; } = string.Empty;

        [JsonPropertyName("ingredients")]
        public List<IngredientAnalysis> Ingredients { get; set; } = [];

        [JsonPropertyName("nutrition")]
        public NutritionInfo Nutrition { get; set; } = new();

        [JsonPropertyName("preservatives")]
        public List<string> Preservatives { get; set; } = [];

        [JsonPropertyName("artificialColors")]
        public List<string> ArtificialColors { get; set; } = [];

        [JsonPropertyName("allergens")]
        public List<string> Allergens { get; set; } = [];

        [JsonPropertyName("additives")]
        public List<string> Additives { get; set; } = [];

        [JsonPropertyName("healthAssessment")]
        public HealthAssessment HealthAssessment { get; set; } = new();

        [JsonPropertyName("confidence")]
        public string Confidence { get; set; } = "Unknown";
    }

    public sealed class IngredientAnalysis
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("purpose")]
        public string Purpose { get; set; } = string.Empty;

        [JsonPropertyName("riskLevel")]
        public string RiskLevel { get; set; } = "Unknown";

        [JsonPropertyName("explanation")]
        public string Explanation { get; set; } = string.Empty;

        [JsonIgnore]
        public string Number { get; set; } = string.Empty;
    }

    public sealed class NutritionInfo
    {
        [JsonPropertyName("servingSize")]
        public string ServingSize { get; set; } = "Not available";

        [JsonPropertyName("servingsPerContainer")]
        public string ServingsPerContainer { get; set; } = "Not available";

        [JsonPropertyName("calories")]
        public double? Calories { get; set; }

        [JsonPropertyName("totalFatGrams")]
        public double? TotalFatGrams { get; set; }

        [JsonPropertyName("saturatedFatGrams")]
        public double? SaturatedFatGrams { get; set; }

        [JsonPropertyName("sodiumMg")]
        public double? SodiumMg { get; set; }

        [JsonPropertyName("carbohydratesGrams")]
        public double? CarbohydratesGrams { get; set; }

        [JsonPropertyName("sugarsGrams")]
        public double? SugarsGrams { get; set; }

        [JsonPropertyName("addedSugarsGrams")]
        public double? AddedSugarsGrams { get; set; }

        [JsonPropertyName("proteinGrams")]
        public double? ProteinGrams { get; set; }

        [JsonPropertyName("fiberGrams")]
        public double? FiberGrams { get; set; }

        // ----- Display helpers for DetailBreakdownPage (computed, [JsonIgnore]) -----

        [JsonIgnore]
        public string CaloriesDisplay =>
            Calories is null ? "N/A" : ((int)Calories.Value).ToString();

        [JsonIgnore]
        public string TotalFatLabel => $"Total Fat {(TotalFatGrams is null ? "N/A" : $"{TotalFatGrams:0.#}g")}";
        [JsonIgnore]
        public string SaturatedFatLabel => $"Saturated Fat {(SaturatedFatGrams is null ? "N/A" : $"{SaturatedFatGrams:0.#}g")}";
        [JsonIgnore]
        public string SodiumLabel => $"Sodium {(SodiumMg is null ? "N/A" : $"{SodiumMg:0.#}mg")}";
        [JsonIgnore]
        public string CarbohydratesLabel => $"Total Carbohydrate {(CarbohydratesGrams is null ? "N/A" : $"{CarbohydratesGrams:0.#}g")}";
        [JsonIgnore]
        public string SugarsLabel => $"Total Sugars {(SugarsGrams is null ? "N/A" : $"{SugarsGrams:0.#}g")}";
        [JsonIgnore]
        public string ProteinLabel => $"Protein {(ProteinGrams is null ? "N/A" : $"{ProteinGrams:0.#}g")}";
        [JsonIgnore]
        public string AddedSugarsNote =>
            AddedSugarsGrams is null
                ? "No added sugars declared on this label."
                : $"Includes {AddedSugarsGrams:0.#}g Added Sugars";

        [JsonIgnore]
        public string TotalFatDv => DvText(TotalFatGrams, 78);
        [JsonIgnore]
        public double TotalFatProgress => DvProgress(TotalFatGrams, 78);

        [JsonIgnore]
        public string SaturatedFatDv => DvText(SaturatedFatGrams, 20);
        [JsonIgnore]
        public double SaturatedFatProgress => DvProgress(SaturatedFatGrams, 20);

        [JsonIgnore]
        public string SodiumDv => DvText(SodiumMg, 2300);
        [JsonIgnore]
        public double SodiumProgress => DvProgress(SodiumMg, 2300);

        [JsonIgnore]
        public string CarbohydratesDv => DvText(CarbohydratesGrams, 275);
        [JsonIgnore]
        public double CarbohydratesProgress => DvProgress(CarbohydratesGrams, 275);

        [JsonIgnore]
        public string SugarsDv => DvText(SugarsGrams, 50);
        [JsonIgnore]
        public double SugarsProgress => DvProgress(SugarsGrams, 50);

        [JsonIgnore]
        public string ProteinDv => DvText(ProteinGrams, 50);
        [JsonIgnore]
        public double ProteinProgress => DvProgress(ProteinGrams, 50);

        private static string DvText(double? grams, double dailyValue)
        {
            if (grams is null)
                return "N/A";

            var percent = Math.Clamp(grams.Value / dailyValue * 100, 0, 999);
            return $"{Math.Round(percent)}%";
        }

        private static double DvProgress(double? grams, double dailyValue)
        {
            if (grams is null || grams.Value <= 0)
                return 0;

            return Math.Clamp(grams.Value / dailyValue, 0, 1);
        }
    }
    public sealed class HealthAssessment
    {
        [JsonPropertyName("overallScore")]
        public int OverallScore { get; set; }

        [JsonPropertyName("verdict")]
        public string Verdict { get; set; } = "Unknown";

        [JsonPropertyName("scoreExplanation")]
        public string ScoreExplanation { get; set; } = string.Empty;

        [JsonPropertyName("positiveFactors")]
        public List<string> PositiveFactors { get; set; } = [];

        [JsonPropertyName("negativeFactors")]
        public List<string> NegativeFactors { get; set; } = [];

        [JsonPropertyName("riskIndicators")]
        public List<RiskIndicator> RiskIndicators { get; set; } = [];

        [JsonPropertyName("healthInsights")]
        public string HealthInsights { get; set; } = string.Empty;

        [JsonPropertyName("recommendation")]
        public string Recommendation { get; set; } = string.Empty;
    }

    public sealed class RiskIndicator
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("riskLevel")]
        public string RiskLevel { get; set; } = "Unknown";

        [JsonIgnore]
        public string BadgeColor =>
            RiskLevel.ToLowerInvariant() switch
            {
                "high" => "#E04B4B",
                "moderate" => "#E0A33B",
                "low" => "#0E7C57",
                _ => "#6B7280"
            };

        [JsonIgnore]
        public string BadgeBackground =>
            RiskLevel.ToLowerInvariant() switch
            {
                "high" => "#FCE6E6",
                "moderate" => "#FFF1D9",
                "low" => "#EAF3EE",
                _ => "#EEF1EF"
            };
    }
    /// <summary>Per-ingredient concern row for the score screen.</summary>
    public class IngredientConcernItem
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string BadgeText { get; set; } = string.Empty;
        public string BadgeColor { get; set; } = "#0E7C57";
        public string BadgeBg { get; set; } = "#EAF3EE";
    }
    public partial class IngredientItem
    {
        public string Number { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? SubText { get; set; }
    }
    /// <summary>Scan list row for the home &amp; history pages.</summary>
    public class ScanItem
    {
        public string Emoji { get; set; } = "🥗";
        public string Title { get; set; } = string.Empty;
        public string When { get; set; } = string.Empty;
        public int Score { get; set; }
        public ScoreTier Tier { get; set; }
        public string TierLabel => Tier.ToString().ToUpperInvariant();
        public Color ScoreColor => Tier switch
        {
            ScoreTier.Excellent => Color.FromArgb("#0E7C57"),
            ScoreTier.Moderate => Color.FromArgb("#E0A33B"),
            ScoreTier.Poor => Color.FromArgb("#E04B4B"),
            _ => Color.FromArgb("#6B7280"),
        };
        public Color TileColor => Tier switch
        {
            ScoreTier.Excellent => Color.FromArgb("#F2EFE6"),
            ScoreTier.Moderate => Color.FromArgb("#F2E6D6"),
            ScoreTier.Poor => Color.FromArgb("#FCE6E6"),
            _ => Color.FromArgb("#EEF1EF"),
        };
    }

    public enum ScoreTier { Excellent, Moderate, Poor }
}