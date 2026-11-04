using AIDiagramDrafting.ViewModels;

namespace AIDiagramDrafting;

public partial class ProcessDiagramPage : ContentPage
{
    public ProcessDiagramPage(ProcessDiagramViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
