using Microsoft.Maui.Controls;
using Syncfusion.Maui.ImageEditor;
using SmartCropping.ViewModel;

namespace SmartCropping;

/// <summary>
/// Mobile (Android/iOS) Smart Cropping page. Vertical action layout and
/// 2-column action grid. Shares the view model, AI services, overlay and
/// apply-crop logic with the desktop view through
/// <see cref="SmartCroppingViewBase"/>.
/// </summary>
public partial class SmartCroppingMobileView : ContentPage
{
    private readonly SmartCroppingViewModel viewModel;
    private SmartCroppingViewBase? bridge;

    public SmartCroppingMobileView(SmartCroppingViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        if (bridge is null && imageEditor.Handler is not null)
        {
            bridge = new SmartCroppingViewBase(imageEditor, viewModel);
        }
    }
}