using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SmartColorPaletteGenerator
{
    public class SmartColorPalette : INotifyPropertyChanged
    {
        private string themeName = string.Empty;
        private int matchPercentage;
        private List<string> colors = new();

        public string ThemeName
        {
            get => themeName;
            set => SetProperty(ref themeName, value);
        }

        public int MatchPercentage
        {
            get => matchPercentage;
            set => SetProperty(ref matchPercentage, value);
        }

        public List<string> Colors
        {
            get => colors;
            set => SetProperty(ref colors, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }
}
