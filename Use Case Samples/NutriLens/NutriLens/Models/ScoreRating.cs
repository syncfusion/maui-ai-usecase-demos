namespace NutriLens.Models;

public static class ScoreRating
{
    public static string GetCategory(int score) => Math.Clamp(score, 0, 100) switch
    {
        >= 80 => "Excellent",
        >= 60 => "Good",
        >= 40 => "Moderate",
        >= 20 => "Poor",
        _ => "Unknown"
    };

    public static string GetColor(int score) => GetCategory(score) switch
    {
        "Excellent" or "Good" => "#047857",
        "Moderate" => "#D99024",
        "Poor" => "#DC2626",
        _ => "#64748B"
    };

    public static bool IsHealthyChoice(int score) => score >= 60;
}