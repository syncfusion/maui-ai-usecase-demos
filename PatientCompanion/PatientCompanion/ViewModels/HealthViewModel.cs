using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PatientCompanion.Models;
using System.Collections.ObjectModel;

namespace PatientCompanion.ViewModels;

public partial class HealthViewModel : BaseViewModel
{
    private readonly Random random = new();

    [ObservableProperty]
    private bool showBloodPressureChart = true;

    [ObservableProperty]
    private bool showHeartRateChart;

    [ObservableProperty]
    private bool showSpo2Chart;

    [ObservableProperty]
    private int selectedRangeIndex;

    public ObservableCollection<HealthMetricCardViewModel> HealthMetrics { get; } = new();

    public ObservableCollection<BloodPressurePoint> CurrentBloodPressureData { get; } = new();

    public ObservableCollection<HealthChartPoint> CurrentChartData { get; } = new();

    private readonly List<BloodPressurePoint> bp90Days = new();

    private readonly List<HealthChartPoint> heartRate90Days = new();

    private readonly List<HealthChartPoint> spo290Days = new();

    [ObservableProperty]
    private HealthMetricCardViewModel? selectedMetricCard;

    [ObservableProperty]
    private string lastUpdatedText = string.Empty;

    public HealthViewModel()
    {
        Title = "My Health";

        UpdateLastUpdatedTime();

        LoadMetricCards();

        GenerateData();

        SelectedRangeIndex = 0;

        SelectBloodPressure();
    }

    private void UpdateLastUpdatedTime()
    {
        var updatedTime = DateTime.Now.AddHours(-6);

        LastUpdatedText =
            $"Updated today at {updatedTime:h:mm tt}";
    }

    partial void OnSelectedRangeIndexChanged(int value)
    {
        System.Diagnostics.Debug.WriteLine($"Range Changed: {value}");

        RefreshChart();
    }

    private void LoadMetricCards()
    {
        HealthMetrics.Add(new HealthMetricCardViewModel
        {
            Title = "BP",
            ChartTitle = "Blood Pressure",
            DisplayValue = "120 / 80",
            Unit = "mmHg",
            Icon = MaterialIcons.Favorite,
            IsSelected = true,
            SelectCommand = SelectBloodPressureCommand
        });

        HealthMetrics.Add(new HealthMetricCardViewModel
        {
            Title = "Heart Rate",
            ChartTitle = "Heart Rate",
            DisplayValue = "72",
            Unit = "bpm",
            Icon = MaterialIcons.TrendingUp,
            SelectCommand = SelectHeartRateCommand
        });

        HealthMetrics.Add(new HealthMetricCardViewModel
        {
            Title = "SpO₂",
            ChartTitle = "Blood Oxygen Saturation",
            DisplayValue = "98",
            Unit = "%",
            Icon = MaterialIcons.Air,
            SelectCommand = SelectSpo2Command
        });
    }

    private void GenerateData()
    {
        var startDate = DateTime.Today.AddDays(-89);

        for (int i = 0; i < 90; i++)
        {
            var date = startDate.AddDays(i);

            bp90Days.Add(new BloodPressurePoint
            {
                Date = date,
                Systolic = random.Next(105, 125),
                Diastolic = random.Next(75, 95)
            });

            heartRate90Days.Add(new HealthChartPoint
            {
                Date = date,
                Value = random.Next(65, 85)
            });

            spo290Days.Add(new HealthChartPoint
            {
                Date = date,
                Value = random.Next(96, 100)
            });
        }
    }

    [RelayCommand]
    private void SelectBloodPressure()
    {
        SelectCard("BP");

        ShowBloodPressureChart = true;
        ShowHeartRateChart = false;
        ShowSpo2Chart = false;

        RefreshChart();
    }

    [RelayCommand]
    private void SelectHeartRate()
    {
        SelectCard("Heart Rate");

        ShowBloodPressureChart = false;
        ShowHeartRateChart = true;
        ShowSpo2Chart = false;

        RefreshChart();
    }

    [RelayCommand]
    private void SelectSpo2()
    {
        SelectCard("SpO₂");

        ShowBloodPressureChart = false;
        ShowHeartRateChart = false;
        ShowSpo2Chart = true;

        RefreshChart();
    }

    private void SelectCard(string title)
    {
        foreach (var item in HealthMetrics)
        {
            item.IsSelected = item.Title == title;

            if (item.IsSelected)
            {
                SelectedMetricCard = item;
            }
        }
    }

    private void RefreshChart()
    {
        CurrentBloodPressureData.Clear();
        CurrentChartData.Clear();

        int days = SelectedRangeIndex switch
        {
            0 => 7,
            1 => 30,
            _ => 90
        };

        if (ShowBloodPressureChart)
        {
            foreach (var item in bp90Days.TakeLast(days))
            {
                CurrentBloodPressureData.Add(item);
            }
        }
        else if (ShowHeartRateChart)
        {
            foreach (var item in heartRate90Days.TakeLast(days))
            {
                CurrentChartData.Add(item);
            }
        }
        else if (ShowSpo2Chart)
        {
            foreach (var item in spo290Days.TakeLast(days))
            {
                CurrentChartData.Add(item);
            }
        }

        System.Diagnostics.Debug.WriteLine(
    $"BP Count = {CurrentBloodPressureData.Count}");
    }
}