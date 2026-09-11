using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SmartVehicleCare.Models;
using SmartVehicleCare.Services;

namespace SmartVehicleCare.ViewModels;

public class SettingsViewModel : INotifyPropertyChanged
{
    public SettingsViewModel()
    {
        _selectedTheme = Application.Current?.UserAppTheme switch
        {
            AppTheme.Light => "Light",
            AppTheme.Dark  => "Dark",
            _              => "System default"
        };

        SetLightThemeCommand  = new Command(() => ApplyTheme(AppTheme.Light));
        SetDarkThemeCommand   = new Command(() => ApplyTheme(AppTheme.Dark));
        SetSystemThemeCommand = new Command(() => ApplyTheme(AppTheme.Unspecified));

        ExitToWelcomeCommand = new Command(async () =>
        {
            // Wipe all persisted state so the app starts clean on next launch
            await AzureOpenAIService.SaveApiKeyToStorageAsync(string.Empty);
            Preferences.Default.Clear();
            VehicleDataService.Instance.FullReset();
            await Shell.Current.GoToAsync("///welcome");
        });

        SaveApiKeyCommand = new Command(async () =>
        {
            if (!string.IsNullOrWhiteSpace(_apiKeyEntry))
                await AzureOpenAIService.SaveApiKeyToStorageAsync(_apiKeyEntry);
            ApiKeyEntry = string.Empty;
            RefreshApiKeyStatus();
        });

        ClearApiKeyCommand = new Command(async () =>
        {
            await AzureOpenAIService.SaveApiKeyToStorageAsync(string.Empty);
            RefreshApiKeyStatus();
        });

        // Sync status when the key is saved or cleared from any other screen (e.g. dashboard popup)
        AzureOpenAIService.ApiKeyChanged += () =>
            MainThread.BeginInvokeOnMainThread(RefreshApiKeyStatus);

        EditVehicleCommand = new Command<Vehicle>(v =>
        {
            if (v == null) return;
            EditingVehicle = new VehicleEditState
            {
                Id                   = v.Id,
                Make                 = v.Make,
                Model                = v.Model,
                Year                 = v.Year,
                Variant              = v.Variant,
                Color                = v.Color,
                OdaMeterReading      = v.OdometerReading,
                ServiceInterval      = v.ServiceInterval
            };
            IsEditVehiclePanelVisible = true;
        });

        SaveVehicleCommand = new Command(() =>
        {
            if (EditingVehicle == null) return;
            var target = VehicleDataService.Instance.Vehicles
                .FirstOrDefault(v => v.Id == EditingVehicle.Id);
            if (target == null) return;

            target.Make               = EditingVehicle.Make;
            target.Model              = EditingVehicle.Model;
            target.Year               = EditingVehicle.Year;
            target.Variant            = EditingVehicle.Variant;
            target.Color              = EditingVehicle.Color;
            target.OdometerReading    = EditingVehicle.OdaMeterReading;
            target.ServiceInterval    = EditingVehicle.ServiceInterval;

            VehicleDataService.Instance.UpdateVehicle(target);
            OnPropertyChanged(nameof(Vehicles));
            IsEditVehiclePanelVisible = false;
            EditingVehicle = null;
        });

        CancelEditCommand = new Command(() =>
        {
            IsEditVehiclePanelVisible = false;
            EditingVehicle = null;
        });

        DeleteVehicleCommand = new Command<Vehicle>(async vehicle =>
        {
            if (vehicle == null) return;

            var confirmed = await Shell.Current.DisplayAlertAsync(
                "Delete vehicle",
                $"Delete {vehicle.MakeModel} and all of its service, fuel, and schedule data?",
                "Delete",
                "Cancel");
            if (!confirmed) return;

            VehicleDataService.Instance.DeleteVehicle(vehicle);
            IsEditVehiclePanelVisible = false;
            EditingVehicle = null;
            OnPropertyChanged(nameof(HasVehicles));
            OnPropertyChanged(nameof(NoVehicles));
        });

        Vehicles.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(HasVehicles));
            OnPropertyChanged(nameof(NoVehicles));
        };
    }

    // ── Exit ──────────────────────────────────────────────────────────────────

    public ICommand ExitToWelcomeCommand { get; }

    // ── AI API Key & Deployment ───────────────────────────────────────────────

    private string _apiKeyEntry = string.Empty;
    public string ApiKeyEntry
    {
        get => _apiKeyEntry;
        set => SetProperty(ref _apiKeyEntry, value);
    }

    public bool HasApiKey => AzureOpenAIService.HasApiKey;
    public string ApiKeyStatusText => AzureOpenAIService.HasApiKey
        ? "API key configured \u2014 AI features active."
        : "No API key \u2014 AI features disabled.";
    public string ApiKeyHint => AzureOpenAIService.HasApiKey
        ? "Key saved \u2014 remove to change"
        : "Paste your Azure OpenAI API key";

    public ICommand SaveApiKeyCommand  { get; }
    public ICommand ClearApiKeyCommand { get; }

    private async Task LoadApiKeyStatusAsync()
    {
        await AzureOpenAIService.LoadApiKeyFromStorageAsync();
        RefreshApiKeyStatus();
    }

    private void RefreshApiKeyStatus()
    {
        OnPropertyChanged(nameof(HasApiKey));
        OnPropertyChanged(nameof(ApiKeyStatusText));
        OnPropertyChanged(nameof(ApiKeyHint));
    }

    // ── Theme ─────────────────────────────────────────────────────────────────

    public ICommand SetLightThemeCommand  { get; }
    public ICommand SetDarkThemeCommand   { get; }
    public ICommand SetSystemThemeCommand { get; }

    public List<string> ThemeOptions { get; } = new() { "Light", "System Default", "Dark" };

    private string _selectedTheme;
    public string SelectedTheme
    {
        get => _selectedTheme;
        set
        {
            if (SetProperty(ref _selectedTheme, value))
                ApplyTheme(value switch
                {
                    "Light" => AppTheme.Light,
                    "Dark"  => AppTheme.Dark,
                    _       => AppTheme.Unspecified
                });
        }
    }

    public bool IsLightActive  => Application.Current?.UserAppTheme == AppTheme.Light;
    public bool IsDarkActive   => Application.Current?.UserAppTheme == AppTheme.Dark;
    public bool IsSystemActive => Application.Current?.UserAppTheme == AppTheme.Unspecified;

    private void ApplyTheme(AppTheme theme)
    {
        if (Application.Current == null) return;
        Application.Current.UserAppTheme = theme;
        OnPropertyChanged(nameof(IsLightActive));
        OnPropertyChanged(nameof(IsDarkActive));
        OnPropertyChanged(nameof(IsSystemActive));
    }

    // ── Vehicles ──────────────────────────────────────────────────────────────

    public ObservableCollection<Vehicle> Vehicles => VehicleDataService.Instance.Vehicles;
    public bool HasVehicles => Vehicles.Count > 0;
    public bool NoVehicles  => Vehicles.Count == 0;

    public ICommand EditVehicleCommand { get; }
    public ICommand SaveVehicleCommand { get; }
    public ICommand CancelEditCommand  { get; }
    public ICommand DeleteVehicleCommand { get; }

    private bool _isEditVehiclePanelVisible;
    public bool IsEditVehiclePanelVisible
    {
        get => _isEditVehiclePanelVisible;
        set => SetProperty(ref _isEditVehiclePanelVisible, value);
    }

    private VehicleEditState? _editingVehicle;
    public VehicleEditState? EditingVehicle
    {
        get => _editingVehicle;
        set => SetProperty(ref _editingVehicle, value);
    }

    public string EditPanelVehicleName => EditingVehicle != null
        ? $"{EditingVehicle.Make} {EditingVehicle.Model}".Trim()
        : "Vehicle";

    // ── About ─────────────────────────────────────────────────────────────────

    public string AppVersion     => $"v{AppInfo.VersionString}";
    public string AppBuildNumber => $"Build {AppInfo.BuildString}";
    public string AppPlatform    => DeviceInfo.Platform.ToString();

    // ── INotifyPropertyChanged ────────────────────────────────────────────────

    public event PropertyChangedEventHandler? PropertyChanged;

    protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string prop = "")
    {
        if (EqualityComparer<T>.Default.Equals(storage, value)) return false;
        storage = value;
        OnPropertyChanged(prop);
        return true;
    }

    protected void OnPropertyChanged([CallerMemberName] string prop = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
}

/// <summary>Mutable copy of a vehicle used while editing in the Settings panel.</summary>
public class VehicleEditState : INotifyPropertyChanged
{
    public int Id { get; set; }

    private string _make = string.Empty;
    public string Make { get => _make; set { _make = value; OnPropertyChanged(); } }

    private string _model = string.Empty;
    public string Model { get => _model; set { _model = value; OnPropertyChanged(); } }

    private string _year = string.Empty;
    public string Year { get => _year; set { _year = value; OnPropertyChanged(); } }

    private string _variant = string.Empty;
    public string Variant { get => _variant; set { _variant = value; OnPropertyChanged(); } }

    private string _color = string.Empty;
    public string Color { get => _color; set { _color = value; OnPropertyChanged(); } }

    private string _odaMeterReading = string.Empty;
    public string OdaMeterReading { get => _odaMeterReading; set { _odaMeterReading = value; OnPropertyChanged(); } }

    private string _serviceInterval = string.Empty;
    public string ServiceInterval { get => _serviceInterval; set { _serviceInterval = value; OnPropertyChanged(); } }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string prop = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
}
