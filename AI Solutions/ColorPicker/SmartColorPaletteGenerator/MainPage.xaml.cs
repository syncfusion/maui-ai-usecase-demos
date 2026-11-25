namespace SmartColorPaletteGenerator
{
    public partial class MainPage : ContentPage
    {
        private readonly MainViewModel viewModel;

        /// <summary>
        /// Initializes a new instance of the MainPage class with the specified MainViewModel.
        /// </summary>
        /// <param name="viewModel"></param>
        public MainPage(MainViewModel viewModel)
        {
            InitializeComponent();
            this.viewModel = viewModel;
            BindingContext = this.viewModel;
            BindColorPickers();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            await viewModel.GeneratePaletteAsync();
            BindColorPickers();
        }

        private void BindColorPickers()
        {
            SetPaletteColors(FirstColorPicker, viewModel.FirstPalette.Colors);
            SetPaletteColors(SecondColorPicker, viewModel.SecondPalette.Colors);
            SetPaletteColors(ThirdColorPicker, viewModel.ThirdPalette.Colors);
        }

        private static void SetPaletteColors( Syncfusion.Maui.Inputs.SfColorPicker picker, IReadOnlyList<string> colors)
        {
            var paletteColors = new System.Collections.ObjectModel.ObservableCollection<Color>();

            foreach (var color in colors.Take(5))
            {
                if (Color.TryParse(color, out var parsedColor))
                {
                    paletteColors.Add(parsedColor);
                }
            }

            picker.PaletteColors = paletteColors;

            if (paletteColors.Count > 0)
            {
                picker.SelectedColor = paletteColors[0];
            }
        }
    }
}
