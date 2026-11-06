using System.Collections.ObjectModel;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;

namespace AIChatSample;

/// <summary>
/// View-model for the Library / Knowledge Hub page. Owns the chat and
/// agent entries the page renders, the chip-group filter, and the
/// search/filter/open/edit/delete commands the list uses.
/// </summary>
public partial class LibraryViewModel : ObservableObject
{
    /// <summary>Current search-text input.</summary>
    [ObservableProperty] private string? searchText;

    /// <summary>True while the page is loading data.</summary>
    [ObservableProperty] private bool isLoading;

    /// <summary>Currently selected filter chip.</summary>
    [ObservableProperty] private LibraryFilterTab? selectedFilterTab;

    // Two-way bound to SfListView.SelectedItem.
    // Tapping a row updates this; the partial method below reacts.
    /// <summary>Currently selected library row (chat or agent).</summary>
    [ObservableProperty] private LibraryItemModel? selectedLibraryItem;

    /// <summary>Full set of library rows rendered by the page.</summary>
    public ObservableCollection<LibraryItemModel> LibraryItems { get; } = new();

    /// <summary>Filtered subset of <see cref="LibraryItems"/> shown after search / filter changes.</summary>
    public ObservableCollection<LibraryItemModel> FilteredItems { get; } = new();

    /// <summary>Chip group filter values ("All", "Chats", "Agents").</summary>
    public ObservableCollection<LibraryFilterTab> Filters { get; } = new();

    /// <summary>Command that runs the search-text filter.</summary>
    public ICommand SearchCommand { get; } = new Command(_ => { });

    /// <summary>Command raised when a different filter chip is selected.</summary>
    public ICommand FilterChangedCommand { get; } = new Command(_ => { });

    /// <summary>Command that opens the filter popover (if the chip group is collapsed).</summary>
    public ICommand OpenFilterCommand { get; } = new Command(() => { });

    /// <summary>Command that opens the tapped library row.</summary>
    public ICommand OpenItemCommand { get; } = new Command(_ => { });

    /// <summary>Command that edits the tapped library row.</summary>
    public ICommand EditItemCommand { get; } = new Command(_ => { });

    /// <summary>Command that deletes the tapped library row.</summary>
    public ICommand DeleteItemCommand { get; } = new Command(_ => { });

    /// <summary>Command that shares the tapped library row.</summary>
    public ICommand ShareItemCommand { get; } = new Command(_ => { });

    /// <summary>Command that opens the per-row "more" menu.</summary>
    public ICommand OpenMoreMenuCommand { get; } = new Command(_ => { });

    /// <summary>
    /// Builds the demo data set and wires the filter chip group.
    /// </summary>
    public LibraryViewModel()
    {
        Filters.Add(new LibraryFilterTab { Text = "All", Filter = LibraryFilter.All });
        Filters.Add(new LibraryFilterTab { Text = "Chats", Filter = LibraryFilter.Chats });
        Filters.Add(new LibraryFilterTab { Text = "Agents", Filter = LibraryFilter.Agents });
        SelectedFilterTab = Filters[0];

        // Demo seed data so the page has something to render before the
        // user creates their own chats / agents. Real apps would back
        // this with the in-memory chat + agent stores.
        LibraryItems.Add(new LibraryItemModel
        {
            Id = "1",
            Title = "Banner images references for website home page",
            CreatedDate = "Mon",
            Image = Application.Current?.RequestedTheme != AppTheme.Dark ? "messageicon.png" : "messageicondark.png",
            ModifiedDate = "Mon",
            Type = LibraryItemType.Chat,
            IconGlyph = "\ue73b",
            IconFontFamily = "UIKitIcons",
            CanShare = true,
            CanEdit = true
        });
        LibraryItems.Add(new LibraryItemModel
        {
            Id = "2",
            Title = "Banner images",
            CreatedDate = "Mon",
            ModifiedDate = "Mon",
            Image = Application.Current?.RequestedTheme != AppTheme.Dark ? "messageicon.png" : "messageicondark.png",
            Type = LibraryItemType.Chat,
            IconGlyph = "\ue73b",
            IconFontFamily = "UIKitIcons",
            CanShare = true,
            CanEdit = true
        });
        LibraryItems.Add(new LibraryItemModel
        {
            Id = "3",
            Title = "Data Analyst",
            CreatedDate = "Mon",
            ModifiedDate = "Mon",
            Image = Application.Current?.RequestedTheme != AppTheme.Dark ? "trendicon.png" : "trendicondark.png",
            Type = LibraryItemType.Agent,
            IconGlyph = "\ue736",
            IconFontFamily = "MaterialAssets",
            CanShare = false,
            CanEdit = false
        });
        LibraryItems.Add(new LibraryItemModel
        {
            Id = "4",
            Title = "Idea Coach",
            CreatedDate = "Mon",
            ModifiedDate = "Mon",
            Image = Application.Current?.RequestedTheme != AppTheme.Dark ? "bulbicon.png" : "bulbicondark.png",
            Type = LibraryItemType.Agent,
            IconGlyph = "\ue736",
            IconFontFamily = "MaterialAssets",
            CanShare = false,
            CanEdit = false
        });
    }
}
