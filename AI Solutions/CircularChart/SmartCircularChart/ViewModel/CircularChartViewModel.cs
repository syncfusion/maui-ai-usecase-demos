using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace SmartCircularChart
{
    public class CircularChartViewModel : INotifyPropertyChanged
    {
        public List<Brush> CustomBrushes { get; }

        private static readonly (string Category, double Value)[] DefaultProducts =
        [
            ("Laptop", 102),
            ("Smartphone", 106),
            ("Tablet", 98),
            ("Smartwatch", 84),
            ("Headphones", 76),
            ("Camera", 68),
            ("Speaker", 62),
            ("Monitor", 90),
            ("Keyboard", 54),
            ("Mouse", 48),
            ("Printer", 58),
            ("Router", 46)
        ];

        private readonly IAzureAIService aiService;
        private ObservableCollection<CircularChartData> chartData = new();
        private string userPrompt = string.Empty;
        private bool isBusy;
        private string statusMessage = "Enter a prompt to regroup the chart.";
        private string groupMode = "Value";
        private double groupTo = 15d;

        public CircularChartViewModel(IAzureAIService aiService)
        {
            this.aiService = aiService;
            CustomBrushes =
            [
                new SolidColorBrush(Color.FromArgb("#EF3E23")),
                new SolidColorBrush(Color.FromArgb("#F47C20")),
                new SolidColorBrush(Color.FromArgb("#F5B126")),
                new SolidColorBrush(Color.FromArgb("#F9DE2A")),
                new SolidColorBrush(Color.FromArgb("#D9ED2B")),
                new SolidColorBrush(Color.FromArgb("#7ED321")),
                new SolidColorBrush(Color.FromArgb("#53A93F")),
                new SolidColorBrush(Color.FromArgb("#5EC97E")),
                new SolidColorBrush(Color.FromArgb("#67D0B1")),
                new SolidColorBrush(Color.FromArgb("#8DDDE6")),
                new SolidColorBrush(Color.FromArgb("#9FD1F8")),
                new SolidColorBrush(Color.FromArgb("#5C8AE6")),
                new SolidColorBrush(Color.FromArgb("#7D71E8")),
                new SolidColorBrush(Color.FromArgb("#9362DA")),
                new SolidColorBrush(Color.FromArgb("#B05AD6")),
                new SolidColorBrush(Color.FromArgb("#D54E9A"))
            ];
            LoadDefaultData();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<CircularChartData> ChartData
        {
            get => chartData;
            set
            {
                if (chartData != value)
                {
                    chartData = value;
                    OnPropertyChanged();
                }
            }
        }

        public string UserPrompt
        {
            get => userPrompt;
            set
            {
                if (userPrompt != value)
                {
                    userPrompt = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsBusy
        {
            get => isBusy;
            set
            {
                if (isBusy != value)
                {
                    isBusy = value;
                    OnPropertyChanged();
                }
            }
        }

        public string StatusMessage
        {
            get => statusMessage;
            set
            {
                if (statusMessage != value)
                {
                    statusMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public string GroupMode
        {
            get => groupMode;
            set
            {
                if (groupMode != value)
                {
                    groupMode = value;
                    OnPropertyChanged();
                }
            }
        }

        public double GroupTo
        {
            get => groupTo;
            set
            {
                if (Math.Abs(groupTo - value) > double.Epsilon)
                {
                    groupTo = value;
                    OnPropertyChanged();
                }
            }
        }

        public async Task GroupDataAsync()
        {
            if (string.IsNullOrWhiteSpace(UserPrompt))
            {
                StatusMessage = "Please enter a prompt to update the chart.";
                return;
            }

            IsBusy = true;
            StatusMessage = "Processing prompt...";

            try
            {
                var response = await aiService.GetResultsFromAI(UserPrompt);
                var groupedData = ParseGroupingResponse(response);

                if (groupedData.Count == 0)
                {
                    groupedData = BuildFallbackGrouping(UserPrompt);
                    if (groupedData.Count == 0)
                    {
                        StatusMessage = "Please enter a valid chart prompt.";
                        return;
                    }

                    StatusMessage = "AI response was unavailable, so default grouping rules were used.";
                }

                ApplyChartControlsFromPrompt(UserPrompt, groupedData);
                ChartData = new ObservableCollection<CircularChartData>(groupedData);
                StatusMessage = string.IsNullOrWhiteSpace(response)
                    ? "Chart updated using prompt rules."
                    : "Chart updated from AI response.";
            }
            catch
            {
                var fallbackData = BuildFallbackGrouping(UserPrompt);
                if (fallbackData.Count == 0)
                {
                    StatusMessage = "Please enter a valid chart prompt.";
                    return;
                }

                ApplyChartControlsFromPrompt(UserPrompt, fallbackData);
                ChartData = new ObservableCollection<CircularChartData>(fallbackData);
                StatusMessage = "AI request failed, so default grouping rules were used.";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private static List<CircularChartData> BuildFallbackGrouping(string prompt)
        {
            var normalizedPrompt = prompt.ToLowerInvariant();
            var threshold = ExtractThreshold(prompt);

            if (normalizedPrompt.Contains("percentage"))
            {
                return BuildGroupedData("Percentage", threshold ?? 8d);
            }

            if (normalizedPrompt.Contains("angle"))
            {
                return BuildGroupedData("Angle", threshold ?? 20d);
            }

            if (normalizedPrompt.Contains("value"))
            {
                return BuildGroupedData("Value", threshold ?? 15d);
            }

            return BuildGroupedData("Value", 15d);
        }

        private static List<CircularChartData> BuildGroupedData(string groupMode, double groupTo)
        {
            var data = DefaultProducts
                .Select((item, index) => new CircularChartData
                {
                    Category = item.Category,
                    Value = item.Value,
                    Group = index % 4 == 0 ? "Others" : $"Segment {index + 1}"
                })
                .ToList();

            foreach (var item in data)
            {
                var shouldGroup = groupMode switch
                {
                    "Percentage" => item.Value <= groupTo,
                    "Angle" => item.Value <= groupTo,
                    _ => item.Value <= groupTo
                };

                if (shouldGroup)
                {
                    item.Group = "Others";
                }
            }

            return data;
        }

        private void LoadDefaultData()
        {
            ChartData = new ObservableCollection<CircularChartData>(
                DefaultProducts.Select((item, index) => new CircularChartData
                {
                    Category = item.Category,
                    Value = item.Value,
                    Group = index % 4 == 0 ? "Others" : $"Segment {index + 1}"
                }));
        }

        private static List<CircularChartData> ParseGroupingResponse(string response)
        {
            if (string.IsNullOrWhiteSpace(response))
            {
                return [];
            }

            try
            {
                var json = ExtractJsonArray(response);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return [];
                }

                var items = JsonSerializer.Deserialize<List<CircularChartData>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return items ?? [];
            }
            catch
            {
                return [];
            }
        }

        private static string ExtractJsonArray(string response)
        {
            var start = response.IndexOf('[');
            if (start < 0)
            {
                return string.Empty;
            }

            var end = response.LastIndexOf(']');
            if (end <= start)
            {
                return string.Empty;
            }

            return response[start..(end + 1)];
        }

        private void ApplyChartControlsFromPrompt(string prompt, List<CircularChartData> groupedData)
        {
            var normalizedPrompt = prompt.ToLowerInvariant();

            if (normalizedPrompt.Contains("percentage"))
            {
                GroupMode = "Percentage";
                GroupTo = ExtractThreshold(prompt) ?? 8d;
                return;
            }

            if (normalizedPrompt.Contains("angle"))
            {
                GroupMode = "Angle";
                GroupTo = ExtractThreshold(prompt) ?? 20d;
                return;
            }

            if (normalizedPrompt.Contains("value"))
            {
                GroupMode = "Value";
                GroupTo = ExtractThreshold(prompt) ?? 15d;
                return;
            }

            if (normalizedPrompt.Contains("category") || normalizedPrompt.Contains("product") || normalizedPrompt.Contains("group"))
            {
                GroupMode = "Value";
                GroupTo = ExtractThreshold(prompt) ?? 15d;
                return;
            }

            if (groupedData.Any(item => string.Equals(item.Group, "Others", StringComparison.OrdinalIgnoreCase)))
            {
                GroupMode = "Value";
                GroupTo = ExtractThreshold(prompt) ?? 15d;
            }
        }

        private static double? ExtractThreshold(string prompt)
        {
            var match = System.Text.RegularExpressions.Regex.Match(prompt, @"(\d+(?:\.\d+)?)");
            return match.Success && double.TryParse(match.Value, out var value) ? value : null;
        }

        private static string BuildPromptSummary(string prompt)
        {
            var normalizedPrompt = prompt.ToLowerInvariant();

            if (normalizedPrompt.Contains("percentage"))
            {
                return $"Percentage grouping under {ExtractThreshold(prompt) ?? 8d}";
            }

            if (normalizedPrompt.Contains("angle"))
            {
                return $"Angle grouping under {ExtractThreshold(prompt) ?? 20d}";
            }

            if (normalizedPrompt.Contains("value"))
            {
                return $"Value grouping under {ExtractThreshold(prompt) ?? 15d}";
            }

            if (normalizedPrompt.Contains("category") || normalizedPrompt.Contains("product") || normalizedPrompt.Contains("group"))
            {
                return "Category-based grouping";
            }

            return "Default grouping";
        }

        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
