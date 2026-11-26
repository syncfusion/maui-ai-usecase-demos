namespace SmartCircularChart;

public interface IGroupingExtractionService
{
    Task<GroupingResponse> ExtractAsync(string prompt, IEnumerable<CircularChartData> chartData, CancellationToken cancellationToken = default);
}
