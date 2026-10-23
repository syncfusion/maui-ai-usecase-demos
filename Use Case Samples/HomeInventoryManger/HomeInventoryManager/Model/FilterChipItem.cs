using CommunityToolkit.Mvvm.ComponentModel;

namespace HomeInventoryManager.Model
{
    /// <summary>
    /// A selectable filter chip used by the AI Advisor's
    /// <c>SfChipGroup</c>. The <see cref="Key"/> is the value passed
    /// to the ViewModel's <c>ApplyFilter</c> command while
    /// <see cref="Label"/> is what the user sees on the chip.
    /// </summary>
    public class FilterChipItem : ObservableObject
    {
        public string Key { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string DotColor { get; set; } = "#94A3B8";

        public FilterChipItem() { }

        public FilterChipItem(string key, string label, string dotColor)
        {
            Key = key;
            Label = label;
            DotColor = dotColor;
        }
    }
}
