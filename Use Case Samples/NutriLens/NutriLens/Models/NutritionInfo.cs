using System.Text.Json.Serialization;

namespace NutriLens.Models;

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

    [JsonIgnore]
    public string CaloriesDisplay => Calories is null ? "N/A" : ((int)Calories.Value).ToString();
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
    public string AddedSugarsNote => AddedSugarsGrams is null
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

    private static string DvText(double? amount, double dailyValue)
    {
        if (amount is null)
            return "N/A";

        var percent = Math.Clamp(amount.Value / dailyValue * 100, 0, 999);
        return $"{Math.Round(percent)}%";
    }

    private static double DvProgress(double? amount, double dailyValue)
        => amount is null || amount.Value <= 0
            ? 0
            : Math.Clamp(amount.Value / dailyValue, 0, 1);
}