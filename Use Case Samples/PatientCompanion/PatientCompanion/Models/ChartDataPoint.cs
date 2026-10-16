using System;
using System.Collections.Generic;
using System.Text;

namespace PatientCompanion.Models;

public class ChartDataPoint
{
    public string Label { get; set; } = string.Empty;

    public double Value { get; set; }
}