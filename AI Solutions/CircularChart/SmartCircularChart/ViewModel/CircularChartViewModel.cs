using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Globalization;
using System.Text.Json;
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

        private readonly IAzureAIService aiService;
        private ObservableCollection<CircularChartData> chartData = new();
        private string userPrompt = string.Empty;
        private bool isBusy;
        private string statusMessage = "Enter a prompt to regroup the chart.";
        private PieGroupMode groupMode = PieGroupMode.Value;
        private double groupTo = double.NaN;

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
                if (!groupTo.Equals(value))
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
                var response = await aiService.GetResultsFromAI(UserPrompt, ChartData);

                if (!TryParseAIGrouping(response, out var mode, out var threshold))
                {
                    StatusMessage = "The AI did not return a valid grouping. Check the endpoint, key and deployment, or rephrase the prompt.";
                    return;
                }

                // The series merges the small slices into "Others" based on these two values.
                GroupMode = mode;
                GroupTo = threshold;

                StatusMessage = double.IsNaN(threshold)
                    ? "Chart updated from AI response: grouping removed."
                    : $"Chart updated from AI response: {mode} grouping, GroupTo = {threshold.ToString("0.##", CultureInfo.InvariantCulture)}.";
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

        // Expected AI reply: {"GroupMode":"Value|Percentage|Angle","GroupTo":25} (GroupTo null removes the grouping).
        private static bool TryParseAIGrouping(string response, out PieGroupMode mode, out double threshold)
        {
            mode = PieGroupMode.Value;
            threshold = double.NaN;

            if (string.IsNullOrWhiteSpace(response))
            {
                return false;
            }

            var start = response.IndexOf('{');
            var end = response.LastIndexOf('}');
            if (start < 0 || end <= start)
            {
                return false;
            }

            try
            {
                using var document = JsonDocument.Parse(response[start..(end + 1)]);
                var root = document.RootElement;

                if (!root.TryGetProperty("GroupMode", out var modeElement) ||
                    !Enum.TryParse(modeElement.GetString(), true, out mode) ||
                    !Enum.IsDefined(mode) ||
                    !root.TryGetProperty("GroupTo", out var toElement))
                {
                    return false;
                }

                if (toElement.ValueKind == JsonValueKind.Null)
                {
                    return true;
                }

                if (toElement.ValueKind == JsonValueKind.Number && toElement.TryGetDouble(out threshold) && threshold >= 0)
                {
                    return true;
                }

                threshold = double.NaN;
                return false;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
