using MauiNotes.Models;
using MauiNotes.Controls.Popups;
using MauiNotes.ViewModels;
using MauiNotes.Views.Editor;
using Syncfusion.TreeView.Engine;
using System.Collections.Specialized;

namespace MauiNotes.Views.Notes;

public partial class NotesPageMobileUI : ContentPage
{
    public NotesPageMobileUI(NotesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        NodeActionPopups.IsMobilePresentation = true;
        viewModel.NoteSelected += OnNoteSelected;
        viewModel.TreeNodes.CollectionChanged += OnTreeNodesCollectionChanged;
    }

    private void OnTreeNodesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action != NotifyCollectionChangedAction.Add ||
            e.NewItems?.OfType<NoteTreeNode>().FirstOrDefault() is not { } addedFolder)
        {
            return;
        }

        Dispatcher.Dispatch(() =>
            NotesTreeView.BringIntoView(
                addedFolder,
                disableAnimation: true,
                canExpand: true,
                scrollToPosition: ScrollToPosition.MakeVisible));
    }

    private async void OnNoteSelected(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(EditorPageMobileUI));
    }

    private void OnNodeMenuClicked(object? sender, EventArgs e)
    {
        if (sender is View anchor && anchor.BindingContext is NoteTreeNode node)
            NodeActionPopups.ShowFor(anchor, node);
    }

    private void OnFolderRowTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not Element { BindingContext: NoteTreeNode { IsFolder: true } folder })
            return;

        TreeViewNode? treeNode = FindTreeViewNode(NotesTreeView.Nodes, folder);
        if (treeNode is null || !treeNode.HasChildNodes)
            return;

        if (treeNode.IsExpanded)
            NotesTreeView.CollapseNode(treeNode);
        else
            NotesTreeView.ExpandNode(treeNode);
    }

    private static TreeViewNode? FindTreeViewNode(
        IEnumerable<TreeViewNode> nodes,
        NoteTreeNode folder)
    {
        foreach (TreeViewNode node in nodes)
        {
            if (ReferenceEquals(node.Content, folder))
                return node;

            TreeViewNode? child = FindTreeViewNode(node.ChildNodes, folder);
            if (child is not null)
                return child;
        }

        return null;
    }
}