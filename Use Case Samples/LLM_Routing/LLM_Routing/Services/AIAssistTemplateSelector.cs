using LLM_Routing.Models;
using Syncfusion.Maui.AIAssistView;

namespace LLM_Routing.Services;

/// <summary>
/// UI-only: picks the response bubble template for AIAssistResponseItem; falls back to the default for everything else.
/// </summary>
public sealed class AIAssistTemplateSelector : ResponseItemTemplateSelector
{
    public DataTemplate? ResponseTemplate { get; set; }

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        if (item is AIAssistResponseItem && ResponseTemplate != null)
            return ResponseTemplate;

        return base.OnSelectTemplate(item, container);
    }
}
