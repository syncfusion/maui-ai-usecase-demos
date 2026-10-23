using Microsoft.Maui.Graphics;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SmartVehicleCare.Models;

// ── Maintenance tab ──────────────────────────────────────────────────────────

public class ServiceRecord : INotifyPropertyChanged
{
    private DateTime _serviceDate = DateTime.Today;
    public DateTime ServiceDate
    {
        get => _serviceDate;
        set { _serviceDate = value; OnPropertyChanged(); OnPropertyChanged(nameof(Date)); }
    }
    public string Date => ServiceDate.ToString("dd MMM yyyy");

    private string _serviceType = string.Empty;
    public string ServiceType { get => _serviceType; set { _serviceType = value; OnPropertyChanged(); } }

    private string _workshop = string.Empty;
    public string Workshop { get => _workshop; set { _workshop = value; OnPropertyChanged(); } }

    private string _amount = string.Empty;
    public string Amount { get => _amount; set { _amount = value; OnPropertyChanged(); } }

    private string _mileage = string.Empty;
    public string Mileage { get => _mileage; set { _mileage = value; OnPropertyChanged(); } }

    private string _status = string.Empty;
    public string Status { get => _status; set { _status = value; OnPropertyChanged(); } }

    private Color _statusColor = Colors.Green;
    public Color StatusColor { get => _statusColor; set { _statusColor = value; OnPropertyChanged(); } }

    private Color _statusBgColor = Color.FromArgb("#DCFCE7");
    public Color StatusBgColor { get => _statusBgColor; set { _statusBgColor = value; OnPropertyChanged(); } }

    private string _notes = string.Empty;
    public string Notes { get => _notes; set { _notes = value; OnPropertyChanged(); } }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string prop = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
}

public class FuelEntry : INotifyPropertyChanged
{
    private DateTime _fuelDate = DateTime.Today;
    public DateTime FuelDate
    {
        get => _fuelDate;
        set { _fuelDate = value; OnPropertyChanged(); OnPropertyChanged(nameof(Date)); }
    }
    public string Date => FuelDate.ToString("dd MMM yyyy");

    private string _fuelType = string.Empty;
    public string FuelType { get => _fuelType; set { _fuelType = value; OnPropertyChanged(); } }

    private string _station = string.Empty;
    public string Station { get => _station; set { _station = value; OnPropertyChanged(); } }

    private double _litresFilled;
    public double LitresFilled
    {
        get => _litresFilled;
        set
        {
            _litresFilled = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(TotalCost));
            OnPropertyChanged(nameof(TotalCostDisplay));
            OnPropertyChanged(nameof(LitresDisplay));
        }
    }

    private double _costPerLitre;
    public double CostPerLitre
    {
        get => _costPerLitre;
        set
        {
            _costPerLitre = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(TotalCost));
            OnPropertyChanged(nameof(TotalCostDisplay));
            OnPropertyChanged(nameof(CostPerLitreDisplay));
        }
    }

    public double TotalCost => Math.Round(LitresFilled * CostPerLitre, 2);
    public string TotalCostDisplay => $"₹{TotalCost:N2}";
    public string LitresDisplay => $"{LitresFilled:N1} L";
    public string CostPerLitreDisplay => $"₹{CostPerLitre:N2}/L";

    private int _odometerReading;
    public int OdometerReading
    {
        get => _odometerReading;
        set { _odometerReading = value; OnPropertyChanged(); OnPropertyChanged(nameof(OdometerDisplay)); }
    }
    public string OdometerDisplay => OdometerReading > 0 ? $"{OdometerReading:N0} km" : "—";

    private bool _isFullTank = true;
    public bool IsFullTank { get => _isFullTank; set { _isFullTank = value; OnPropertyChanged(); } }

    private Color _fuelTypeColor = Color.FromArgb("#3B5BDB");
    public Color FuelTypeColor { get => _fuelTypeColor; set { _fuelTypeColor = value; OnPropertyChanged(); } }

    private Color _fuelTypeBgColor = Color.FromArgb("#EEF2FF");
    public Color FuelTypeBgColor { get => _fuelTypeBgColor; set { _fuelTypeBgColor = value; OnPropertyChanged(); } }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string prop = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
}

public class AIInsightItem
{
    public string Priority { get; set; } = string.Empty;
    public Color PriorityColor { get; set; } = Colors.Red;
    public Color PriorityBgColor { get; set; } = Color.FromArgb("#FEE2E2");
    public Color LeftAccentColor { get; set; } = Colors.Red;
    public Color CardBgColor { get; set; } = Color.FromArgb("#FFF5F5");
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconText { get; set; } = "⚠";
    public Color IconColor { get; set; } = Colors.Red;
    public Color IconBgColor { get; set; } = Color.FromArgb("#FEE2E2");
}

public class HealthMetric
{
    public string Label { get; set; } = string.Empty;
    public int Value { get; set; }
    public double ValueNormalized => Value / 100.0;
    public Color BarColor { get; set; } = Color.FromArgb("#3B5BDB");
    public bool HasWarning { get; set; }
    public string Percentage => $"{Value}%";
}

public class HealthGaugeSegment
{
    public string Category { get; set; } = string.Empty;
    public double Value { get; set; }
}

public class RadarDataPoint
{
    public string Category { get; set; } = string.Empty;
    public double Value    { get; set; }
}

public class NearbyNetworkItem
{
    public string Name          { get; set; } = string.Empty;
    public string TypeLabel     { get; set; } = string.Empty;
    public string RatingOrPrice { get; set; } = string.Empty;
    public string OpenTime      { get; set; } = string.Empty;
    public string IconText      { get; set; } = "🔧";
    public Color  IconBgColor   { get; set; } = Color.FromArgb("#1A3A5C");
    public Color  IconTextColor { get; set; } = Color.FromArgb("#8ED3FF");
    public CustomMarker? Marker   { get; set; }
}

// ── Schedule tab ─────────────────────────────────────────────────────────────

public class TimelineAppointment
{
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public Color AccentColor { get; set; } = Color.FromArgb("#EBF5FF");
    public Color BorderColor { get; set; } = Color.FromArgb("#3B5BDB");
    public string TimeSlot { get; set; } = string.Empty;
}

public class UpcomingTask
{
    public string Title { get; set; } = string.Empty;
    public string Badge { get; set; } = string.Empty;
    public Color BadgeColor { get; set; } = Colors.Red;
    public Color BadgeBgColor { get; set; } = Color.FromArgb("#FEE2E2");
    public string Description { get; set; } = string.Empty;
    public string IconText { get; set; } = "🔧";
    public Color IconBgColor { get; set; } = Color.FromArgb("#EBF5FF");
}

public class ReminderItem
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public Color AccentColor { get; set; } = Color.FromArgb("#3B5BDB");
    public string ActionIconText { get; set; } = "🛡";
}
public class ScheduleAlert
{
    public string Badge { get; set; } = string.Empty;
    public Color BadgeColor { get; set; } = Colors.Red;
    public Color BadgeBgColor { get; set; } = Colors.Transparent;
    public Color LeftAccentColor { get; set; } = Colors.Red;
    public string Date { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ActionText { get; set; } = string.Empty;
    public Color ActionColor { get; set; } = Color.FromArgb("#3B5BDB");
    public ScheduleReminder? Reminder { get; set; }
}

public class ScheduleReminder
{
    public string ReminderType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime DueDate { get; set; } = DateTime.Today;
    public TimeSpan DueTime { get; set; } = new TimeSpan(10, 0, 0);
    public string Priority { get; set; } = "Medium";
    public int RemindBeforeDays { get; set; } = 7;
    public bool IsRepeat { get; set; }
}