using System.ComponentModel;

namespace SmartAITags.Model
{
    /// <summary>
    /// Represents a single AI-generated tag shown as a chip.
    /// Mirrors the simple reference Person class (Name property only).
    /// </summary>
    public class TagModel : INotifyPropertyChanged
    {
        private string? _name;

        /// <summary>Tag display text (e.g. "Login", "Authentication", "Biometric").</summary>
        public string? Name
        {
            get => _name;
            set
            {
                if (_name == value) return;
                _name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }

        public override string ToString() => Name ?? string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}