namespace AIPoweredWritingAssistant
{
    using Syncfusion.Maui.AIAssistView;
    using Microsoft.Extensions.DependencyInjection;
    using System.Threading.Tasks;

    /// <summary>
    /// Class representing the main page of the application, which serves as the primary user interface for interacting with the assist view and related features.
    /// </summary>
    public partial class MainPage : ContentPage
    {
        #region Field

        /// <summary>
        /// Represents the view model used to manage the state and behavior of the assist view.
        /// </summary>
        AssistViewViewModel assistViewViewModel;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the MainPage class.
        /// </summary>
        public MainPage()
        {
            InitializeComponent();
            var ai = ServiceHelper.Services.GetRequiredService<IAzureAIService>();
            assistViewViewModel = new AssistViewViewModel(ai);
            this.BindingContext = assistViewViewModel;
            ComboBoxViewModel comboBoxViewModel = new ComboBoxViewModel();
            comboBox.ItemsSource = comboBoxViewModel.Features;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handles the click event for the button, performing platform-specific navigation or UI updates in response to user interaction.
        /// </summary>
        /// <param name="sender">The sender of the event, typically the button that was clicked.</param>
        /// <param name="e">Event arguments associated with the button click event.</param>
        private async void OnAIButtonClicked(object sender, EventArgs e)
        {
#if ANDROID || (IOS && !MACCATALYST)
            // On Android and iOS, navigate to a dedicated page showing the AssistView
            var navParams = new Dictionary<string, object>
            {
                { "vm", assistViewViewModel }
            };
            await Shell.Current.GoToAsync(nameof(AssistViewPage), true, navParams);
#else
            // Keep existing behavior (Windows and other platforms)
            Grid.SetColumnSpan(richTextEditor, 1);
            assistViewGrid.IsVisible = true;
            AiButton.IsVisible = false;
            headerView.IsVisible = true;
            AssistView.IsVisible = true;
#endif
        }

        /// <summary>
        /// Handles the click event for the close button, hiding the header and assist views.
        /// </summary>
        /// <param name="sender">The sender of the event, typically the button that was clicked.</param>
        /// <param name="e">Event arguments associated with the button click event.</param>
        private void OnCloseButtonClicked(object sender, EventArgs e)
        {
            Grid.SetColumnSpan(richTextEditor, 2);
            this.assistViewViewModel.AssistItems.Clear();
            assistViewGrid.IsVisible = false;
            headerView.IsVisible = false;
            AssistView.IsVisible = false;
            AiButton.IsVisible = true;
        }

        /// <summary>
        /// Handles the SelectionChanged event of the ComboBox control.
        /// </summary>
        /// <param name="sender">The source of the event, typically the ComboBox whose selection has changed.</param>
        /// <param name="e">A SelectionChangedEventArgs object that contains data about the selection change.</param>
        private void comboBox_SelectionChanged(object sender, Syncfusion.Maui.Inputs.SelectionChangedEventArgs e)
        {
            this.assistViewViewModel.GetComboBoxSelection(e);
        }

        #endregion
    }
}