using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SmartVehicleCare.Models;
using SmartVehicleCare.Services;

namespace SmartVehicleCare.ViewModels;

public class WelcomeViewModel : INotifyPropertyChanged
{
    private readonly VehicleDataService _dataService;

    #region Wizard Navigation

    private int _currentStep = 0;

    public int CurrentStep
    {
        get => _currentStep;
        set
        {
            if (SetProperty(ref _currentStep, value))
            {
                OnPropertyChanged(nameof(IsWizardVisible));
                OnPropertyChanged(nameof(IsLandingVisible));

                OnPropertyChanged(nameof(IsStep1Visible));
                OnPropertyChanged(nameof(IsStep2Visible));
                OnPropertyChanged(nameof(IsStep3Visible));

                OnPropertyChanged(nameof(StepText));
                OnPropertyChanged(nameof(IsBackEnabled));
                OnPropertyChanged(nameof(NextButtonText));

                OnPropertyChanged(nameof(Dot1Active));
                OnPropertyChanged(nameof(Dot2Active));
                OnPropertyChanged(nameof(Dot3Active));

                OnPropertyChanged(nameof(CanProceedToNext));
                OnPropertyChanged(nameof(IsStep1CloseVisible));
            }
        }
    }

    public bool IsWizardVisible => CurrentStep > 0;
    public bool IsLandingVisible => CurrentStep == 0;

    public bool IsStep1Visible => CurrentStep == 1;
    public bool IsStep2Visible => CurrentStep == 2;
    public bool IsStep3Visible => CurrentStep == 3;

    public bool IsBackEnabled => CurrentStep > 1 && CurrentStep < 3;

    public string StepText =>
        CurrentStep > 0
            ? $"Step {CurrentStep} of 3"
            : string.Empty;

    public string NextButtonText => CurrentStep switch
    {
        1 => "Vehicle details →",
        2 => "Finish setup →",
        _ => "Continue →"
    };

    public bool Dot1Active => CurrentStep >= 1;
    public bool Dot2Active => CurrentStep >= 2;
    public bool Dot3Active => CurrentStep >= 3;

    #endregion

    #region Step 3 — Documents

  

    #endregion

    #region Step 1 - Vehicle Type

    private int _selectedVehicleTypeIndex = -1;

    public int SelectedVehicleTypeIndex
    {
        get => _selectedVehicleTypeIndex;
        set
        {
            if (SetProperty(ref _selectedVehicleTypeIndex, value))
            {
                OnPropertyChanged(nameof(IsCarSelected));
                OnPropertyChanged(nameof(IsMotoSelected));
                OnPropertyChanged(nameof(MakeHint));
                OnPropertyChanged(nameof(ModelHint));
                OnPropertyChanged(nameof(VariantHint));

                OnPropertyChanged(nameof(IsStep1Valid));
                OnPropertyChanged(nameof(CanProceedToNext));
            }
        }
    }

    public bool IsCarSelected => SelectedVehicleTypeIndex == 0;
    public bool IsMotoSelected => SelectedVehicleTypeIndex == 1;
    public bool IsTruckSelected => SelectedVehicleTypeIndex == 2;
    public bool IsEvSelected => SelectedVehicleTypeIndex == 3;

    public string MakeHint => IsMotoSelected ? "e.g. Honda, Yamaha, Royal Enfield" : "e.g. Toyota, Honda, BMW";
    public string ModelHint => IsMotoSelected ? "e.g. CBR600, FZ25, Thunderbird" : "e.g. Camry, Civic, Swift";
    public string VariantHint => IsMotoSelected ? "e.g. Standard, Sport, Cruiser" : "e.g. ZXI, VXI";

    #endregion

    #region Step 2 - Vehicle Details

    private string _selectedMake = string.Empty;

    public string SelectedMake
    {
        get => _selectedMake;
        set
        {
            if (SetProperty(ref _selectedMake, value))
            {
                NotifyStep2Validation();
            }
        }
    }

    private string _vehicleModel = string.Empty;

    public string VehicleModel
    {
        get => _vehicleModel;
        set
        {
            if (SetProperty(ref _vehicleModel, value))
            {
                NotifyStep2Validation();
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
                NotifyStep2Validation();
            }
        }
    }

    #endregion

    #region Validation

    public bool IsStep1Valid =>
        SelectedVehicleTypeIndex >= 0;

    public bool IsStep1CloseVisible => CurrentStep == 1;

    public bool IsOdaMeterReadingValid =>
        int.TryParse(OdaMeterReading, out int odaMeterReading) &&
        odaMeterReading >= 0;

    public bool IsStep2Valid =>
        !string.IsNullOrWhiteSpace(SelectedMake) &&
        !string.IsNullOrWhiteSpace(VehicleModel) &&
        !string.IsNullOrWhiteSpace(OdaMeterReading) &&
        IsOdaMeterReadingValid;

    public bool CanProceedToNext =>
        CurrentStep switch
        {
            1 => IsStep1Valid,
            2 => IsStep2Valid,
            3 => true,
            _ => true
        };

    private void NotifyStep2Validation()
    {
        OnPropertyChanged(nameof(IsStep2Valid));
        OnPropertyChanged(nameof(IsOdaMeterReadingValid));
        OnPropertyChanged(nameof(CanProceedToNext));
    }

    #endregion

    #region Commands

    public ICommand GoToWizardCommand { get; }

    public ICommand NextStepCommand { get; }

    public ICommand BackStepCommand { get; }

    public ICommand CloseWizardCommand { get; }

    public ICommand SelectVehicleTypeCommand { get; }

    public ICommand ExploreWithSampleDataCommand { get; }

    private bool _isLoadingSampleData;
    public bool IsLoadingSampleData
    {
        get => _isLoadingSampleData;
        private set => SetProperty(ref _isLoadingSampleData, value);
    }

    public event Action? SetupCompleted;
    public event Action? ExploreWithSampleDataRequested;

    #endregion

    public void ResetWelcomeFlow()
    {
        CurrentStep = 0;

        SelectedVehicleTypeIndex = -1;
        SelectedMake = string.Empty;
        VehicleModel = string.Empty;
        Variant = string.Empty;
        OdaMeterReading = string.Empty;

    }

    public WelcomeViewModel(VehicleDataService dataService)
    {
        _dataService = dataService;

        GoToWizardCommand = new Command(() =>
        {
            CurrentStep = 1;
        });

        NextStepCommand = new Command(() =>
        {
            if (CurrentStep < 3)
            {
                CurrentStep++;

                if (CurrentStep == 3)
                {
                    AddVehicleFromWizard();
                    SetupCompleted?.Invoke();
                }
            }
        });

        ExploreWithSampleDataCommand = new Command(async () =>
        {
            if (IsLoadingSampleData)
                return;

            IsLoadingSampleData = true;
            try
            {
                await Task.Delay(150);
                LoadSampleData();
                ExploreWithSampleDataRequested?.Invoke();
            }
            finally
            {
                IsLoadingSampleData = false;
            }
        });

        BackStepCommand = new Command(() =>
        {
            if (CurrentStep > 1)
            {
                CurrentStep--;
            }
        });

        CloseWizardCommand = new Command(() =>
        {
            CurrentStep = 0;
        });

        SelectVehicleTypeCommand = new Command<string>(index =>
        {
            if (int.TryParse(index, out int selectedIndex))
            {
                SelectedVehicleTypeIndex = selectedIndex;
            }
        });

    }

    /// <summary>Creates the vehicle entered in the wizard and makes it the active vehicle.</summary>
    private void AddVehicleFromWizard()
    {
        var vehicle = new Vehicle
        {
            VehicleType     = SelectedVehicleTypeIndex,
            Make            = SelectedMake,
            Model           = VehicleModel,
            Variant         = Variant,
            OdometerReading = OdaMeterReading,
            CreatedDate     = DateTime.Now,
            IsActive        = true
        };

        _dataService.AddVehicle(vehicle);
        _dataService.SelectedVehicle = vehicle;
    }

    /// <summary>Seeds a fully populated demo session (two vehicles with service/fuel/reminder history).</summary>
    private void LoadSampleData()
    {
        _dataService.TryLoadDemoData();

        var car = new Vehicle
        {
            Id = 1,
            VehicleType = 0,
            Make = "Hyundai",
            Model = "i20",
            Variant = "Sportz",
            OdometerReading = "45230",
            ServiceInterval = "Every 10,000 km",
            CreatedDate = DateTime.Now,
            IsActive = true
        };

        var bike = new Vehicle
        {
            Id = 2,
            VehicleType = 1,
            Make = "Honda",
            Model = "Activa",
            Variant = "125",
            OdometerReading = "18950",
            ServiceInterval = "Every 5,000 km",
            CreatedDate = DateTime.Now,
            IsActive = true
        };

        _dataService.AddVehicle(car);
        _dataService.AddVehicle(bike);
        _dataService.SelectedVehicle = car;

        _dataService.AddServiceRecord(car.Id, new ServiceRecord
        {
            ServiceType = "Oil & Filter Change",
            ServiceDate = DateTime.Today.AddDays(-24),
            Workshop = "Hyundai Authorized Service",
            Amount = "₹3200",
            Mileage = "45200 km",
            Status = "Completed",
            Notes = "Routine service with filter change."
        });

        _dataService.AddServiceRecord(car.Id, new ServiceRecord
        {
            ServiceType = "Brake Inspection",
            ServiceDate = DateTime.Today.AddDays(-61),
            Workshop = "City Auto Works",
            Amount = "₹1850",
            Mileage = "44600 km",
            Status = "Completed",
            Notes = "Brake pads checked; wear within limit."
        });

        _dataService.AddServiceRecord(car.Id, new ServiceRecord
        {
            ServiceType = "Tyre Rotation",
            ServiceDate = DateTime.Today.AddDays(-97),
            Workshop = "WheelCare Center",
            Amount = "₹1600",
            Mileage = "44120 km",
            Status = "Completed",
            Notes = "Rotated tyres and checked alignment."
        });

        _dataService.AddServiceRecord(car.Id, new ServiceRecord
        {
            ServiceType = "Battery Health Check",
            ServiceDate = DateTime.Today.AddDays(-150),
            Workshop = "AutoCare Plus",
            Amount = "₹1300",
            Mileage = "43680 km",
            Status = "Completed",
            Notes = "Battery voltage checked and terminals cleaned."
        });

        _dataService.AddServiceRecord(car.Id, new ServiceRecord
        {
            ServiceType = "Cabin Filter Replacement",
            ServiceDate = DateTime.Today.AddDays(-260),
            Workshop = "City Auto Works",
            Amount = "₹950",
            Mileage = "43210 km",
            Status = "Completed",
            Notes = "Air filter replaced for cabin airflow improvement."
        });

        _dataService.AddFuelEntry(car.Id, new FuelEntry
        {
            FuelDate = DateTime.Today.AddDays(-7),
            FuelType = "Petrol",
            Station = "Shell Rajaji Nagar",
            LitresFilled = 30,
            CostPerLitre = 102.40,
            OdometerReading = 45230,
            IsFullTank = true
        });

        _dataService.AddFuelEntry(car.Id, new FuelEntry
        {
            FuelDate = DateTime.Today.AddDays(-19),
            FuelType = "Petrol",
            Station = "HP Fuel Point",
            LitresFilled = 28,
            CostPerLitre = 101.80,
            OdometerReading = 44910,
            IsFullTank = true
        });

        _dataService.AddFuelEntry(car.Id, new FuelEntry
        {
            FuelDate = DateTime.Today.AddDays(-41),
            FuelType = "Petrol",
            Station = "Reliance Fuel",
            LitresFilled = 32,
            CostPerLitre = 100.90,
            OdometerReading = 44540,
            IsFullTank = true
        });

        _dataService.AddFuelEntry(car.Id, new FuelEntry
        {
            FuelDate = DateTime.Today.AddDays(-68),
            FuelType = "Petrol",
            Station = "Indian Oil - Purasawalkam",
            LitresFilled = 29,
            CostPerLitre = 103.60,
            OdometerReading = 44210,
            IsFullTank = true
        });

        _dataService.AddFuelEntry(car.Id, new FuelEntry
        {
            FuelDate = DateTime.Today.AddDays(-112),
            FuelType = "Petrol",
            Station = "Bharat Petroleum - Koyambedu",
            LitresFilled = 31,
            CostPerLitre = 102.90,
            OdometerReading = 43875,
            IsFullTank = true
        });

        _dataService.AddReminder(car.Id, new ScheduleReminder
        {
            Title = "Oil Change",
            ReminderType = "Service",
            DueDate = DateTime.Today.AddDays(15),
            Priority = "High"
        });

        _dataService.AddReminder(car.Id, new ScheduleReminder
        {
            Title = "Tyre Pressure Check",
            ReminderType = "Inspection",
            DueDate = DateTime.Today.AddDays(20),
            Priority = "High"
        });

        _dataService.AddReminder(car.Id, new ScheduleReminder
        {
            Title = "Brake Check",
            ReminderType = "Inspection",
            DueDate = DateTime.Today.AddDays(32),
            Priority = "Medium"
        });

        _dataService.AddReminder(car.Id, new ScheduleReminder
        {
            Title = "Wheel Alignment Check",
            ReminderType = "Inspection",
            DueDate = DateTime.Today.AddDays(18),
            Priority = "High"
        });

        _dataService.AddReminder(car.Id, new ScheduleReminder
        {
            Title = "General Service Due",
            ReminderType = "Service",
            DueDate = DateTime.Today.AddDays(45),
            Priority = "Medium"
        });

        _dataService.AddServiceRecord(bike.Id, new ServiceRecord
        {
            ServiceType = "General Service",
            ServiceDate = DateTime.Today.AddDays(-95),
            Workshop = "Two Wheeler Care",
            Amount = "₹1800",
            Mileage = "18650 km",
            Status = "Completed",
            Notes = "Chain, spark plug, and engine check completed."
        });

        _dataService.AddFuelEntry(bike.Id, new FuelEntry
        {
            FuelDate = DateTime.Today.AddDays(-5),
            FuelType = "Petrol",
            Station = "Bharath Fuel Hub",
            LitresFilled = 6.2,
            CostPerLitre = 106.20,
            OdometerReading = 18950,
            IsFullTank = true
        });

        _dataService.AddReminder(bike.Id, new ScheduleReminder
        {
            Title = "Service Reminder",
            ReminderType = "Service",
            DueDate = DateTime.Today.AddDays(24),
            Priority = "Medium"
        });

        // Publish one final state after the complete sample dataset is loaded.
        _dataService.SelectedVehicle = car;
        _dataService.NotifyDataReloaded();
    }

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;

    protected bool SetProperty<T>(
        ref T backingStore,
        T value,
        [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
        {
            return false;
        }

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