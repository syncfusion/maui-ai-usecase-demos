using MauiNotes.Views.Editor;
using MauiNotes.Views.Notes;
using MauiNotes.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace MauiNotes
{
    public partial class AppShell : Shell
    {
        public AppShell(IServiceProvider services, NotesViewModel notesViewModel)
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(EditorPageMobileUI), typeof(EditorPageMobileUI));

            if (DeviceInfo.Idiom == DeviceIdiom.Phone)
            {
                notesViewModel.IsFormatPopoverOpen = false;
                notesViewModel.IsTextPaletteOpen = false;
                notesViewModel.IsNodeMenuOpen = false;
                notesViewModel.SelectedFolderNode = null;
                HomeContent.ContentTemplate = new DataTemplate(
                    () => services.GetRequiredService<NotesPageMobileUI>());
            }
            else
            {
                HomeContent.ContentTemplate = new DataTemplate(
                    () => services.GetRequiredService<NotesPageDesktopUI>());
            }
        }
    }
}
