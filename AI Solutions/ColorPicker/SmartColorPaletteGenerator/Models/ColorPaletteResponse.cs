namespace SmartColorPaletteGenerator
{
    public class ColorPaletteResponse
    {
        public string ThemeName { get; set; } = string.Empty;
        public int MatchPercentage { get; set; }
        public List<string> Colors { get; set; } = new();
        public string Rationale { get; set; } = string.Empty;
    }
}
