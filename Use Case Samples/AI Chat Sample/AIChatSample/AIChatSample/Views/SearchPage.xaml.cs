using AIChatSample.Controls;
using Syncfusion.Maui.Toolkit.Popup;

namespace AIChatSample;

public partial class SearchPage : ContentPage
{
    private AIMainLayoutViewModel? _layoutVm;
    private RecentChatItem? _shareItem;
    private RecentChatItem? _moreItem;

    public SearchPage()
    {
        InitializeComponent();
        this.BindingContext = ServiceHelper.GetService<AIMainLayoutViewModel>() ?? new AIMainLayoutViewModel();
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        if (Handler is null)
        {
            AIChatNavigationPanel.NavigationRequested -= OnNavigationRequested;
        }
        else
        {
            AIChatNavigationPanel.NavigationRequested -= OnNavigationRequested;
            AIChatNavigationPanel.NavigationRequested += OnNavigationRequested;

            _layoutVm = ServiceHelper.GetService<AIMainLayoutViewModel>();
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
    }

    private async void OnBackRequested(object? sender, EventArgs e)
    {
        if (Navigation?.NavigationStack.Count > 1)
        {
            await Navigation.PopAsync(animated: true);
        }
    }

    private void OnNavigationRequested(object? sender, NavigationRequestEventArgs e)
    {
        ChatNavigationService.Handle(this, e.Key, e.Parameter);
    }

    private void OnNavigationMenuTapped(object? sender, EventArgs e)
    {
#if ANDROID
        AIChatNavigationPanel.ToggleCompact();
#endif
    }

    private void RecentChatItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        if (e.DataItem is RecentChatItem item)
        {
            ChatNavigationService.Handle(this, "OpenChat", item);
        }
    }

    private void OnShareTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is RecentChatItem item)
        {
            _shareItem = item;
            var link = _layoutVm?.BuildShareLink(item) ?? string.Empty;
            // Set the popup's BindingContext so the inner label can render
            // the link. The ContentTemplate uses {Binding} which
            // resolves to the popup's BindingContext.
            SharePopup.BindingContext = link;
            SharePopup.ShowRelativeToView(RecentChatsList, PopupRelativePosition.AlignToRightOf, -100, 20);
        }
    }

    private void OnShareCopyClicked(object? sender, EventArgs e)
    {
        var link = SharePopup.BindingContext as string ?? string.Empty;
        if (!string.IsNullOrEmpty(link))
        {
            try
            {
                Clipboard.SetTextAsync(link);
            }
            catch
            {
                // Clipboard can fail on some platforms; swallow so the
                // popup still closes.
            }
        }
        SharePopup.IsOpen = false;
    }

    private void OnShareCancelClicked(object? sender, EventArgs e)
    {
        SharePopup.IsOpen = false;
    }

    private void OnMoreTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is RecentChatItem item)
        {
            _moreItem = item;
            MorePopup.BindingContext = item;
            MorePopup.ShowRelativeToView(RecentChatsList, PopupRelativePosition.AlignToRightOf, -100, 20);
        }
    }

    private void OnMoreOpenClicked(object? sender, EventArgs e)
    {
        MorePopup.IsOpen = false;
        if (_moreItem is not null)
        {
            ChatNavigationService.Handle(this, "OpenChat", _moreItem);
            _moreItem = null;
        }
    }

    private async void OnMoreDeleteClicked(object? sender, EventArgs e)
    {
        MorePopup.IsOpen = false;
        var item = _moreItem;
        _moreItem = null;
        if (item is null || _layoutVm is null) return;

        var confirmed = await DisplayAlertAsync("Delete chat",
            $"Delete \"{item.Title}\"? This cannot be undone.",
            "Delete", "Cancel");
        if (!confirmed) return;

        _layoutVm.DeleteRecentChat(item);
    }
}
