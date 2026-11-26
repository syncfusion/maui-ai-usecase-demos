using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Globalization;
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

        private readonly IGroupingExtractionService extractionService;
        private readonly GroupingResponseValidator validator;
        private CancellationTokenSource? generationCts;
        private ObservableCollection<CircularChartData> chartData = new();
        private string userPrompt = string.Empty;
        private bool isBusy;
        private string statusMessage = "Enter a prompt to regroup the chart.";
        private PieGroupMode groupMode = PieGroupMode.Value;
        private double groupTo = double.NaN;

        public CircularChartViewModel(IGroupingExtractionService extractionService, GroupingResponseValidator validator)
        {
            this.extractionService = extractionService;
            this.validator = validator;
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
            var prompt = UserPrompt.Trim();
            if (string.IsNullOrWhiteSpace(prompt))
            {
                StatusMessage = "Please enter a prompt to update the chart.";
                return;
            }

            generationCts?.Cancel();
            generationCts?.Dispose();
            var cts = generationCts = new CancellationTokenSource();

            IsBusy = true;
            StatusMessage = "Processing prompt...";

            try
            {
                var grouping = await extractionService.ExtractAsync(prompt, ChartData, cts.Token);
                cts.Token.ThrowIfCancellationRequested();

                if (!validator.Validate(grouping, out var errorMessage))
                {
                    StatusMessage = errorMessage;
                    return;
                }

                // The series merges the small slices into "Others" based on these two values.
                GroupMode = grouping.GroupMode;
                GroupTo = grouping.GroupTo ?? double.NaN;

                StatusMessage = grouping.GroupTo is null
                    ? "Grouping removed."
                    : $"Chart updated: {grouping.GroupMode} grouping, GroupTo = {grouping.GroupTo.Value.ToString("0.##", CultureInfo.InvariantCulture)}.";
            }
            catch (OperationCanceledException)
            {
                StatusMessage = "Update canceled.";
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Azure OpenAI credentials are not configured.", StringComparison.OrdinalIgnoreCase))
            {
                StatusMessage = "Azure OpenAI credentials are required to update the chart.";
            }
            catch (InvalidOperationException ex) when (ex.Message.StartsWith("Azure OpenAI request failed:", StringComparison.OrdinalIgnoreCase))
            {
                StatusMessage = ex.Message;
            }
            catch (InvalidOperationException ex) when (ex.Message.StartsWith("Azure OpenAI returned", StringComparison.OrdinalIgnoreCase))
            {
                StatusMessage = "Azure OpenAI returned a response that the app could not parse.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Unable to update the chart right now: {ex.Message}";
            }
            finally
            {
                if (ReferenceEquals(generationCts, cts))
                {
                    IsBusy = false;
                    generationCts = null;
                }

                cts.Dispose();
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

        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
