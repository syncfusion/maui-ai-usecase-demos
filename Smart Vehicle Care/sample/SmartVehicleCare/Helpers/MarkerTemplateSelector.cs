using SmartVehicleCare.Models;

namespace SmartVehicleCare.Helpers;

public class MarkerTemplateSelector : DataTemplateSelector
{
    public DataTemplate? NormalTemplate  { get; set; }
    public DataTemplate? DetailTemplate  { get; set; }
    public DataTemplate? OfflineTemplate { get; set; }

    protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
    {
        if (item is not CustomMarker marker) return NormalTemplate;
        // No AI image → use offline text-only template
        if (marker.Image == null)
            return OfflineTemplate ?? NormalTemplate;
        // AI image available → select by whether an address is present
        return string.IsNullOrEmpty(marker.Address) ? NormalTemplate : DetailTemplate;
    }
}
