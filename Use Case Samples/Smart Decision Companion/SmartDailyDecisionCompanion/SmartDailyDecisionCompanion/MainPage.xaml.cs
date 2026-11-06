namespace SmartDailyDecisionCompanion
{
    /// <summary>
    /// Code-behind for the main page. Task state, tab selection, and the Accept/Postpone
    /// actions all live in <see cref="MainViewModel"/>; the tab views stay in step through the
    /// two-way SelectedIndex binding declared in MainPage.xaml. This class triggers the
    /// awaited startup credential check once the page is visible and shows any alert the
    /// ViewModel requests — the view owns the platform actions.
    /// </summary>
    public partial class MainPage : ContentPage
    {
        private bool _isInitialized;

        public MainPage(MainViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
            vm.OnCredentialsInvalidRequested += OnCredentialsInvalidRequested;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Credential validation performs I/O, so it is awaited here rather than fired
            // from the AI service constructor, which keeps the outcome deterministic.
            if (_isInitialized || BindingContext is not MainViewModel vm)
                return;

            _isInitialized = true;
            await vm.InitializeAsync();
        }

        private async void OnCredentialsInvalidRequested()
        {
            // The AI service awaits with ConfigureAwait(false), so the event can be raised
            // from a thread-pool thread — marshal the platform alert onto the main thread.
            if (!MainThread.IsMainThread)
            {
                MainThread.BeginInvokeOnMainThread(OnCredentialsInvalidRequested);
                return;
            }

            await DisplayAlertAsync(
                "Alert",
                "The Azure API key or endpoint is missing or incorrect. Please verify your credentials in Services/AzureBaseService.cs.",
                "OK");
        }
    }
}