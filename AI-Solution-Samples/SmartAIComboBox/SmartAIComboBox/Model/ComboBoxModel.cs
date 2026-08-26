using System.ComponentModel;

namespace SmartAIComboBox.SmartAIComboBox
{
    public class FoodModel : INotifyPropertyChanged
    {
        public string? Name { get; set; }
        public double Price { get; set; }
        public string? Category { get; set; }
        public string? Diet { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value) return;
                _isSelected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
            }
        }

        public override string ToString() => Name ?? string.Empty;
        public event PropertyChangedEventHandler? PropertyChanged;
    }
}