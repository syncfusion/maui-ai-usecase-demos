using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace StockChart.Models;

public partial class StockModel : ObservableObject
{
    [JsonPropertyName("Stock")]
    public string Stock { get; init; } = string.Empty;

    [JsonPropertyName("Company")]
    public string Company { get; init; } = string.Empty;

    [JsonPropertyName("OfficialName")]
    public string OfficialName { get; init; } = string.Empty;

    [JsonPropertyName("Exchange")]
    public string Exchange { get; init; } = string.Empty;

    [JsonPropertyName("isFavorite")]
    public bool IsFavorite { get; init; }

    [JsonPropertyName("Data")]
    public List<CandleDataModel> Data { get; init; } = [];

    [JsonIgnore]
    public string Symbol => Stock.ToUpperInvariant() switch
    {
        "APPLE" => "AAPL",
        "FACEBOOK" => "META",
        "GOOGLE" => "GOOGL",
        "MICROSOFT" => "MSFT",
        "NESTLE" => "NESN",
        "NETFLIX" => "NFLX",
        "TESLA" => "TSLA",
        _ => Stock.ToUpperInvariant()
    };

    [JsonIgnore]
    public CandleDataModel LatestCandle => Data[^1];

    [JsonIgnore]
    public string PriceText => $"${LatestCandle.Close:N2}";

    [JsonIgnore]
    public string ChangeText => $"{LatestCandle.Close - LatestCandle.Open:+0.00;-0.00;0.00} ({(LatestCandle.Close - LatestCandle.Open) / LatestCandle.Open:+0.00%;-0.00%;0.00%})";

    [JsonIgnore]
    public string ExchangeText => Exchange;

    [JsonIgnore]
    public string LogoImageSource => $"{Stock.ToLowerInvariant()}.png";

    [ObservableProperty]
    public partial bool IsSelected { get; set; }
}
