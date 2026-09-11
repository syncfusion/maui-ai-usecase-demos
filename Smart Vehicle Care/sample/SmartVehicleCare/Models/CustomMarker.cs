using Syncfusion.Maui.Maps;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SmartVehicleCare.Models;

public class CustomMarker : MapMarker, INotifyPropertyChanged
{
    public string? Name              { get; set; }
    public string? Details           { get; set; }
    public string? Address           { get; set; }
    public string? Distance          { get; set; }
    public string? ImageName         { get; set; }
    public Uri?    Image             { get; set; }
    public bool    IsCurrentLocation { get; set; }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;
            _isSelected = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(MarkerSize));
        }
    }

    public double MarkerSize => IsSelected ? 34 : 22;

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
