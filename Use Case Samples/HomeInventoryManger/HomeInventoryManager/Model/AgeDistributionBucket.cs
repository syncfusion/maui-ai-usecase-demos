namespace HomeInventoryManager.Model
{
    /// <summary>
    /// Represents a single bucket in the "Age Distribution" bar chart
    /// shown on the AI Advisor Lifecycle Analytics section.
    /// </summary>
    public class AgeDistributionBucket
    {
        public string Bucket { get; set; } = string.Empty;   // "0-2y", "3-5y", "6-8y", "9-11y", "12y+"
        public int Count { get; set; }
    }
}
