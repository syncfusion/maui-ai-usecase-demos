using System.ComponentModel;

namespace AITaskPrioritization
{
    public class CardDetails : INotifyPropertyChanged
    {
        private string? _name;
        private string? _title;
        private string? _description;
        private string? _category;
        private int _index;
        private double _progress;
        private string? _image;
        private DateTime _dueDate;
        private Color _urgencyStrokeColor = Colors.Transparent;
        private double _strokeThickness = 0;
        private double glowOpacity = 1;

        public string? Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public string? Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(nameof(Title)); }
        }

        public string? Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(nameof(Description)); }
        }

        public string? Category
        {
            get => _category;
            set { _category = value; OnPropertyChanged(nameof(Category)); }
        }

        public int Index
        {
            get => _index;
            set { _index = value; OnPropertyChanged(nameof(Index)); }
        }

        public double Progress
        {
            get => _progress;
            set { _progress = value; OnPropertyChanged(nameof(Progress)); }
        }

        public string? Image
        {
            get => _image;
            set { _image = value; OnPropertyChanged(nameof(Image)); }
        }

        public DateTime DueDate
        {
            get => _dueDate;
            set { _dueDate = value; OnPropertyChanged(nameof(DueDate)); }
        }

        public Color UrgencyStrokeColor
        {
            get => _urgencyStrokeColor;
            set { _urgencyStrokeColor = value; OnPropertyChanged(nameof(UrgencyStrokeColor)); }
        }

        public double StrokeThickness
        {
            get => _strokeThickness;
            set { _strokeThickness = value; OnPropertyChanged(nameof(StrokeThickness)); }
        }

        public double GlowOpacity
        {
            get => glowOpacity;
            set
            {
                glowOpacity = value;
                OnPropertyChanged(nameof(GlowOpacity));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}