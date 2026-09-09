namespace NutriLens.Models;

public sealed class StaticDetailBreakdownData
{
    public string ProductName { get; init; } =
        "Organic Oat Milk, Vanilla";

    public int Score { get; init; } = 82;

    public string ServingSize { get; init; } =
        "1 Cup (240ml)";

    public string Calories { get; init; } = "130";
    public string TotalFat { get; init; } = "14g";
    public string SaturatedFat { get; init; } = "0.5g";
    public string Sodium { get; init; } = "110mg";
    public string Carbohydrates { get; init; } = "20g";
    public string Sugars { get; init; } = "7g";
    public string AddedSugars { get; init; } = "7g";
    public string Protein { get; init; } = "3g";

    public string FullIngredients { get; init; } =
        """
        Oat Base (water, oats),
        cane sugar,
        gellan gum,
        sea salt,
        natural vanilla flavor,
        calcium carbonate,
        dipotassium phosphate,
        vitamin A palmitate,
        vitamin D2,
        riboflavin,
        vitamin B12.
        """;
}