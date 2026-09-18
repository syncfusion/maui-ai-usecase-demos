using System.Text.Json.Serialization;

namespace StockChart.Models;

public sealed class CandleDataModel
{
    [JsonPropertyName("Date")]
    public DateTime Date { get; init; }

    [JsonPropertyName("Open")]
    public double Open { get; init; }

    [JsonPropertyName("High")]
    public double High { get; init; }

    [JsonPropertyName("Low")]
    public double Low { get; init; }

    [JsonPropertyName("Close")]
    public double Close { get; init; }

    [JsonPropertyName("Volume")]
    public double Volume { get; init; }
}
