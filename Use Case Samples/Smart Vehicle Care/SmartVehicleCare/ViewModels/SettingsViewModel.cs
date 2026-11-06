using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
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

        ExitToWelcomeCommand = new Command(async () =>
        {
            // Sign out: clear only the AI credentials and return to the welcome wizard.
            await AzureOpenAIService.SaveApiKeyToStorageAsync(string.Empty);
            OnNavigateToWelcomeRequested?.Invoke();
        });

        SaveApiKeyCommand = new Command(async () =>
        {
            if (!string.IsNullOrWhiteSpace(_apiKeyEntry))
                await AzureOpenAIService.SaveApiKeyToStorageAsync(_apiKeyEntry);
            ApiKeyEntry = string.Empty;
            RefreshAiStatus();
        });

        ClearApiKeyCommand = new Command(async () =>
        {
            await AzureOpenAIService.SaveApiKeyToStorageAsync(string.Empty);
            RefreshAiStatus();
        });

        // Restore endpoint + deployment name persisted from a previous session.
        AzureOpenAIService.LoadEndpoint();
        AzureOpenAIService.LoadDeploymentName();
        DeploymentNameEntry = AzureOpenAIService.DeploymentName;

        SaveEndpointCommand = new Command(() =>
        {
            AzureOpenAIService.SaveEndpoint(_endpointEntry);
            EndpointEntry = string.Empty;
            RefreshAiStatus();
        });

        ClearEndpointCommand = new Command(() =>
        {
            AzureOpenAIService.SaveEndpoint(string.Empty);
            RefreshAiStatus();
        });

        SaveDeploymentNameCommand = new Command(() =>
        {
            AzureOpenAIService.SaveDeploymentName(string.IsNullOrWhiteSpace(_deploymentNameEntry)
                ? null : _deploymentNameEntry);
            DeploymentNameEntry = AzureOpenAIService.DeploymentName;
        });

        // Sync status when the key is saved or cleared from any other screen (e.g. dashboard popup)
        AzureOpenAIService.ApiKeyChanged += () =>
            MainThread.BeginInvokeOnMainThread(RefreshAiStatus);

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

            // Confirm via the view-provided dialog before irreversible deletion.
            bool confirmed = false;
            var handler = OnDeleteVehicleConfirmationRequested;
            if (handler != null)
                confirmed = await handler(vehicle);
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
    
    // Events for navigation and confirmation dialogs (MVVM compliant)
    // Delegate properties for view-driven dialogs (MVVM compliant; assigned by the view, not subscribed)
    public Func<Vehicle, Task<bool>>? OnDeleteVehicleConfirmationRequested { get; set; }
    public event Action OnNavigateToWelcomeRequested;

    // ── AI API Key, Endpoint & Deployment ────────────────────────────────────

    private string _apiKeyEntry = string.Empty;
    public string ApiKeyEntry
    {
        get => _apiKeyEntry;
        set => SetProperty(ref _apiKeyEntry, value);
    }

    public bool HasApiKey => AzureOpenAIService.HasApiKey;
    public string ApiKeyHint => AzureOpenAIService.HasApiKey
        ? "Key saved — remove to change"
        : "Paste your Azure OpenAI API key";

    private string _endpointEntry = string.Empty;
    public string EndpointEntry
    {
        get => _endpointEntry;
        set => SetProperty(ref _endpointEntry, value);
    }

    public bool HasEndpoint => AzureOpenAIService.HasEndpoint;
    public string EndpointHint => AzureOpenAIService.HasEndpoint
        ? AzureOpenAIService.BaseEndpoint
        : "https://<your-project>.openai.azure.com/openai/v1";

    private string _deploymentNameEntry = string.Empty;
    public string DeploymentNameEntry
    {
        get => _deploymentNameEntry;
        set => SetProperty(ref _deploymentNameEntry, value);
    }

    public string AiStatusText =>
        (AzureOpenAIService.HasApiKey, AzureOpenAIService.HasEndpoint) switch
        {
            (true, true)  => "AI configured — key and endpoint are set.",
            (true, false) => "API key set, but the endpoint is missing.",
            (false, true) => "Endpoint set, but the API key is missing.",
            _             => "No AI credentials — AI features are disabled.",
        };

    public ICommand SaveApiKeyCommand  { get; }
    public ICommand ClearApiKeyCommand { get; }
    public ICommand SaveEndpointCommand { get; }
    public ICommand ClearEndpointCommand { get; }
    public ICommand SaveDeploymentNameCommand { get; }

    private void RefreshAiStatus()
    {
        OnPropertyChanged(nameof(HasApiKey));
        OnPropertyChanged(nameof(ApiKeyHint));
        OnPropertyChanged(nameof(HasEndpoint));
        OnPropertyChanged(nameof(EndpointHint));
        OnPropertyChanged(nameof(AiStatusText));
    }

    // ── Theme ─────────────────────────────────────────────────────────────────

    public List<string> ThemeOptions { get; } = new() { "Light", "System default", "Dark" };

    private string _selectedTheme;
    public string SelectedTheme
    {
        get => _selectedTheme;
        set
        {
            if (SetProperty(ref _selectedTheme, value))
                OnThemeChangeRequested?.Invoke();
        }
    }

    /// <summary>Raised when the theme choice changes; the view applies the platform theme.</summary>
    public event Action? OnThemeChangeRequested;

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
