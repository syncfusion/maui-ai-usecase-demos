using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Syncfusion.Maui.AIAssistView;
using SmartVehicleCare.Models;
using SmartVehicleCare.Services;

namespace SmartVehicleCare.ViewModels;

public class AIRecentInsight
{
    public string Text        { get; set; } = string.Empty;
    public string Badge       { get; set; } = string.Empty;
    public Color  BadgeColor  { get; set; } = Colors.Gray;
    public Color  BadgeBgColor { get; set; } = Colors.Transparent;
}

public class AIAssistViewModel : INotifyPropertyChanged
{
    private readonly AzureOpenAIService _ai = new();
    private Vehicle? _selectedVehicle;
    private double _vehicleHealthScore;
    private string _vehicleHealthLabel = "—";

    public ObservableCollection<Vehicle> Vehicles { get; } = new();

    public Vehicle? SelectedVehicle
    {
        get => _selectedVehicle;
        set
        {
            if (SetProperty(ref _selectedVehicle, value))
            {
                OnPropertyChanged(nameof(VehicleDisplayName));
                OnPropertyChanged(nameof(VehicleVin));
                OnPropertyChanged(nameof(VehicleOdometer));
                OnPropertyChanged(nameof(VehicleIconGlyph));
                OnPropertyChanged(nameof(VehicleHealthLabel));
                OnPropertyChanged(nameof(VehicleHealthBadgeColor));
                RebuildRecentInsights();
            }
        }
    }

    public string VehicleDisplayName => SelectedVehicle?.MakeModel ?? "Your vehicle";
    public string VehicleVin         => !string.IsNullOrEmpty(SelectedVehicle?.RegistrationNumber)
        ? SelectedVehicle.RegistrationNumber : "—";
    public string VehicleOdometer    => !string.IsNullOrEmpty(SelectedVehicle?.OdometerReading)
        ? $"{SelectedVehicle.OdometerReading} km" : "—";
    public string VehicleIconGlyph => SelectedVehicle?.VehicleType switch
    {
        1 => "\uE52F", // Motorcycle / bicycle icon
        _ => "\uE531"  // Car
    };
    public string VehicleHealthLabel => _vehicleHealthLabel;
    public Color VehicleHealthBadgeColor => _vehicleHealthLabel switch
    {
        "EXCELLENT" => Color.FromArgb("#16A34A"),
        "GOOD" => Color.FromArgb("#0EA5E9"),
        "FAIR" => Color.FromArgb("#F59E0B"),
        "POOR" => Color.FromArgb("#EA580C"),
        "CRITICAL" => Color.FromArgb("#DC2626"),
        _ => Color.FromArgb("#A8A29E") // neutral grey for no-data state
    };

    public ObservableCollection<IAssistItem> AssistItems { get; } = new();
    public ObservableCollection<AssistConversationItem> ConversationItems { get; } = new();

    public ObservableCollection<ISuggestion> Suggestions { get; } = new()
    {
        new AssistSuggestion { Text = "When is the next service schedule?"  },
        new AssistSuggestion { Text = "Analyze fuel efficiency"     },
        new AssistSuggestion { Text = "Vehicle health report"       },
    };

    public ObservableCollection<AIRecentInsight> RecentInsights { get; } = new();

    public bool HasChatItems => AssistItems.Count > 0;
    public bool HasRecentInsights => RecentInsights.Count > 0;

    public ICommand RequestCommand     { get; }
    public ICommand SuggestionTappedCommand { get; }

    public AIAssistViewModel()
    {
        RequestCommand = new Command<object>(ExecuteRequest);
        SuggestionTappedCommand = new Command<object>(ExecuteSuggestionRequest);
        InitializeVehicles();

        VehicleDataService.Instance.Vehicles.CollectionChanged += (_, e) =>
        {
            void SynchronizeVehicles()
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
                {
                    Vehicles.Clear();
                    SelectedVehicle = null;
                }
                else if (e.NewItems != null)
                    foreach (Vehicle vehicle in e.NewItems)
                        if (!Vehicles.Any(v => v.Id == vehicle.Id))
                            Vehicles.Add(vehicle);

                if (e.Action != System.Collections.Specialized.NotifyCollectionChangedAction.Reset && e.OldItems != null)
                    foreach (Vehicle vehicle in e.OldItems)
                    {
                        var existing = Vehicles.FirstOrDefault(v => v.Id == vehicle.Id);
                        if (existing != null) Vehicles.Remove(existing);
                    }

                var selected = VehicleDataService.Instance.SelectedVehicle;
                if (selected != null && Vehicles.Any(v => v.Id == selected.Id))
                    SelectedVehicle = selected;
            }

            if (MainThread.IsMainThread)
                SynchronizeVehicles();
            else
                MainThread.BeginInvokeOnMainThread(SynchronizeVehicles);
        };

        VehicleDataService.Instance.DataReloaded += () =>
        {
            void Synchronize()
            {
                Vehicles.Clear();
                foreach (var vehicle in VehicleDataService.Instance.Vehicles)
                    Vehicles.Add(vehicle);

                var selected = VehicleDataService.Instance.SelectedVehicle;
                SelectedVehicle = selected;
            }

            if (MainThread.IsMainThread)
                Synchronize();
            else
                MainThread.BeginInvokeOnMainThread(Synchronize);
        };
        
        // Subscribe to vehicle selection changes - clear chat when vehicle changes
        VehicleDataService.Instance.SelectedVehicleChanged += v =>
        {
            if (v != _selectedVehicle)
            {
                SelectedVehicle = v;
                ClearChat();
            }
        };

        VehicleDataService.Instance.DataChanged += vehicleId =>
        {
            if (SelectedVehicle?.Id != vehicleId) return;

            void Refresh()
            {
                RebuildRecentInsights();
                OnPropertyChanged(nameof(VehicleOdometer));
            }

            if (MainThread.IsMainThread)
                Refresh();
            else
                MainThread.BeginInvokeOnMainThread(Refresh);
        };
    }

    private void InitializeVehicles()
    {
        var svc = VehicleDataService.Instance;
        Vehicles.Clear();
        
        if (svc.Vehicles.Count > 0)
        {
            foreach (var v in svc.Vehicles) Vehicles.Add(v);
            SelectedVehicle = svc.SelectedVehicle ?? Vehicles[0];
        }
        else if (svc.IsDemoMode)
        {
            Vehicles.Add(new Vehicle { Id = 1, Make = "Hyundai", Model = "i20" });
            SelectedVehicle = Vehicles[0];
        }
        else
        {
            SelectedVehicle = null;
        }
        
        RebuildRecentInsights();
    }

    private void RebuildRecentInsights()
    {
        if (SelectedVehicle == null) return;

        RecentInsights.Clear();
        OnPropertyChanged(nameof(HasRecentInsights));
        var svc = VehicleDataService.Instance;
        var vid = SelectedVehicle.Id;

        // Calculate health score
        CalculateHealthScore(vid);

        // Get vehicle data
        var services = svc.GetServiceRecords(vid).OrderByDescending(s => s.ServiceDate).ToList();
        var fuels = svc.GetFuelEntries(vid).OrderByDescending(f => f.FuelDate).ToList();
        var reminders = svc.GetReminders(vid).OrderBy(r => r.DueDate).ToList();

        // Service insight
        if (services.Any())
        {
            var latest = services.First();
            var daysSince = (DateTime.Today - latest.ServiceDate).TotalDays;
            if (daysSince > 180)
            {
                RecentInsights.Add(new AIRecentInsight
                {
                    Text = $"Service overdue by {(int)(daysSince - 180)} days",
                    Badge = "High Priority",
                    BadgeColor = Color.FromArgb("#EF4444"),
                    BadgeBgColor = Color.FromArgb("#30EF4444")
                });
            }
            else if (daysSince > 90)
            {
                RecentInsights.Add(new AIRecentInsight
                {
                    Text = $"Service due in ~{(int)(180 - daysSince)} days",
                    Badge = "Medium",
                    BadgeColor = Color.FromArgb("#F59E0B"),
                    BadgeBgColor = Color.FromArgb("#30F59E0B")
                });
            }
            else
            {
                RecentInsights.Add(new AIRecentInsight
                {
                    Text = "Service is current and healthy",
                    Badge = "Good",
                    BadgeColor = Color.FromArgb("#22C55E"),
                    BadgeBgColor = Color.FromArgb("#3022C55E")
                });
            }
        }

        // Fuel insight
        if (fuels.Any())
        {
            var avgCost = fuels.Average(f => f.TotalCost);
            var latestSpend = fuels.First().TotalCost;
            var trend = latestSpend < avgCost ? "below average" : latestSpend > avgCost ? "above average" : "at average";
            
            RecentInsights.Add(new AIRecentInsight
            {
                Text = $"Fuel spending is {trend} at ₹{avgCost:N0}/fill",
                Badge = "Info",
                BadgeColor = Color.FromArgb("#0EA5E9"),
                BadgeBgColor = Color.FromArgb("#300EA5E9")
            });
        }

        OnPropertyChanged(nameof(HasRecentInsights));
    }

    private void CalculateHealthScore(int vehicleId)
    {
        var svc = VehicleDataService.Instance;
        var services = svc.GetServiceRecords(vehicleId).OrderByDescending(s => s.ServiceDate).ToList();
        var fuels = svc.GetFuelEntries(vehicleId).OrderByDescending(f => f.FuelDate).ToList();
        var reminders = svc.GetReminders(vehicleId).OrderBy(r => r.DueDate).ToList();

        var hasAnyData = services.Any() || fuels.Any() || reminders.Any();
        if (!hasAnyData)
        {
            _vehicleHealthScore = 0;
            _vehicleHealthLabel = "NO DATA";
            return;
        }

        var categories = new List<(double score, double weight)>();

        // Service score (40% weight)
        if (services.Any())
        {
            var days = (DateTime.Today - services.First().ServiceDate).TotalDays;
            double score = days > 365 ? 20 : days > 180 ? 50 : days > 90 ? 80 : 100;
            categories.Add((score, 40));
        }

        // Reminder score (15% weight)
        if (reminders.Any())
        {
            var overdue = reminders.Count(r => r.DueDate.Date < DateTime.Today);
            double score = overdue == 0 ? 90 : 60;
            categories.Add((score, 15));
        }

        // Fuel score (10% weight)
        if (fuels.Any())
        {
            var recentFuelCount = fuels.Count(f => f.FuelDate >= DateTime.Today.AddDays(-30));
            double score = recentFuelCount >= 2 ? 90 : recentFuelCount == 1 ? 70 : 50;
            categories.Add((score, 10));
        }

        var totalWeight = categories.Sum(c => c.weight);
        var overallScore = totalWeight > 0 ? Math.Round(categories.Sum(c => c.score * c.weight) / totalWeight) : 0;
        
        _vehicleHealthScore = overallScore;
        _vehicleHealthLabel = _vehicleHealthScore >= 85 ? "EXCELLENT" : _vehicleHealthScore >= 70 ? "GOOD" : _vehicleHealthScore >= 50 ? "FAIR" : "POOR";
    }


    private async void ExecuteRequest(object obj)
    {
        if (obj is not RequestEventArgs args) return;

        var requestText = args.RequestItem?.Text ?? string.Empty;
        var svc = VehicleDataService.Instance;
        var vid = SelectedVehicle?.Id ?? 0;

        // Build comprehensive vehicle context for AI
        var services = svc.GetServiceRecords(vid).OrderByDescending(s => s.ServiceDate).Take(5).ToList();
        var fuels = svc.GetFuelEntries(vid).OrderByDescending(f => f.FuelDate).Take(5).ToList();
        var reminders = svc.GetReminders(vid).OrderBy(r => r.DueDate).Take(5).ToList();

        // Format service history
        var serviceHistory = services.Any()
            ? string.Join("\n", services.Select(s => $"- {s.ServiceDate:dd MMM yyyy}: {s.ServiceType} at {s.Workshop} ({s.Amount})"))
            : "- No service records";

        // Format fuel history
        var fuelHistory = fuels.Any()
            ? string.Join("\n", fuels.Select(f => $"- {f.FuelDate:dd MMM yyyy}: {f.LitresDisplay} at {f.Station} (₹{f.TotalCost:N0})"))
            : "- No fuel records";

        // Format reminders
        var reminderStatus = reminders.Any()
            ? string.Join("\n", reminders.Select(r => {
                var status = r.DueDate.Date < DateTime.Today ? "OVERDUE" : r.DueDate.Date <= DateTime.Today.AddDays(7) ? "DUE SOON" : "UPCOMING";
                return $"- {r.Title} ({r.ReminderType}): {status} - {r.DueDate:dd MMM yyyy}";
            }))
            : "- No reminders";

        var vehicleInfo = $"""
Vehicle: {SelectedVehicle?.Make} {SelectedVehicle?.Model} ({SelectedVehicle?.Year})
Odometer: {SelectedVehicle?.OdometerReading} km
Health Score: {_vehicleHealthScore}/100 ({_vehicleHealthLabel})

Service History (Last 5):
{serviceHistory}

Fuel Log (Last 5):
{fuelHistory}

Reminders:
{reminderStatus}

User Query: {requestText}
""";

        var systemMsg =
            $"You are a vehicle care specialist for {SelectedVehicle?.Make} {SelectedVehicle?.Model}. " +
            "Analyze the vehicle data provided and give concise, practical advice about maintenance, costs, and care. " +
            "Be specific and reference actual data from the vehicle records. " +
            "Keep answers to 2-3 sentences unless a detailed breakdown is needed.";

        var aiText = await _ai.GetResultsFromAI(vehicleInfo, systemMsg);

        if (string.IsNullOrEmpty(aiText))
            aiText = $"I'm having trouble reaching the AI right now ({_ai.LastError ?? "no response"}). Please try again shortly.";

        var requestItem = args.RequestItem ?? new AssistItem
        {
            Text = requestText,
            IsRequested = true,
            DateTime = DateTime.Now
        };

        AssistItems.Add(new AssistItem
        {
            Text        = aiText,
            IsRequested = false,
            RequestItem = requestItem,
            DateTime    = DateTime.Now,
        });

        OnPropertyChanged(nameof(HasChatItems));
        RefreshConversationHistory();
        args.Handled = true;
    }

    private async void ExecuteSuggestionRequest(object obj)
    {
        if (obj is not ISuggestion suggestion) return;

        // Create a request item with the suggestion text
        var requestItem = new AssistItem 
        { 
            Text = suggestion.Text,
            IsRequested = true,
            DateTime = DateTime.Now
        };
        var requestArgs = new RequestEventArgs(requestItem);
        
        // Execute as a regular request
        ExecuteRequest(requestArgs);
    }

    private void RefreshConversationHistory()
    {
        ConversationItems.Clear();

        if (AssistItems.Count == 0)
            return;

        var list = new List<IAssistItem>(AssistItems);
        foreach (var item in list)
        {
            var title = !string.IsNullOrWhiteSpace(item.Text)
                ? item.Text.Length > 36 ? item.Text.Substring(0, 36) + "..." : item.Text
                : "AI chat";

            ConversationItems.Add(new AssistConversationItem
            {
                Title = title,
                DateTime = item.DateTime,
                AssistItems = new ObservableCollection<IAssistItem> { item }
            });
        }
    }

    public void ClearChat()
    {
        AssistItems.Clear();
        ConversationItems.Clear();
        OnPropertyChanged(nameof(HasChatItems));
        RefreshConversationHistory();
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

