using MauiNotes.ViewModels;
using MauiNotes.Views.Editor;
using MauiNotes.Views.Notes;

namespace MauiNotes
{
    public partial class AppShell : Shell
    {
        private readonly IServiceProvider services;
        private readonly NotesViewModel notesViewModel;

        public AppShell(IServiceProvider services, NotesViewModel notesViewModel)
        {
            this.services = services;
            this.notesViewModel = notesViewModel;
            InitializeComponent();
            Routing.RegisterRoute(nameof(EditorPageMobileUI), typeof(EditorPageMobileUI));

            if (DeviceInfo.Idiom == DeviceIdiom.Phone)
            {
                notesViewModel.IsFormatPopoverOpen = false;
                notesViewModel.IsTextPaletteOpen = false;
                notesViewModel.IsNodeMenuOpen = false;
                notesViewModel.SelectedFolderNode = null;
                HomeContent.ContentTemplate = new DataTemplate(() =>
                    services.GetRequiredService<NotesPageMobileUI>());
            }
        }
    }
}
