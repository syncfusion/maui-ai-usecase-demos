using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SmartVehicleCare.Models;

namespace SmartVehicleCare.ViewModels;

public class AddFuelViewModel : INotifyPropertyChanged
{
    #region Vehicle Label
    private string _vehicleLabel = string.Empty;
    public string VehicleLabel
    {
        get => _vehicleLabel;
        set => SetProperty(ref _vehicleLabel, value);
    }
    #endregion

    #region Fuel Type
    private string _selectedFuelType = string.Empty;
    public string SelectedFuelType
    {
        get => _selectedFuelType;
        set
        {
            if (SetProperty(ref _selectedFuelType, value))
            {
                OnPropertyChanged(nameof(IsPetrolSelected));
                OnPropertyChanged(nameof(IsDieselSelected));
                OnPropertyChanged(nameof(IsCNGSelected));
                OnPropertyChanged(nameof(IsElectricSelected));
                NotifyValidation();
            }
        }
    }
    public bool IsPetrolSelected  => SelectedFuelType == "Petrol";
    public bool IsDieselSelected  => SelectedFuelType == "Diesel";
    public bool IsCNGSelected     => SelectedFuelType == "CNG";
    public bool IsElectricSelected => SelectedFuelType == "Electric";
    public ICommand SelectFuelTypeCommand { get; }
    #endregion

    #region Date
    private bool _isFuelDateSet;
    private DateTime _fuelDate = DateTime.Today;
    public DateTime FuelDate
    {
        get => _fuelDate;
        set
        {
            if (SetProperty(ref _fuelDate, value))
            {
                _isFuelDateSet = true;
                OnPropertyChanged(nameof(FuelDateDisplay));
            }
        }
    }
    public string FuelDateDisplay => _isFuelDateSet ? _fuelDate.ToString("dd MMM yyyy") : string.Empty;
    #endregion

    #region Station
    private string _station = string.Empty;
    public string Station
    {
        get => _station;
        set => SetProperty(ref _station, value);
    }
    #endregion

    #region Litres Filled
    private string _litresFilled = string.Empty;
    public string LitresFilled
    {
        get => _litresFilled;
        set
        {
            if (SetProperty(ref _litresFilled, value))
            {
                OnPropertyChanged(nameof(TotalCostDisplay));
                OnPropertyChanged(nameof(TotalCostBreakdown));
                NotifyValidation();
            }
        }
    }
    #endregion

    #region Cost Per Litre
    private string _costPerLitre = string.Empty;
    public string CostPerLitre
    {
        get => _costPerLitre;
        set
        {
            if (SetProperty(ref _costPerLitre, value))
            {
                OnPropertyChanged(nameof(TotalCostDisplay));
                OnPropertyChanged(nameof(TotalCostBreakdown));
            }
        }
    }
    #endregion

    #region Computed Total Cost
    private double ParsedLitres => double.TryParse(LitresFilled, out var l) ? l : 0;
    private double ParsedCostPerLitre => double.TryParse(CostPerLitre, out var c) ? c : 0;
    public double ComputedTotal => Math.Round(ParsedLitres * ParsedCostPerLitre, 2);
    public string TotalCostDisplay => ComputedTotal > 0 ? $"₹{ComputedTotal:N2}" : "₹0.00";
    public string TotalCostBreakdown => (ParsedLitres > 0 && ParsedCostPerLitre > 0)
        ? $"{ParsedLitres:N1} L × ₹{ParsedCostPerLitre:N2}"
        : string.Empty;
    #endregion

    #region Odometer
    private string _odometerReading = string.Empty;
    public string OdometerReading
    {
        get => _odometerReading;
        set
        {
            if (SetProperty(ref _odometerReading, value))
                NotifyValidation();
        }
    }
    #endregion

    #region Full Tank
    private bool _isFullTank = true;
    public bool IsFullTank
    {
        get => _isFullTank;
        set => SetProperty(ref _isFullTank, value);
    }
    #endregion

    #region CanSave
    public bool CanSave =>
        !string.IsNullOrWhiteSpace(SelectedFuelType) &&
        !string.IsNullOrWhiteSpace(LitresFilled) &&
        double.TryParse(LitresFilled, out var l) && l > 0 &&
        !string.IsNullOrWhiteSpace(OdometerReading) &&
        int.TryParse(OdometerReading, out _);

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

    public Action<FuelEntry>? OnFuelEntrySaved;
    public Action? OnCloseRequested;

    private void ExecuteSave()
    {
        if (!CanSave) return;

        Color fuelTypeColor, fuelTypeBgColor;
        switch (SelectedFuelType)
        {
            case "Diesel":
                fuelTypeColor   = Color.FromArgb("#065F46");
                fuelTypeBgColor = Color.FromArgb("#D1FAE5");
                break;
            case "CNG":
                fuelTypeColor   = Color.FromArgb("#D97706");
                fuelTypeBgColor = Color.FromArgb("#FEF3C7");
                break;
            case "Electric":
                fuelTypeColor   = Color.FromArgb("#2563EB");
                fuelTypeBgColor = Color.FromArgb("#DBEAFE");
                break;
            default:
                fuelTypeColor   = Color.FromArgb("#DC2626");
                fuelTypeBgColor = Color.FromArgb("#FEE2E2");
                break;
        }

        _ = double.TryParse(LitresFilled, out var litres);
        _ = double.TryParse(CostPerLitre, out var costPerL);
        _ = int.TryParse(OdometerReading, out var odo);

        var entry = new FuelEntry
        {
            FuelDate        = FuelDate,
            FuelType        = SelectedFuelType,
            Station         = Station,
            LitresFilled    = litres,
            CostPerLitre    = costPerL,
            OdometerReading = odo,
            IsFullTank      = IsFullTank,
            FuelTypeColor   = fuelTypeColor,
            FuelTypeBgColor = fuelTypeBgColor,
        };
        OnFuelEntrySaved?.Invoke(entry);
    }
    #endregion

    public AddFuelViewModel()
    {
        SelectFuelTypeCommand = new Command<string>(t => SelectedFuelType = t);
        SaveCommand   = new Command(ExecuteSave, () => CanSave);
        CancelCommand = new Command(() => OnCloseRequested?.Invoke());
        CloseCommand  = new Command(() => OnCloseRequested?.Invoke());
    }

    public void Reset()
    {
        _selectedFuelType  = string.Empty;
        _isFuelDateSet     = false;
        _fuelDate          = DateTime.Today;
        _station           = string.Empty;
        _litresFilled      = string.Empty;
        _costPerLitre      = string.Empty;
        _odometerReading   = string.Empty;
        _isFullTank        = true;

        OnPropertyChanged(nameof(SelectedFuelType));
        OnPropertyChanged(nameof(IsPetrolSelected));
        OnPropertyChanged(nameof(IsDieselSelected));
        OnPropertyChanged(nameof(IsCNGSelected));
        OnPropertyChanged(nameof(IsElectricSelected));
        OnPropertyChanged(nameof(FuelDate));
        OnPropertyChanged(nameof(FuelDateDisplay));
        OnPropertyChanged(nameof(Station));
        OnPropertyChanged(nameof(LitresFilled));
        OnPropertyChanged(nameof(CostPerLitre));
        OnPropertyChanged(nameof(TotalCostDisplay));
        OnPropertyChanged(nameof(TotalCostBreakdown));
        OnPropertyChanged(nameof(OdometerReading));
        OnPropertyChanged(nameof(IsFullTank));
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
