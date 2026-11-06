using Microsoft.Maui.Graphics;

namespace HomeInventoryManager.Model
{
    /// <summary>
    /// A single slice of the "Replacement Forecast" doughnut chart on
    /// the AI Advisor Lifecycle Analytics section.
    /// </summary>
    public class ForecastSlice
    {
        public string Category { get; set; } = string.Empty; // "Healthy" | "Monitor" | "Attention" | "High Risk"
        public int Count { get; set; }
        public Color SliceColor { get; set; } = Colors.Transparent;
    }
}
