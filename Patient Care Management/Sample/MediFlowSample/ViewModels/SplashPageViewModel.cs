using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MediFlowSample.ViewModels;

public class SplashPageViewModel : INotifyPropertyChanged
{
    private double _loadingProgress;

    public double LoadingProgress
    {
        get => _loadingProgress;
        set
        {
            if (_loadingProgress != value)
            {
                _loadingProgress = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
