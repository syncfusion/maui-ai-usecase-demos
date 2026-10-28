using System.Diagnostics;
using Microsoft.Maui.Controls;
using Syncfusion.Maui.ImageEditor;
using SmartCropping.ViewModel;

namespace SmartCropping;

/// <summary>
/// Desktop (Stitch-style) Smart Cropping page. Owns the <c>SfImageEditor</c>
/// and uses <see cref="SmartCroppingViewBase"/> to bridge its events to
/// the shared view model.
/// </summary>
public partial class SmartCropping : ContentPage
{
    private readonly SmartCroppingViewModel viewModel;
    private SmartCroppingViewBase? bridge;

    public SmartCropping(SmartCroppingViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        if (bridge is null && ImageEditor.Handler is not null)
        {
            bridge = new SmartCroppingViewBase(ImageEditor, viewModel);
        }
    }

    // The view model uses IImageEditorController through the bridge, so
    // these members are not needed externally on the page itself; the
    // bridge already implements the controller surface. They are kept
    // implicit by virtue of the bridge living inside the bridge.
}