namespace AIProcessDiagramDrafting;

public partial class ProcessDiagramPage : ContentPage
{
    public ProcessDiagramPage(ViewModels.ProcessDiagramViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
