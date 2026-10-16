using AIChatSample.Controls;

namespace AIChatSample;

public partial class NewChat : ContentPage
{
    private bool spanUpdatePending;
    private AIMainLayoutViewModel? _layoutVm;

    public NewChat()
    {
        InitializeComponent();
        Color sfAIAssistViewBackground = Color.FromArgb("#FCF8FA");
        if (Application.Current != null)
        {
            sfAIAssistViewBackground = Application.Current.UserAppTheme == AppTheme.Dark
            ? (Color?)Application.Current.Resources["SentMessageColorDark"] ?? Color.FromArgb("#313032")
            : (Color?)Application.Current.Resources["SentMessageColorLight"] ?? Color.FromArgb("#FCF8FA");
        }


        ResourceDictionary dictionary = new ResourceDictionary();
        dictionary.Add("SfAIAssistViewTheme", "CustomTheme");
        dictionary.Add("SfAIAssistViewBackground", sfAIAssistViewBackground);
        dictionary.Add("SfAIAssistViewHeaderSuggestionBackground", Color.FromArgb("#FCF8FA"));
        this.Resources.Add(dictionary);

    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        if (Handler is null)
        {
            AIChatNavigationPanel.NavigationRequested -= OnNavigationRequested;
            if (_layoutVm is not null)
            {
                _layoutVm.ChatSessionReplaced -= OnChatSessionReplaced;
                _layoutVm = null;
            }
        }
        else
        {
            AIChatNavigationPanel.NavigationRequested -= OnNavigationRequested;
            AIChatNavigationPanel.NavigationRequested += OnNavigationRequested;

            // FR-0.4: subscribe to ChatSessionReplaced so opening a
            // recent chat from the drawer re-binds the AIAssistView to
            // the chosen session.
            _layoutVm = ServiceHelper.GetService<AIMainLayoutViewModel>();
            if (_layoutVm is not null)
            {
                _layoutVm.ChatSessionReplaced -= OnChatSessionReplaced;
                _layoutVm.ChatSessionReplaced += OnChatSessionReplaced;
            }
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // FR-0.4: respect the active session chosen from the drawer.
        // If the user navigated to this page by tapping a recent chat,
        // AIMainLayoutViewModel has already raised ChatSessionReplaced
        // and the bound view-model has been hydrated. We must NOT clear
        // it back to the welcome state in that case.
        if (BindingContext is AIChatSuggestionViewModel viewModel &&
            _layoutVm is not null && _layoutVm.CurrentSession is not null)
        {
            // Re-hydrate defensively: if the session id doesn't match
            // the view-model's current session, the new session wins.
            if (!ReferenceEquals(viewModel.CurrentSession, _layoutVm.CurrentSession))
            {
                viewModel.LoadSession(_layoutVm.CurrentSession);
            }
            return;
        }

        // First-time entry or user tapped "New chat" from the drawer
        // with no active session selected yet — reset the conversation
        // to the welcome state.
        if (BindingContext is AIChatSuggestionViewModel viewModel2)
        {
            viewModel2.ResetConversation();
        }
    }

    private void OnChatSessionReplaced(object? sender, ChatSession session)
    {
        if (BindingContext is AIChatSuggestionViewModel viewModel)
        {
            viewModel.LoadSession(session);
        }
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

    private void AndroidMenuTapped(object? sender, EventArgs e)
    {
#if ANDROID
        AIChatNavigationPanel.ToggleCompact();
#endif
    }

        private void SuggestionsLayoutSizeChanged(object? sender, EventArgs e)
        {
                if (sender is not CollectionView suggestionsLayout ||
                        suggestionsLayout.ItemsLayout is not GridItemsLayout gridLayout)
                {
                        return;
                }

                var targetSpan = suggestionsLayout.Width < 560 ? 1 : 2;
                if (gridLayout.Span == targetSpan || spanUpdatePending)
                {
                        return;
                }

                spanUpdatePending = true;
                Dispatcher.Dispatch(() =>
                {
                        spanUpdatePending = false;
                        if (suggestionsLayout.ItemsLayout is GridItemsLayout currentLayout &&
                                currentLayout.Span != targetSpan)
                        {
                                currentLayout.Span = targetSpan;
                        }
                });
        }
}