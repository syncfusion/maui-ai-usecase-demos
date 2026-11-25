using MauiNotes.ViewModels;

namespace MauiNotes;

public partial class AppShell : Shell
{
    public AppShell(NotesViewModel notesViewModel)
    {
        InitializeComponent();
        BindingContext = notesViewModel;

        // Phone uses the notes list and pushes the editor. Tablet and desktop
        // keep the two-column layout, including iOS tablets and narrow windows.
        if (DeviceInfo.Idiom == DeviceIdiom.Phone)
        {
            notesViewModel.IsFormatPopoverOpen = false;
            notesViewModel.IsTextPaletteOpen = false;
            notesViewModel.IsNodeMenuOpen = false;
            notesViewModel.SelectedFolderNode = null;
            HomeContent.ContentTemplate = new DataTemplate(typeof(Views.Notes.NotesPageMobileUI));
        }
    }
}
