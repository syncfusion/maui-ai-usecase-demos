using System.Text.Json;
using StockChart.Models;

namespace StockChart.Services;

public sealed class StockDataService
{
    private static readonly string[] StockFiles =
    [
        "apple.json",
        "facebook.json",
        "google.json",
        "microsoft.json",
        "nestle.json",
        "netflix.json",
        "tesla.json"
    ];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<IReadOnlyList<StockModel>> LoadAsync(CancellationToken cancellationToken = default)
    {
        var stocks = new List<StockModel>();

        foreach (var fileName in StockFiles)
        {
            await using var stream = await FileSystem.OpenAppPackageFileAsync($"Stocks/{fileName}");
            var stock = await JsonSerializer.DeserializeAsync<StockModel>(stream, JsonOptions, cancellationToken)
                ?? throw new InvalidDataException($"The stock file '{fileName}' is empty.");

            Validate(stock, fileName);
            stock.Data.Sort((left, right) => left.Date.CompareTo(right.Date));
            stocks.Add(stock);
        }

        return stocks.OrderBy(stock => stock.Company).ToArray();
    }

    private static void Validate(StockModel stock, string fileName)
    {
        if (string.IsNullOrWhiteSpace(stock.Stock) ||
            string.IsNullOrWhiteSpace(stock.Company) ||
            string.IsNullOrWhiteSpace(stock.OfficialName) ||
            string.IsNullOrWhiteSpace(stock.Exchange) ||
            stock.Data.Count == 0)
        {
            throw new InvalidDataException($"The stock file '{fileName}' is missing required data.");
        }

        if (stock.Data.Any(candle => candle.Date == default ||
                                     candle.Open <= 0 ||
                                     candle.High <= 0 ||
                                     candle.Low <= 0 ||
                                     candle.Close <= 0 ||
                                     candle.High < candle.Low ||
                                     candle.Volume < 0))
        {
            throw new InvalidDataException($"The stock file '{fileName}' contains an invalid candle.");
        }

        if (stock.Data.Select(candle => candle.Date).Distinct().Count() != stock.Data.Count)
        {
            throw new InvalidDataException($"The stock file '{fileName}' contains duplicate dates.");
        }
    }
}
