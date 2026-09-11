using System.Collections.ObjectModel;
using SmartVehicleCare.Models;

namespace SmartVehicleCare.Services;

/// <summary>
/// Owns the in-memory demo data used by the sample.
/// </summary>
public class VehicleDataService
{
    private static VehicleDataService? _instance;
    public static VehicleDataService Instance => _instance ??= new VehicleDataService();

    /// <summary>
    /// The sample keeps one in-memory demo mode for the lifetime of the app process.
    /// </summary>
    public enum DataMode { Demo, Real }

    private DataMode _currentMode = DataMode.Demo;

    private VehicleDataService() { }

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
    public void NotifyDataChanged(int vehicleId) => DataChanged?.Invoke(vehicleId);
    public void NotifyDataReloaded() => DataReloaded?.Invoke();

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

    public void FullReset()
    {
        ClearAllData();
        _currentMode = DataMode.Demo;
        ModeChanged?.Invoke(_currentMode);
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

        if (vehicle.Id <= 0)
            vehicle.Id = Vehicles.Count == 0 ? 1 : Vehicles.Max(v => v.Id) + 1;
        else if (Vehicles.Any(v => v.Id == vehicle.Id))
            vehicle.Id = Vehicles.Count == 0 ? 1 : Vehicles.Max(v => v.Id) + 1;

        Vehicles.Add(vehicle);
        SelectedVehicle ??= vehicle;
        
    }

    /// <summary>Persist in-place edits to a vehicle already in the collection and notify listeners.</summary>
    public void UpdateVehicle(Vehicle vehicle)
    {
        if (vehicle == null) return;
        DataChanged?.Invoke(vehicle.Id);
    }

    public void DeleteVehicle(Vehicle vehicle)
    {
        if (vehicle == null || !Vehicles.Contains(vehicle)) return;

        var wasSelected = ReferenceEquals(SelectedVehicle, vehicle);
        Vehicles.Remove(vehicle);
        _vehicleServices.Remove(vehicle.Id);
        _vehicleFuel.Remove(vehicle.Id);
        _vehicleReminders.Remove(vehicle.Id);
        DataChanged?.Invoke(vehicle.Id);

        if (wasSelected)
            SelectedVehicle = Vehicles.LastOrDefault();
    }

    // ── Per-vehicle service records ───────────────────────────────────────────

    private readonly Dictionary<int, List<ServiceRecord>> _vehicleServices = new();

    public void AddServiceRecord(int vehicleId, ServiceRecord record)
    {
        if (!_vehicleServices.ContainsKey(vehicleId)) _vehicleServices[vehicleId] = new();
        _vehicleServices[vehicleId].Insert(0, record);
        DataChanged?.Invoke(vehicleId);
    }

    public void UpdateServiceRecord(int vehicleId, ServiceRecord record)
    {
        if (!_vehicleServices.TryGetValue(vehicleId, out var records) || !records.Contains(record)) return;
        DataChanged?.Invoke(vehicleId);
    }

    public IReadOnlyList<ServiceRecord> GetServiceRecords(int vehicleId)
        => _vehicleServices.TryGetValue(vehicleId, out var r) ? r.AsReadOnly() : Array.Empty<ServiceRecord>();

    // ── Per-vehicle fuel entries ──────────────────────────────────────────────

    private readonly Dictionary<int, List<FuelEntry>> _vehicleFuel = new();

    public void AddFuelEntry(int vehicleId, FuelEntry entry)
    {
        if (!_vehicleFuel.ContainsKey(vehicleId)) _vehicleFuel[vehicleId] = new();
        _vehicleFuel[vehicleId].Insert(0, entry);
        DataChanged?.Invoke(vehicleId);
    }

    public void UpdateFuelEntry(int vehicleId, FuelEntry entry)
    {
        if (!_vehicleFuel.TryGetValue(vehicleId, out var entries) || !entries.Contains(entry)) return;
        DataChanged?.Invoke(vehicleId);
    }

    public IReadOnlyList<FuelEntry> GetFuelEntries(int vehicleId)
        => _vehicleFuel.TryGetValue(vehicleId, out var f) ? f.AsReadOnly() : Array.Empty<FuelEntry>();

    public void RemoveServiceRecord(int vehicleId, ServiceRecord record)
    {
        if (_vehicleServices.TryGetValue(vehicleId, out var list))
            list.Remove(record);
        DataChanged?.Invoke(vehicleId);
    }

    public void RemoveFuelEntry(int vehicleId, FuelEntry entry)
    {
        if (_vehicleFuel.TryGetValue(vehicleId, out var list))
            list.Remove(entry);
        DataChanged?.Invoke(vehicleId);
    }

    // ── Per-vehicle schedule reminders ────────────────────────────────────────

    private readonly Dictionary<int, List<ScheduleReminder>> _vehicleReminders = new();

    public void AddReminder(int vehicleId, ScheduleReminder reminder)
    {
        if (!_vehicleReminders.ContainsKey(vehicleId)) _vehicleReminders[vehicleId] = new();
        _vehicleReminders[vehicleId].Insert(0, reminder);
        DataChanged?.Invoke(vehicleId);
    }

    public IReadOnlyList<ScheduleReminder> GetReminders(int vehicleId)
        => _vehicleReminders.TryGetValue(vehicleId, out var r) ? r.AsReadOnly() : Array.Empty<ScheduleReminder>();

    public void RemoveReminder(int vehicleId, ScheduleReminder reminder)
    {
        if (_vehicleReminders.TryGetValue(vehicleId, out var list))
            list.Remove(reminder);
        DataChanged?.Invoke(vehicleId);
    }
}
