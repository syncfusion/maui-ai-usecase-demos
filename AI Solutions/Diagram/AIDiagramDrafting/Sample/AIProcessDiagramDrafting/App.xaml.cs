using AIDiagramDrafting.ViewModels;

namespace AIDiagramDrafting;

public partial class App : Application
{
    public App(ProcessDiagramPage processDiagramPage)
    {
        InitializeComponent();
        MainPage = processDiagramPage;
    }
}
