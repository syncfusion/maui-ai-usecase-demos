using System.Collections.ObjectModel;
using System.Text.Json;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Graphics;
using SmartVehicleCare.Models;

namespace SmartVehicleCare.Services;

/// <summary>
/// Owns the demo data used by the sample. Data is held in memory and persisted
/// to device storage (Preferences + JSON) so it survives app restarts.
/// </summary>
public class VehicleDataService
{
    private const string StorageKey = "SmartVehicleCare_Store";

    private static VehicleDataService? _instance;
    public static VehicleDataService Instance => _instance ??= new VehicleDataService();

    /// <summary>
    /// The sample keeps one in-memory demo mode for the lifetime of the app process.
    /// </summary>
    public enum DataMode { Demo }

    private DataMode _currentMode = DataMode.Demo;
    private bool _storageLoaded;

    private VehicleDataService() { }

    /// <summary>
    /// Loads previously saved data from device storage (call once at app startup).
    /// Fails silently when nothing is stored yet — the wizard then seeds a fresh session.
    /// </summary>
    public void LoadFromStorage()
    {
        if (_storageLoaded) return;

        try
        {
            var json = Preferences.Get(StorageKey, string.Empty);
            if (string.IsNullOrWhiteSpace(json)) return;

            var store = JsonSerializer.Deserialize<VehicleStore>(json);
            if (store?.Vehicles is not { Count: > 0 }) return;

            Vehicles.Clear();
            foreach (var dto in store.Vehicles)
            {
                var vehicle = new Vehicle
                {
                    Id                    = dto.Id,
                    VehicleType           = dto.VehicleType,
                    Make                  = dto.Make,
                    Model                 = dto.Model,
                    Year                  = dto.Year,
                    Variant               = dto.Variant,
                    Color                 = dto.Color,
                    OdometerReading       = dto.OdometerReading,
                    RegistrationNumber    = dto.RegistrationNumber,
                    PurchaseYear          = dto.PurchaseYear,
                    ServiceInterval       = dto.ServiceInterval,
                    CustomServiceIntervalKm = dto.CustomServiceIntervalKm,
                    InsuranceRenewalDate  = new DateTime(dto.InsuranceRenewalDateTicks),
                    NextServiceDueDate    = new DateTime(dto.NextServiceDueDateTicks),
                    CreatedDate           = new DateTime(dto.CreatedDateTicks),
                    IsActive              = dto.IsActive,
                };
                Vehicles.Add(vehicle);
            }

            if (store.Services is not null)
            {
                foreach (var dto in store.Services)
                {
                    if (!_vehicleServices.TryGetValue(dto.VehicleId, out _))
                        _vehicleServices[dto.VehicleId] = new();
                    _vehicleServices[dto.VehicleId].Add(ToServiceRecord(dto));
                }
            }

            if (store.Fuel is not null)
            {
                foreach (var dto in store.Fuel)
                {
                    if (!_vehicleFuel.TryGetValue(dto.VehicleId, out _))
                        _vehicleFuel[dto.VehicleId] = new();
                    _vehicleFuel[dto.VehicleId].Add(ToFuelEntry(dto));
                }
            }

            if (store.Reminders is not null)
            {
                foreach (var dto in store.Reminders)
                {
                    if (!_vehicleReminders.TryGetValue(dto.VehicleId, out _))
                        _vehicleReminders[dto.VehicleId] = new();
                    _vehicleReminders[dto.VehicleId].Add(ToReminder(dto));
                }
            }

            SelectedVehicle = Vehicles.FirstOrDefault();
            _currentMode = DataMode.Demo;
            _storageLoaded = true;
            System.Diagnostics.Debug.WriteLine("[VehicleDataService] Restored data from device storage");
        }
        catch (Exception ex)
        {
            // Corrupt/unreadable store — start clean rather than crash the app.
            System.Diagnostics.Debug.WriteLine($"[VehicleDataService] Failed to load store: {ex.Message}");
        }
    }

    /// <summary>
    /// Persists the current state to device storage as JSON.
    /// Serialized under a lock — mutation events can arrive from any thread.
    /// </summary>
    public void SaveToStorage()
    {
        lock (_saveLock)
        {
        try
        {
            var store = new VehicleStore
            {
                Vehicles = Vehicles.Select(v => new VehicleDto
                {
                    Id                    = v.Id,
                    VehicleType           = v.VehicleType,
                    Make                  = v.Make,
                    Model                 = v.Model,
                    Year                  = v.Year,
                    Variant               = v.Variant,
                    Color                 = v.Color,
                    OdometerReading       = v.OdometerReading,
                    RegistrationNumber    = v.RegistrationNumber,
                    PurchaseYear          = v.PurchaseYear,
                    ServiceInterval       = v.ServiceInterval,
                    CustomServiceIntervalKm = v.CustomServiceIntervalKm,
                    InsuranceRenewalDateTicks  = v.InsuranceRenewalDate.Ticks,
                    NextServiceDueDateTicks    = v.NextServiceDueDate.Ticks,
                    CreatedDateTicks           = v.CreatedDate.Ticks,
                    IsActive              = v.IsActive,
                }).ToList(),
                Services = _vehicleServices
                    .SelectMany(kv => kv.Value.Select(r => (kv.Key, r)))
                    .Select(x => new ServiceDto
                    {
                        VehicleId   = x.Key,
                        ServiceDate = x.r.ServiceDate,
                        ServiceType = x.r.ServiceType,
                        Workshop    = x.r.Workshop,
                        Amount      = x.r.Amount,
                        Mileage     = x.r.Mileage,
                        Status      = x.r.Status,
                        Notes       = x.r.Notes,
                    }).ToList(),
                Fuel = _vehicleFuel
                    .SelectMany(kv => kv.Value.Select(e => (kv.Key, e)))
                    .Select(x => new FuelDto
                    {
                        VehicleId       = x.Key,
                        FuelDate        = x.e.FuelDate,
                        FuelType        = x.e.FuelType,
                        Station         = x.e.Station,
                        LitresFilled    = x.e.LitresFilled,
                        CostPerLitre    = x.e.CostPerLitre,
                        OdometerReading = x.e.OdometerReading,
                        IsFullTank      = x.e.IsFullTank,
                    }).ToList(),
                Reminders = _vehicleReminders
                    .SelectMany(kv => kv.Value.Select(r => (kv.Key, r)))
                    .Select(x => new ReminderDto
                    {
                        VehicleId        = x.Key,
                        ReminderType     = x.r.ReminderType,
                        Title            = x.r.Title,
                        DueDate          = x.r.DueDate,
                        DueTimeTicks     = x.r.DueTime.Ticks,
                        Priority         = x.r.Priority,
                        RemindBeforeDays = x.r.RemindBeforeDays,
                        IsRepeat         = x.r.IsRepeat,
                    }).ToList(),
            };
            Preferences.Set(StorageKey, JsonSerializer.Serialize(store));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[VehicleDataService] Failed to save store: {ex.Message}");
        }
        }
    }

    private readonly object _saveLock = new();

    public ObservableCollection<Vehicle> Vehicles { get; } = new();
    
    /// <summary>
    /// Gets the current data mode.
    /// </summary>
    public DataMode CurrentMode => _currentMode;
    
    /// <summary>
    /// Returns true when the sample is using its in-memory demo mode.
    /// </summary>
    public bool IsDemoMode => _currentMode == DataMode.Demo;
    
    /// <summary>
    private Vehicle? _selectedVehicle;
    public Vehicle? SelectedVehicle
    {
        get => _selectedVehicle;
        set
        {
            if (_selectedVehicle == value) return;
            _selectedVehicle = value;
            SelectedVehicleChanged?.Invoke(value);
        }
    }

    public event Action<Vehicle?>? SelectedVehicleChanged;
    public event Action<DataMode>? ModeChanged;
    public event Action? DataReloaded;
    // Fired when any per-vehicle data changes (service/fuel/doc/reminder added)
    public event Action<int>? DataChanged;

    public void NotifyDataChanged(int vehicleId)
    {
        DataChanged?.Invoke(vehicleId);
        SaveToStorage();
    }

    public void NotifyDataReloaded()
    {
        DataReloaded?.Invoke();
        SaveToStorage();
    }

    public void ClearAllData()
    {
        Vehicles.Clear();
        if (_selectedVehicle != null)
        {
            _selectedVehicle = null;
            SelectedVehicleChanged?.Invoke(null);
        }
        _vehicleServices.Clear();
        _vehicleFuel.Clear();
        _vehicleReminders.Clear();
    }


    /// <summary>
    /// Switches to Demo mode and loads sample data.
    /// Clears all existing data and starts a fresh demo session.
    /// </summary>
    public bool TryLoadDemoData()
    {
        ClearAllData();
        _currentMode = DataMode.Demo;
        ModeChanged?.Invoke(_currentMode);
        
        System.Diagnostics.Debug.WriteLine("[VehicleDataService] Started a fresh demo session");
        return true;
    }

    public void AddVehicle(Vehicle vehicle)
    {
        if (vehicle == null) return;

        // Always allocate a fresh, unused id (max existing id + 1).
        if (vehicle.Id <= 0 || Vehicles.Any(v => v.Id == vehicle.Id))
            vehicle.Id = Vehicles.Count == 0 ? 1 : Vehicles.Max(v => v.Id) + 1;

        Vehicles.Add(vehicle);
        SelectedVehicle ??= vehicle;
        NotifyDataChanged(vehicle.Id);
    }

    /// <summary>Persist in-place edits to a vehicle already in the collection and notify listeners.</summary>
    public void UpdateVehicle(Vehicle vehicle)
    {
        if (vehicle == null) return;
        NotifyDataChanged(vehicle.Id);
    }

    public void DeleteVehicle(Vehicle vehicle)
    {
        if (vehicle == null || !Vehicles.Contains(vehicle)) return;

        var wasSelected = ReferenceEquals(SelectedVehicle, vehicle);
        Vehicles.Remove(vehicle);
        _vehicleServices.Remove(vehicle.Id);
        _vehicleFuel.Remove(vehicle.Id);
        _vehicleReminders.Remove(vehicle.Id);
        NotifyDataChanged(vehicle.Id);

        if (wasSelected)
            SelectedVehicle = Vehicles.LastOrDefault();
    }

    // ── Per-vehicle service records ───────────────────────────────────────────

    private readonly Dictionary<int, List<ServiceRecord>> _vehicleServices = new();

    public void AddServiceRecord(int vehicleId, ServiceRecord record)
    {
        if (!_vehicleServices.ContainsKey(vehicleId)) _vehicleServices[vehicleId] = new();
        _vehicleServices[vehicleId].Insert(0, record);
        NotifyDataChanged(vehicleId);
    }

    public void UpdateServiceRecord(int vehicleId, ServiceRecord record)
    {
        if (!_vehicleServices.TryGetValue(vehicleId, out var records) || !records.Contains(record)) return;
        NotifyDataChanged(vehicleId);
    }

    public IReadOnlyList<ServiceRecord> GetServiceRecords(int vehicleId)
        => _vehicleServices.TryGetValue(vehicleId, out var r) ? r.AsReadOnly() : Array.Empty<ServiceRecord>();

    // ── Per-vehicle fuel entries ──────────────────────────────────────────────

    private readonly Dictionary<int, List<FuelEntry>> _vehicleFuel = new();

    public void AddFuelEntry(int vehicleId, FuelEntry entry)
    {
        if (!_vehicleFuel.ContainsKey(vehicleId)) _vehicleFuel[vehicleId] = new();
        _vehicleFuel[vehicleId].Insert(0, entry);
        NotifyDataChanged(vehicleId);
    }

    public void UpdateFuelEntry(int vehicleId, FuelEntry entry)
    {
        if (!_vehicleFuel.TryGetValue(vehicleId, out var entries) || !entries.Contains(entry)) return;
        NotifyDataChanged(vehicleId);
    }

    public IReadOnlyList<FuelEntry> GetFuelEntries(int vehicleId)
        => _vehicleFuel.TryGetValue(vehicleId, out var f) ? f.AsReadOnly() : Array.Empty<FuelEntry>();

    public void RemoveServiceRecord(int vehicleId, ServiceRecord record)
    {
        if (_vehicleServices.TryGetValue(vehicleId, out var list))
            list.Remove(record);
        NotifyDataChanged(vehicleId);
    }

    public void RemoveFuelEntry(int vehicleId, FuelEntry entry)
    {
        if (_vehicleFuel.TryGetValue(vehicleId, out var list))
            list.Remove(entry);
        NotifyDataChanged(vehicleId);
    }

    // ── Per-vehicle schedule reminders ────────────────────────────────────────

    private readonly Dictionary<int, List<ScheduleReminder>> _vehicleReminders = new();

    public void AddReminder(int vehicleId, ScheduleReminder reminder)
    {
        if (!_vehicleReminders.ContainsKey(vehicleId)) _vehicleReminders[vehicleId] = new();
        _vehicleReminders[vehicleId].Insert(0, reminder);
        NotifyDataChanged(vehicleId);
    }

    public IReadOnlyList<ScheduleReminder> GetReminders(int vehicleId)
        => _vehicleReminders.TryGetValue(vehicleId, out var r) ? r.AsReadOnly() : Array.Empty<ScheduleReminder>();

    public void RemoveReminder(int vehicleId, ScheduleReminder reminder)
    {
        if (_vehicleReminders.TryGetValue(vehicleId, out var list))
            list.Remove(reminder);
        NotifyDataChanged(vehicleId);
    }

    // ── JSON serialization helpers ────────────────────────────────────────────

    private static ServiceRecord ToServiceRecord(ServiceDto dto)
    {
        var record = new ServiceRecord
        {
            ServiceDate = dto.ServiceDate,
            ServiceType = dto.ServiceType,
            Workshop    = dto.Workshop,
            Amount      = dto.Amount,
            Mileage     = dto.Mileage,
            Status      = dto.Status,
            Notes       = dto.Notes,
        };
        switch (dto.Status)
        {
            case "Pending":
                record.StatusColor = Color.FromArgb("#D97706");
                record.StatusBgColor = Color.FromArgb("#FEF3C7");
                break;
            case "Scheduled":
                record.StatusColor = Color.FromArgb("#3B5BDB");
                record.StatusBgColor = Color.FromArgb("#EEF2FF");
                break;
            default:
                record.StatusColor = Color.FromArgb("#16A34A");
                record.StatusBgColor = Color.FromArgb("#DCFCE7");
                break;
        }
        return record;
    }

    private static FuelEntry ToFuelEntry(FuelDto dto)
    {
        var entry = new FuelEntry
        {
            FuelDate        = dto.FuelDate,
            FuelType        = dto.FuelType,
            Station         = dto.Station,
            LitresFilled    = dto.LitresFilled,
            CostPerLitre    = dto.CostPerLitre,
            OdometerReading = dto.OdometerReading,
            IsFullTank      = dto.IsFullTank,
        };
        switch (dto.FuelType)
        {
            case "Diesel":
                entry.FuelTypeColor = Color.FromArgb("#065F46");
                entry.FuelTypeBgColor = Color.FromArgb("#D1FAE5");
                break;
            case "CNG":
                entry.FuelTypeColor = Color.FromArgb("#D97706");
                entry.FuelTypeBgColor = Color.FromArgb("#FEF3C7");
                break;
            case "Electric":
                entry.FuelTypeColor = Color.FromArgb("#2563EB");
                entry.FuelTypeBgColor = Color.FromArgb("#DBEAFE");
                break;
            default:
                entry.FuelTypeColor = Color.FromArgb("#DC2626");
                entry.FuelTypeBgColor = Color.FromArgb("#FEE2E2");
                break;
        }
        return entry;
    }

    private static ScheduleReminder ToReminder(ReminderDto dto) => new()
    {
        ReminderType     = dto.ReminderType,
        Title            = dto.Title,
        DueDate          = dto.DueDate,
        DueTime          = new TimeSpan(dto.DueTimeTicks),
        Priority         = dto.Priority,
        RemindBeforeDays = dto.RemindBeforeDays,
        IsRepeat         = dto.IsRepeat,
    };

    // ── DTOs ──────────────────────────────────────────────────────────────────

    private class VehicleStore
    {
        public List<VehicleDto>  Vehicles { get; set; } = new();
        public List<ServiceDto>  Services { get; set; } = new();
        public List<FuelDto>     Fuel     { get; set; } = new();
        public List<ReminderDto> Reminders{ get; set; } = new();
    }

    private class VehicleDto
    {
        public int Id { get; set; }
        public int VehicleType { get; set; }
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Year { get; set; } = string.Empty;
        public string Variant { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string OdometerReading { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public string PurchaseYear { get; set; } = string.Empty;
        public string ServiceInterval { get; set; } = string.Empty;
        public string CustomServiceIntervalKm { get; set; } = string.Empty;
        public long InsuranceRenewalDateTicks { get; set; }
        public long NextServiceDueDateTicks { get; set; }
        public long CreatedDateTicks { get; set; }
        public bool IsActive { get; set; }
    }

    private class ServiceDto
    {
        public int VehicleId { get; set; }
        public DateTime ServiceDate { get; set; }
        public string ServiceType { get; set; } = string.Empty;
        public string Workshop { get; set; } = string.Empty;
        public string Amount { get; set; } = string.Empty;
        public string Mileage { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    private class FuelDto
    {
        public int VehicleId { get; set; }
        public DateTime FuelDate { get; set; }
        public string FuelType { get; set; } = string.Empty;
        public string Station { get; set; } = string.Empty;
        public double LitresFilled { get; set; }
        public double CostPerLitre { get; set; }
        public int OdometerReading { get; set; }
        public bool IsFullTank { get; set; }
    }

    private class ReminderDto
    {
        public int VehicleId { get; set; }
        public string ReminderType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public long DueTimeTicks { get; set; }
        public string Priority { get; set; } = string.Empty;
        public int RemindBeforeDays { get; set; }
        public bool IsRepeat { get; set; }
    }
}
