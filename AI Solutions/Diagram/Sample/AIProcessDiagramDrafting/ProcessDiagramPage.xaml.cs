using AIProcessDiagramDrafting.ViewModels;

namespace AIProcessDiagramDrafting;

public partial class ProcessDiagramPage : ContentPage
{
    public ProcessDiagramPage(ProcessDiagramViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
