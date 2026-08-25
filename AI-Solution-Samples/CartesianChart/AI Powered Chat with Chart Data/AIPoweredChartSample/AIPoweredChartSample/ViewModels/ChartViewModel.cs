using Syncfusion.Maui.AIAssistView;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Text.RegularExpressions;

namespace AIPoweredChartSample
{
    /// <summary>
    /// ChartViewModel is a view model class that manages the data and commands for a chart view, including handling user interactions with an assist view and retrieving AI-generated responses based on user queries related to the chart data.
    /// </summary>
    public class ChartViewModel : INotifyPropertyChanged
    {
        private string _xAxisTitle = "Year";
        private string _yAxisTitle = "Units";
        private ICommand assistViewRequestCommand = null!;
        private ObservableCollection<IAssistItem> _assistItems;
        private readonly IAzureAIService azureAIService;
        private IAssistItem? requestItem;
        private ObservableCollection<ISuggestion> _suggestions;

        /// <summary>
        /// This constructor initializes a new instance of the ChartViewModel class.
        /// </summary>
        public ChartViewModel(IAzureAIService azureAIService)
        {
            PreparingChartData();
            this.azureAIService = azureAIService;
            this._assistItems = new ObservableCollection<IAssistItem>();
            _suggestions = new ObservableCollection<ISuggestion>
            {
                new AssistSuggestion { Text = "What is the highest Units sale and which year?" },
                new AssistSuggestion { Text = "Which year had lowest production?" }
            };

            this.AssistViewRequestCommand = new Command<object>(ExecuteRequestCommand);
            this.CopyCommand = new Command(async (obj) => await ExecuteCopyCommand(obj));
            this.RetryCommand = new Command(async (obj) => await ExecuteRetryCommandAsync(obj));
        }

        public string ChartTitle => "Laptop Manufacturing Company";

        /// <summary>
        /// Gets the collection of chart points representing production data for the chart.
        /// </summary>
        public ObservableCollection<ChartPoint> ProductionPoints { get; } = new ObservableCollection<ChartPoint>();

        /// <summary>
        /// Gets the collection of chart points representing sales data for the chart.
        /// </summary>
        public ObservableCollection<ChartPoint> SalesPoints { get; } = new ObservableCollection<ChartPoint>();

        /// <summary>
        /// Gets or sets the command that copies the selected content to the clipboard.
        /// </summary>
        public ICommand CopyCommand { get; }

        /// <summary>
        /// Gets or sets the command that executes a retry operation.
        /// </summary>
        public ICommand RetryCommand { get; }

        /// <summary>
        /// Gets or sets the command that is executed when a suggestion item is selected in the assist view.
        /// </summary>
        public ICommand AssistViewRequestCommand
        {
            get
            {
                return this.assistViewRequestCommand;
            }
            set
            {
                this.assistViewRequestCommand = value;
            }
        }

        /// <summary>
        /// Gets or sets the title for the X-axis of the chart.
        /// </summary>
        public string XAxisTitle
        {
            get => _xAxisTitle;
            set { _xAxisTitle = value; RaisePropertyChanged(nameof(XAxisTitle)); }
        }

        /// <summary>
        /// Gets or sets the title for the Y-axis of the chart.
        /// </summary>
        public string YAxisTitle
        {
            get => _yAxisTitle;
            set { _yAxisTitle = value; RaisePropertyChanged(nameof(YAxisTitle)); }
        }

        /// <summary>
        /// Gets or sets the collection of assist items associated with this instance.
        /// </summary>
        public ObservableCollection<IAssistItem> AssistItems
        {
            get
            {
                return this._assistItems;
            }

            set
            {
                this._assistItems = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets the collection of suggestions that can be displayed in the assist view for user interaction.
        /// </summary>
        public ObservableCollection<ISuggestion> Suggestions => _suggestions;

        /// <summary>
        /// Method to execute when the copy command is triggered.
        /// </summary>
        private async Task ExecuteCopyCommand(object obj)
        {
            if (obj is AssistItem assistItem && assistItem.Text != null)
            {
                string text = assistItem.Text;
                text = Regex.Replace(text, "<.*?>|&nbsp;", string.Empty);
                await Clipboard.SetTextAsync(text);
            }
        }

        /// <summary>
        /// Method to execute when the retry command is triggered.
        /// </summary>
        private async Task ExecuteRetryCommandAsync(object obj)
        {
            if (obj is AssistItem assistItem && assistItem.RequestItem is IAssistItem item)
            {
                requestItem = item;
                await this.GetResult(requestItem).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Method to execute when a suggestion item is selected in the assist view.
        /// </summary>
        /// <param name="obj"></param>
        private async void ExecuteRequestCommand(object obj)
        {
            var requestEventArgs = obj as RequestEventArgs;
            if (requestEventArgs?.RequestItem == null) return;
            var request = requestEventArgs.RequestItem;
            await GetResult(request).ConfigureAwait(true);
        }

        /// <summary>
        /// Method to prepare the chart data for production and sales.
        /// </summary>
        public void PreparingChartData()
        {
            var startYear = 2016;
            var endYear = DateTime.Now.Year;

            var rnd = new Random();
            // Define reasonable low/high ranges for production and a sales factor range
            double prodMin = 600; // lower bound for production units
            double prodMax = 2200; // upper bound for production units
            double salesFactorMin = 0.55; // sales as fraction of production
            double salesFactorMax = 0.95;

            for (int year = startYear; year <= endYear; year++)
            {
                // Randomized production within the range
                double production = Math.Round(prodMin + rnd.NextDouble() * (prodMax - prodMin));
                // Sales is a randomized fraction of production to give variation
                double sales = Math.Round(production * (salesFactorMin + rnd.NextDouble() * (salesFactorMax - salesFactorMin)));

                ProductionPoints.Add(new ChartPoint { Year = year.ToString(), Units = production });
                SalesPoints.Add(new ChartPoint { Year = year.ToString(), Units = sales });
            }
        }

        /// <summary>
        /// Method to get the result from the Azure AI service based on the user query and the provided chart data.
        /// </summary>
        private async Task GetResult(object inputQuery)
        {
            await Task.Delay(1000).ConfigureAwait(true);
            AssistItem request = (AssistItem)inputQuery;
            var question = request.Text;

            // Build data lists
            var prodList = string.Join(", ", ProductionPoints.Select(p => $"{p.Year}:{p.Units}"));
            var salesList = string.Join(", ", SalesPoints.Select(p => $"{p.Year}:{p.Units}"));

            var prompt = $@"You are given two datasets for laptop production and sales by year.
                         Production data (year:units): {prodList}
                         Sales data (year:units): {salesList}
                         Question: {question}
                         Instructions: Analyze ONLY the provided data above. Do NOT perform any calculations outside this data. Provide a single concise answer and nothing else. If the question asks for a multiple years and a values, reply in the format: YEAR - VALUE units, YEAR - VALUE units.";
            if (request != null)
            {
                var response = await azureAIService!.GetResultsFromAI(prompt).ConfigureAwait(true);
                response = response.Replace("\r\n", "<br>").Replace("\n", "<br>");
                AssistItem responseItem = new AssistItem() { Text = response };
                responseItem.RequestItem = inputQuery;
                this.AssistItems.Add(responseItem);
            }
        }

        /// <summary>
        /// Event that is raised when a property value changes, allowing the UI to update accordingly.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Occurs when property is changed.
        /// </summary>
        /// <param name="propName">changed property name; automatically supplied by the compiler when omitted.</param>
        public void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName ?? string.Empty));
        }
    }
}