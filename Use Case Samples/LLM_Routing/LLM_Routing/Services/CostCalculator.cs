namespace LLM_Routing.Services;

/// <summary>
/// Per-1K-token pricing (USD) for each Azure OpenAI deployment.
///
/// Prices are read from environment variables so the cost rate can be made accurate
/// without changing code. Set these per model (model name uppercased, with '.', '-'
/// replaced by '_'):
///   AZURE_PRICE_INPUT_{MODEL}   — USD per 1K input tokens
///   AZURE_PRICE_OUTPUT_{MODEL}  — USD per 1K output tokens
///
/// Examples:
///   AZURE_PRICE_INPUT_GPT_4_1_NANO
///   AZURE_PRICE_OUTPUT_GPT_4_1_MINI
///
/// If a per-model value is not set, the constants below are used as defaults so the
/// app still produces a number. Adjust those defaults to your billing plan.
/// </summary>
public static class CostCalculator
{
    private const decimal NanoInputPer1K = 0.0001m;
    private const decimal NanoOutputPer1K = 0.0004m;
    private const decimal MiniInputPer1K = 0.0004m;
    private const decimal MiniOutputPer1K = 0.0016m;
    private const decimal LunaInputPer1K = 0.003m;
    private const decimal LunaOutputPer1K = 0.012m;

    public static decimal EstimateUsd(string model, int inputTokens, int outputTokens)
    {
        var (defaultIn, defaultOut) = model switch
        {
            "gpt-4.1-nano" => (NanoInputPer1K, NanoOutputPer1K),
            "gpt-4.1-mini" => (MiniInputPer1K, MiniOutputPer1K),
            "gpt-5.6-luna" => (LunaInputPer1K, LunaOutputPer1K),
            _ => (NanoInputPer1K, NanoOutputPer1K)
        };

        var inPer1K = ReadPrice("AZURE_PRICE_INPUT", model, defaultIn);
        var outPer1K = ReadPrice("AZURE_PRICE_OUTPUT", model, defaultOut);

        var cost = (inputTokens / 1000m) * inPer1K + (outputTokens / 1000m) * outPer1K;
        return Math.Round(cost, 6);
    }

    public static string FormatUsd(decimal cost)
        => cost < 0.01m
            ? $"${cost:F4}"
            : $"${cost:F3}";

    private static decimal ReadPrice(string prefix, string model, decimal fallback)
    {
        if (string.IsNullOrWhiteSpace(model))
            return fallback;

        var envName = $"{prefix}_{ModelToEnvSuffix(model)}";
        var raw = Environment.GetEnvironmentVariable(envName);
        return decimal.TryParse(raw, System.Globalization.NumberStyles.Number,
                   System.Globalization.CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : fallback;
    }

    /// <summary>
    /// Converts a deployment/model name into the env-var suffix form:
    /// uppercased, with '.', '-', ' ' replaced by '_'.
    /// </summary>
    public static string ModelToEnvSuffix(string model)
    {
        var chars = model.Select(c =>
            char.IsLetterOrDigit(c) ? char.ToUpperInvariant(c) : '_').ToArray();
        return new string(chars);
    }
}
