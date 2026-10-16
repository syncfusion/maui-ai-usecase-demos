using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SmartVehicleCare.Models;

namespace SmartVehicleCare.ViewModels;

public class AddServiceViewModel : INotifyPropertyChanged
{
    #region Vehicle Label
    private string _vehicleLabel = string.Empty;
    public string VehicleLabel
    {
        get => _vehicleLabel;
        set => SetProperty(ref _vehicleLabel, value);
    }
    #endregion

    #region Service Type
    private string _selectedType = string.Empty;
    public string SelectedType
    {
        get => _selectedType;
        set
        {
            if (SetProperty(ref _selectedType, value))
            {
                OnPropertyChanged(nameof(IsOilChangeSelected));
                OnPropertyChanged(nameof(IsTyreRotationSelected));
                OnPropertyChanged(nameof(IsBrakeServiceSelected));
                OnPropertyChanged(nameof(IsElectricalSelected));
                OnPropertyChanged(nameof(IsGeneralInspectionSelected));
                OnPropertyChanged(nameof(IsCustomSelected));
                if (value != "Custom")
                    ServiceTypeInput = value;
                else
                    ServiceTypeInput = string.Empty;
                NotifyValidation();
            }
        }
    }

    public bool IsOilChangeSelected       => SelectedType == "Oil & Filter Change";
    public bool IsTyreRotationSelected    => SelectedType == "Tyre & Wheel Service";
    public bool IsBrakeServiceSelected    => SelectedType == "Brake Inspection & Repair";
    public bool IsElectricalSelected      => SelectedType == "Battery & Electrical";
    public bool IsGeneralInspectionSelected => SelectedType == "General Service";
    public bool IsCustomSelected          => SelectedType == "Custom";

    private string _serviceTypeInput = string.Empty;
    public string ServiceTypeInput
    {
        get => _serviceTypeInput;
        set
        {
            if (SetProperty(ref _serviceTypeInput, value))
                NotifyValidation();
        }
    }

    public ICommand SelectTypeCommand { get; }
    #endregion

    #region Service Date
    private bool _isServiceDateSet;
    private DateTime _serviceDate = DateTime.Today;
    public DateTime ServiceDate
    {
        get => _serviceDate;
        set
        {
            if (SetProperty(ref _serviceDate, value))
            {
                _isServiceDateSet = true;
                OnPropertyChanged(nameof(ServiceDateDisplay));
                NotifyValidation();
            }
        }
    }
    public string ServiceDateDisplay => _isServiceDateSet ? _serviceDate.ToString("dd MMM yyyy") : string.Empty;
    #endregion

    #region Mileage
    private string _mileage = string.Empty;
    public string Mileage
    {
        get => _mileage;
        set => SetProperty(ref _mileage, value);
    }
    #endregion

    #region Total Cost
    private string _totalCost = string.Empty;
    public string TotalCost
    {
        get => _totalCost;
        set => SetProperty(ref _totalCost, value);
    }
    #endregion

    #region Service Provider
    private string _serviceProvider = string.Empty;
    public string ServiceProvider
    {
        get => _serviceProvider;
        set => SetProperty(ref _serviceProvider, value);
    }
    #endregion

    #region Status
    private string _status = "Completed";
    public string Status
    {
        get => _status;
        set
        {
            if (SetProperty(ref _status, value))
            {
                OnPropertyChanged(nameof(IsCompletedSelected));
                OnPropertyChanged(nameof(IsPendingSelected));
                OnPropertyChanged(nameof(IsScheduledSelected));
            }
        }
    }
    public bool IsCompletedSelected => Status == "Completed";
    public bool IsPendingSelected   => Status == "Pending";
    public bool IsScheduledSelected => Status == "Scheduled";
    public ICommand SelectStatusCommand { get; }
    #endregion

    #region Notes
    private string _notes = string.Empty;
    public string Notes
    {
        get => _notes;
        set => SetProperty(ref _notes, value);
    }
    #endregion

    #region CanSave
    public bool CanSave => !string.IsNullOrWhiteSpace(ServiceTypeInput) && _isServiceDateSet;

    private void NotifyValidation()
    {
        OnPropertyChanged(nameof(CanSave));
        ((Command)SaveCommand).ChangeCanExecute();
    }
    #endregion

    #region Commands + Events
    public ICommand SaveCommand   { get; }
    public ICommand CancelCommand { get; }
    public ICommand CloseCommand  { get; }

    public Action<ServiceRecord>? OnServiceSaved;
    public Action? OnCloseRequested;

    private void ExecuteSave()
    {
        if (!CanSave) return;

        Color statusColor, statusBgColor;
        switch (Status)
        {
            case "Pending":
                statusColor   = Color.FromArgb("#D97706");
                statusBgColor = Color.FromArgb("#FEF3C7");
                break;
            case "Scheduled":
                statusColor   = Color.FromArgb("#3B5BDB");
                statusBgColor = Color.FromArgb("#EEF2FF");
                break;
            default:
                statusColor   = Color.FromArgb("#16A34A");
                statusBgColor = Color.FromArgb("#DCFCE7");
                break;
        }

        var costStr = string.IsNullOrWhiteSpace(TotalCost) ? "—"
            : (TotalCost.StartsWith("₹") ? TotalCost : $"₹{TotalCost}");
        var mileageStr = string.IsNullOrWhiteSpace(Mileage) ? "—"
            : (Mileage.Contains("km") ? Mileage : $"{Mileage} km");

        var record = new ServiceRecord
        {
            ServiceDate     = ServiceDate,
            ServiceType     = ServiceTypeInput.Trim(),
            Workshop        = ServiceProvider,
            Amount          = costStr,
            Mileage         = mileageStr,
            Status          = Status,
            StatusColor     = statusColor,
            StatusBgColor   = statusBgColor,
            Notes           = Notes,
        };
        OnServiceSaved?.Invoke(record);
    }
    #endregion

    public AddServiceViewModel()
    {
        SelectTypeCommand   = new Command<string>(t => SelectedType = t);
        SelectStatusCommand = new Command<string>(s => Status = s);
        SaveCommand   = new Command(ExecuteSave, () => CanSave);
        CancelCommand = new Command(() => OnCloseRequested?.Invoke());
        CloseCommand  = new Command(() => OnCloseRequested?.Invoke());
    }

    public void Reset()
    {
        _selectedType      = string.Empty;
        _serviceTypeInput  = string.Empty;
        _isServiceDateSet  = false;
        _serviceDate       = DateTime.Today;
        _mileage           = string.Empty;
        _totalCost         = string.Empty;
        _serviceProvider   = string.Empty;
        _status            = "Completed";
        _notes             = string.Empty;

        OnPropertyChanged(nameof(SelectedType));
        OnPropertyChanged(nameof(ServiceTypeInput));
        OnPropertyChanged(nameof(IsOilChangeSelected));
        OnPropertyChanged(nameof(IsTyreRotationSelected));
        OnPropertyChanged(nameof(IsBrakeServiceSelected));
        OnPropertyChanged(nameof(IsElectricalSelected));
        OnPropertyChanged(nameof(IsGeneralInspectionSelected));
        OnPropertyChanged(nameof(IsCustomSelected));
        OnPropertyChanged(nameof(ServiceDate));
        OnPropertyChanged(nameof(ServiceDateDisplay));
        OnPropertyChanged(nameof(Mileage));
        OnPropertyChanged(nameof(TotalCost));
        OnPropertyChanged(nameof(ServiceProvider));
        OnPropertyChanged(nameof(Status));
        OnPropertyChanged(nameof(IsCompletedSelected));
        OnPropertyChanged(nameof(IsPendingSelected));
        OnPropertyChanged(nameof(IsScheduledSelected));
        OnPropertyChanged(nameof(Notes));
        NotifyValidation();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected bool SetProperty<T>(ref T store, T value, [CallerMemberName] string prop = "")
    {
        if (EqualityComparer<T>.Default.Equals(store, value)) return false;
        store = value;
        OnPropertyChanged(prop);
        return true;
    }
    protected void OnPropertyChanged([CallerMemberName] string prop = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
}
