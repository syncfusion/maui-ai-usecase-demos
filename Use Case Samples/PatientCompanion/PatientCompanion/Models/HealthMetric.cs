namespace PatientCompanion.Models;

public class HealthMetric
{
    public DateTime Date { get; set; }

    public int SystolicPressure { get; set; }

    public int DiastolicPressure { get; set; }

    public int HeartRate { get; set; }

    public int SpO2 { get; set; }

    public double Weight { get; set; }

    public string BloodPressureDisplay =>
        $"{SystolicPressure}/{DiastolicPressure}";
}