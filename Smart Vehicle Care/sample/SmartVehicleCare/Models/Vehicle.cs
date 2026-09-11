using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SmartVehicleCare.Models;

public class Vehicle : INotifyPropertyChanged
{
    public int Id { get; set; }

    // Vehicle Type (0=Car, 1=Motorcycle, 2=Commercial, 3=Electric)
    public int VehicleType { get; set; }

    private string _make = string.Empty;
    public string Make { get => _make; set { _make = value; OnPropertyChanged(); OnPropertyChanged(nameof(MakeModel)); OnPropertyChanged(nameof(DisplayName)); } }

    private string _model = string.Empty;
    public string Model { get => _model; set { _model = value; OnPropertyChanged(); OnPropertyChanged(nameof(MakeModel)); OnPropertyChanged(nameof(DisplayName)); } }

    private string _year = string.Empty;
    public string Year { get => _year; set { _year = value; OnPropertyChanged(); } }

    private string _variant = string.Empty;
    public string Variant { get => _variant; set { _variant = value; OnPropertyChanged(); } }

    private string _color = string.Empty;
    public string Color { get => _color; set { _color = value; OnPropertyChanged(); } }

    private string _odaMeterReading = string.Empty;
    public string OdometerReading { get => _odaMeterReading; set { _odaMeterReading = value; OnPropertyChanged(); } }

    private string _registrationNumber = string.Empty;
    public string RegistrationNumber { get => _registrationNumber; set { _registrationNumber = value; OnPropertyChanged(); } }

    private string _purchaseYear = string.Empty;
    public string PurchaseYear { get => _purchaseYear; set { _purchaseYear = value; OnPropertyChanged(); } }

    private string _serviceInterval = string.Empty;
    public string ServiceInterval { get => _serviceInterval; set { _serviceInterval = value; OnPropertyChanged(); } }

    private string _customServiceIntervalKm = string.Empty;
    public string CustomServiceIntervalKm { get => _customServiceIntervalKm; set { _customServiceIntervalKm = value; OnPropertyChanged(); } }

    // Computed Properties
    public string MakeModel   => $"{Make} {Model}".Trim();
    public string DisplayName => $"{Make} {Model} ";

    // Legacy properties for backward compatibility
    public int LegacyYear
    {
        get => int.TryParse(Year, out var y) ? y : DateTime.Now.Year;
        set => Year = value.ToString();
    }

    // Other properties
    public DateTime InsuranceRenewalDate { get; set; }
    public DateTime NextServiceDueDate   { get; set; }
    public DateTime CreatedDate          { get; set; } = DateTime.Now;
    public bool     IsActive             { get; set; } = true;

    public override string ToString() => MakeModel;

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string prop = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
}
