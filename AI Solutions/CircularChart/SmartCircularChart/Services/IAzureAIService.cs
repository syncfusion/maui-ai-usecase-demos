namespace SmartCircularChart
{
    public interface IAzureAIService
    {
        Task<string> GetResultsFromAI(string userAIPrompt, IEnumerable<CircularChartData> chartData);
    }
}
