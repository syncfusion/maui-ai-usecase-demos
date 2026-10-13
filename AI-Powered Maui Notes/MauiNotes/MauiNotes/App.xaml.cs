using MauiNotes.ViewModels;

namespace MauiNotes
{
    public partial class App : Application
    {
        private readonly NotesViewModel notesViewModel;

        public App(NotesViewModel notesViewModel)
        {
            this.notesViewModel = notesViewModel;
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell(notesViewModel));
        }
    }
}