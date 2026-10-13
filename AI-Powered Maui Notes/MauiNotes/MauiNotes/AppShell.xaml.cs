using MauiNotes.Views.Editor;
using MauiNotes.Views.Notes;
using MauiNotes.ViewModels;

namespace MauiNotes
{
    public partial class AppShell : Shell
    {
        public static NotesViewModel SharedNotesViewModel { get; private set; } = null!;

        public AppShell(NotesViewModel notesViewModel)
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(EditorPageMobileUI), typeof(EditorPageMobileUI));
            Routing.RegisterRoute("EditorPage", typeof(EditorPageMobileUI));

            SharedNotesViewModel = notesViewModel;
            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                SharedNotesViewModel.IsFormatPopoverOpen = false;
                SharedNotesViewModel.IsTextPaletteOpen = false;
                SharedNotesViewModel.IsNodeMenuOpen = false;
                SharedNotesViewModel.SelectedFolderNode = null;
                HomeContent.ContentTemplate = new DataTemplate(() => new NotesPageMobileUI(SharedNotesViewModel));
            }
        }
    }
}
