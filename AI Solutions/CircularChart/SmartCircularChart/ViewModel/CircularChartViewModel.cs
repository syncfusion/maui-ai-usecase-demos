using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using Syncfusion.Maui.Toolkit.Charts;

namespace SmartCircularChart
{
    public class CircularChartViewModel : INotifyPropertyChanged
    {
        public List<Brush> CustomBrushes { get; }

        private static readonly (string Category, double Value)[] DefaultProducts =
        [
            ("Laptop", 240),
            ("Smartphone", 260),
            ("Tablet", 210),
            ("Smartwatch", 85),
            ("Headphones", 75),
            ("Camera", 45),
            ("Speaker", 30),
            ("Monitor", 198),
            ("Keyboard", 15),
            ("Mouse", 11),
            ("Printer", 22),
            ("Router", 9)
        ];

        private const double DefaultGroupTo = 15d;

        private readonly IAzureAIService aiService;
        private ObservableCollection<CircularChartData> chartData = new();
        private string userPrompt = string.Empty;
        private bool isBusy;
        private string statusMessage = "Enter a prompt to regroup the chart.";
        private PieGroupMode groupMode = PieGroupMode.Value;
        private double groupTo = DefaultGroupTo;

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

        public PieGroupMode GroupMode
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
                GroupingRule? rule = null;
                var fromAI = false;

                try
                {
                    rule = ParseAIRule(await aiService.GetResultsFromAI(UserPrompt));
                    fromAI = rule != null;
                }
                catch
                {
                    // Fall back to the local prompt rules below.
                }

                rule ??= ParsePromptRule(UserPrompt);
                if (rule == null)
                {
                    StatusMessage = "Please enter a valid grouping prompt, e.g. 'Group by value under 25'.";
                    return;
                }

                if (!TryResolveGrouping(rule, out var mode, out var threshold))
                {
                    StatusMessage = "The requested amount is not valid for the current data.";
                    return;
                }

                // GroupMode and GroupTo are applied to the series; the chart merges the small slices into "Others".
                GroupMode = mode;
                GroupTo = threshold;

                StatusMessage = $"{(fromAI ? "Chart updated from AI response" : "Chart updated using prompt rules")}: {mode} grouping, GroupTo = {threshold.ToString("0.##", CultureInfo.InvariantCulture)}.";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void LoadDefaultData()
        {
            ChartData = new ObservableCollection<CircularChartData>(
                DefaultProducts.Select(item => new CircularChartData
                {
                    Category = item.Category,
                    Value = item.Value
                }));
        }

        // Converts an intent (threshold or ranking) into the GroupMode / GroupTo pair understood by the series.
        private bool TryResolveGrouping(GroupingRule rule, out PieGroupMode mode, out double threshold)
        {
            mode = PieGroupMode.Value;
            threshold = rule.Amount;

            switch (rule.Kind)
            {
                case RuleKind.Percentage:
                    mode = PieGroupMode.Percentage;
                    return threshold >= 0;

                case RuleKind.Angle:
                    mode = PieGroupMode.Angle;
                    return threshold >= 0;

                case RuleKind.Value:
                    return threshold >= 0;

                case RuleKind.Top:
                {
                    // Keep the N largest slices: merge everything not larger than the (N + 1)th largest value.
                    var count = (int)rule.Amount;
                    var descending = ChartData.Select(item => item.Value).OrderByDescending(v => v).ToList();
                    if (count < 1)
                    {
                        return false;
                    }

                    threshold = count >= descending.Count ? 0 : descending[count];
                    return true;
                }

                case RuleKind.Smallest:
                {
                    // Merge the N smallest slices: merge everything not larger than the Nth smallest value.
                    var count = (int)rule.Amount;
                    var ascending = ChartData.Select(item => item.Value).OrderBy(v => v).ToList();
                    if (count < 1)
                    {
                        return false;
                    }

                    threshold = ascending[Math.Min(count, ascending.Count) - 1];
                    return true;
                }

                default:
                    return false;
            }
        }

        // Expected AI reply: {"Kind":"Value|Percentage|Angle|Top|Smallest","Amount":25}
        private static GroupingRule? ParseAIRule(string response)
        {
            if (string.IsNullOrWhiteSpace(response))
            {
                return null;
            }

            var start = response.IndexOf('{');
            var end = response.LastIndexOf('}');
            if (start < 0 || end <= start)
            {
                return null;
            }

            try
            {
                using var document = JsonDocument.Parse(response[start..(end + 1)]);
                var root = document.RootElement;

                if (!root.TryGetProperty("Kind", out var kindElement) ||
                    !Enum.TryParse<RuleKind>(kindElement.GetString(), true, out var kind) ||
                    !root.TryGetProperty("Amount", out var amountElement) ||
                    !amountElement.TryGetDouble(out var amount))
                {
                    return null;
                }

                return new GroupingRule(kind, amount);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static GroupingRule? ParsePromptRule(string prompt)
        {
            var text = prompt.ToLowerInvariant();

            var top = Regex.Match(text, @"\b(?:top|largest|biggest|highest)\s+(\d+)");
            if (top.Success)
            {
                return new GroupingRule(RuleKind.Top, double.Parse(top.Groups[1].Value, CultureInfo.InvariantCulture));
            }

            var smallest = Regex.Match(text, @"\b(?:smallest|lowest|bottom|least)\s+(\d+)");
            if (smallest.Success)
            {
                return new GroupingRule(RuleKind.Smallest, double.Parse(smallest.Groups[1].Value, CultureInfo.InvariantCulture));
            }

            var number = Regex.Match(text, @"\d+(?:\.\d+)?");
            double? amount = number.Success ? double.Parse(number.Value, CultureInfo.InvariantCulture) : null;

            if (text.Contains("degree") || text.Contains("angle"))
            {
                return new GroupingRule(RuleKind.Angle, amount ?? 20d);
            }

            if (text.Contains("percent") || text.Contains('%'))
            {
                return new GroupingRule(RuleKind.Percentage, amount ?? 8d);
            }

            if (text.Contains("value") || text.Contains("category") || text.Contains("product") || text.Contains("group") || text.Contains("merge"))
            {
                return new GroupingRule(RuleKind.Value, amount ?? DefaultGroupTo);
            }

            return null;
        }

        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private enum RuleKind
        {
            Value,
            Percentage,
            Angle,
            Top,
            Smallest
        }

        private sealed record GroupingRule(RuleKind Kind, double Amount);
    }
}
