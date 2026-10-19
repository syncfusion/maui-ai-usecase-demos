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
            // Deliberate: the single-page mobile layout (notes list with an
            // editor page navigated via Shell) is registered on Android only.
            // iOS / MacCatalyst / Windows keep the two-column desktop layout
            // where the editor is always visible next to the tree.
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
