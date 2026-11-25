#nullable enable
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SmartColorPaletteGenerator
{
    /// <summary>
    /// Represents the view model for the main window of the SmartColorPaletteGenerator application.
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ISmartColorPaletteService paletteService;
        private string styleInput = string.Empty;
        private bool isBusy;
        private string generationStatus = "Three AI-powered palette recommendations are ready.";
        private readonly List<SmartColorPalette> palettes = new();

        public MainViewModel(ISmartColorPaletteService paletteService)
        {
            this.paletteService = paletteService;
            InitializeDefaultPalettes();
        }

        public string StyleInput
        {
            get => styleInput;
            set => SetProperty(ref styleInput, value);
        }

        public bool IsBusy
        {
            get => isBusy;
            private set => SetProperty(ref isBusy, value);
        }

        public string GenerationStatus
        {
            get => generationStatus;
            private set => SetProperty(ref generationStatus, value);
        }

        public IReadOnlyList<SmartColorPalette> Palettes => palettes;

        public SmartColorPalette FirstPalette => GetPaletteOrDefault(0);
        public SmartColorPalette SecondPalette => GetPaletteOrDefault(1);
        public SmartColorPalette ThirdPalette => GetPaletteOrDefault(2);

        public event PropertyChangedEventHandler? PropertyChanged;

        public async Task GeneratePaletteAsync()
        {
            var prompt = string.IsNullOrWhiteSpace(StyleInput) ? "Application Name" : StyleInput.Trim();
            await LoadPalettesAsync(prompt);
        }

        private async Task LoadPalettesAsync(string prompt)
        {
            try
            {
                IsBusy = true;
                GenerationStatus = "Generating three palette recommendations...";

                var response = await paletteService.GeneratePaletteVariationsAsync(prompt);
                ApplyPalettes(response, prompt);
                GenerationStatus = $"Three palette recommendations generated for {prompt}.";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void InitializeDefaultPalettes()
        {
            ApplyPalettes(new List<ColorPaletteResponse>
            {
                new()
                {
                    ThemeName = "Professional Blue",
                    MatchPercentage = 96,
                    Colors = new List<string> { "#2563EB", "#60A5FA", "#93C5FD", "#EFF6FF", "#1E3A8A" }
                },
                new()
                {
                    ThemeName = "Modern Teal",
                    MatchPercentage = 91,
                    Colors = new List<string> { "#0F766E", "#14B8A6", "#5EEAD4", "#CCFBF1", "#134E4A" }
                },
                new()
                {
                    ThemeName = "Premium Dark",
                    MatchPercentage = 88,
                    Colors = new List<string> { "#111827", "#374151", "#6366F1", "#C7D2FE", "#F9FAFB" }
                }
            });
        }

        private void ApplyPalettes(IReadOnlyList<ColorPaletteResponse> response, string? styleDescription = null)
        {
            while (palettes.Count < 3)
            {
                palettes.Add(new SmartColorPalette());
            }

            var incoming = response.Take(3).ToList();

            for (var index = 0; index < 3; index++)
            {
                var palette = palettes[index];

                if (index < incoming.Count)
                {
                    var item = incoming[index];
                    palette.ThemeName = NormalizeThemeName(item.ThemeName, styleDescription);
                    palette.MatchPercentage = item.MatchPercentage;
                    palette.Colors = NormalizeColors(item.Colors, index);
                }
                else
                {
                    palette.ThemeName = $"Palette {index + 1}";
                    palette.MatchPercentage = 0;
                    palette.Colors = GetFallbackColors(index);
                }
            }

            OnPropertyChanged(nameof(Palettes));
            OnPropertyChanged(nameof(FirstPalette));
            OnPropertyChanged(nameof(SecondPalette));
            OnPropertyChanged(nameof(ThirdPalette));
        }

        private SmartColorPalette GetPaletteOrDefault(int index)
        {
            return index < palettes.Count
                ? palettes[index]
                : new SmartColorPalette
                {
                    ThemeName = $"Palette {index + 1}",
                    MatchPercentage = 0,
                    Colors = GetFallbackColors(index)
                };
        }

        private static List<string> NormalizeColors(IEnumerable<string>? colors, int index)
        {
            var normalized = colors?.Where(IsValidHexColor).Take(5).ToList() ?? new List<string>();

            if (normalized.Count == 0)
                return GetFallbackColors(index);

            while (normalized.Count < 5)
            {
                normalized.Add(GetFallbackColors(index)[normalized.Count]);
            }

            return normalized;
        }

        private static string NormalizeThemeName(string themeName, string? styleDescription)
        {
            var name = string.IsNullOrWhiteSpace(themeName) ? "Palette" : themeName.Trim();

            if (string.IsNullOrWhiteSpace(styleDescription))
                return name;

            var prompt = styleDescription.Trim();

            if (name.StartsWith(prompt, StringComparison.OrdinalIgnoreCase))
            {
                name = name[prompt.Length..].TrimStart('-', ' ', ':');
            }

            return string.IsNullOrWhiteSpace(name) ? "Palette" : name;
        }

        private static bool IsValidHexColor(string? value)
        {
            return !string.IsNullOrWhiteSpace(value) && Color.TryParse(value, out _);
        }

        private static List<string> GetFallbackColors(int index)
        {
            return index switch
            {
                0 => new List<string> { "#2563EB", "#60A5FA", "#93C5FD", "#EFF6FF", "#1E3A8A" },
                1 => new List<string> { "#0F766E", "#14B8A6", "#5EEAD4", "#CCFBF1", "#134E4A" },
                _ => new List<string> { "#111827", "#374151", "#6366F1", "#C7D2FE", "#F9FAFB" }
            };
        }

        private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
