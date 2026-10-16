using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PatientCompanion.Models
{
    public class FooterItem : INotifyPropertyChanged
    {
        private bool _isSelected;

        private static readonly Color ActiveColor =
            Color.FromArgb("#0F766E");

        private static readonly Color InactiveColor =
            Color.FromArgb("#64748B");

        public string Title { get; set; } = string.Empty;

        public string Icon { get; set; } = string.Empty;

        public string Route { get; set; } = string.Empty;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value)
                    return;

                _isSelected = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(TextColor));
                OnPropertyChanged(nameof(IconColor));
            }
        }

        public Color TextColor =>
            IsSelected
                ? ActiveColor
                : InactiveColor;

        public Color IconColor =>
            IsSelected
                ? ActiveColor
                : InactiveColor;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(
            [CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }
    }
}