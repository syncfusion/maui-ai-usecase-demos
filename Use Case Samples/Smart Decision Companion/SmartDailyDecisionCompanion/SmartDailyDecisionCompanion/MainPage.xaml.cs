namespace SmartDailyDecisionCompanion
{
    /// <summary>
    /// Code-behind for the main page. Task state, tab selection, and the Accept/Postpone
    /// actions all live in <see cref="MainViewModel"/>; the tab views stay in step through the
    /// two-way SelectedIndex binding declared in MainPage.xaml. This class only triggers the
    /// awaited startup credential check once the page is visible.
    /// </summary>
    public partial class MainPage : ContentPage
    {
        private bool _isInitialized;

        public MainPage(MainViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
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
    }
}