using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace SmartVehicleCare.ViewModels;

public class AddVehicleViewModel : INotifyPropertyChanged
{
    #region Dropdown Sources

    public List<string> VehicleTypes { get; } =
    [
        "Car",
        "Motorcycle",
    ];

    #endregion

    #region Vehicle Type

    private string _selectedVehicleType = string.Empty;

    public string SelectedVehicleType
    {
        get => _selectedVehicleType;
        set
        {
            if (SetProperty(ref _selectedVehicleType, value))
            {
                NotifyValidation();
            }
        }
    }

    #endregion

    #region Vehicle Details

    private string _make = string.Empty;

    public string Make
    {
        get => _make;
        set
        {
            if (SetProperty(ref _make, value))
            {
                NotifyValidation();
            }
        }
    }

    private string _model = string.Empty;

    public string Model
    {
        get => _model;
        set
        {
            if (SetProperty(ref _model, value))
            {
                NotifyValidation();
            }
        }
    }

    private string _variant = string.Empty;

    public string Variant
    {
        get => _variant;
        set => SetProperty(ref _variant, value);
    }

    private string _odaMeterReading = string.Empty;

    public string OdaMeterReading
    {
        get => _odaMeterReading;
        set
        {
            if (SetProperty(ref _odaMeterReading, value))
            {
                NotifyValidation();
            }
        }
    }

    #endregion

    #region Validation

    public bool IsOdometerValid =>
        int.TryParse(OdaMeterReading, out int km) &&
        km >= 0;

    public bool CanAddVehicle =>
        !string.IsNullOrWhiteSpace(SelectedVehicleType) &&
        !string.IsNullOrWhiteSpace(Make) &&
        !string.IsNullOrWhiteSpace(Model) &&
        !string.IsNullOrWhiteSpace(OdaMeterReading) &&
        IsOdometerValid;

    private void NotifyValidation()
    {
        OnPropertyChanged(nameof(IsOdometerValid));
        OnPropertyChanged(nameof(CanAddVehicle));
    }

    #endregion

    #region Commands

    public ICommand AddVehicleCommand { get; }

    public ICommand CloseCommand { get; }

    #endregion

    #region Events

    public event Action? OnAddVehicleRequested;

    public event Action? OnCloseRequested;

    #endregion

    public AddVehicleViewModel()
    {
        AddVehicleCommand = new Command(() =>
        {
            if (!CanAddVehicle)
                return;

            OnAddVehicleRequested?.Invoke();
        });

        CloseCommand = new Command(() =>
        {
            OnCloseRequested?.Invoke();
        });
    }

    #region Helpers

    public void Reset()
    {
        SelectedVehicleType = string.Empty;

        Make = string.Empty;
        Model = string.Empty;

        Variant = string.Empty;

        OdaMeterReading = string.Empty;

    }

    #endregion

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;

    protected bool SetProperty<T>(
        ref T backingStore,
        T value,
        [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
            return false;

        backingStore = value;

        OnPropertyChanged(propertyName);

        return true;
    }

    protected void OnPropertyChanged(
        [CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }

    #endregion
}