



using AIChatSample;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;

namespace AIChatSample.Controls;

/// <summary>
/// Centralized mapping from a navigation key produced by
/// <see cref="AIChatNavigationPanel"/> to a concrete page instance that
/// gets pushed onto the current <see cref="NavigationPage"/>.
/// </summary>
internal static class ChatNavigationService
{
    /// <summary>
    /// Handles a <see cref="AIChatNavigationPanel.NavigationRequested"/>
    /// event by pushing the matching page on the supplied page's
    /// navigation stack. Wrapped in try/catch so an unobserved exception
    /// from a partial navigation failure does not tear down the app
    /// (this method is invoked from a synchronous event handler).
    /// </summary>
    public static async void Handle(Page currentPage, string key, object? parameter = null)
    {
        try
        {
            await HandleAsync(currentPage, key, parameter).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Navigation failed for key '{key}': {ex.Message}");
        }
    }

    /// <summary>
    /// Awaitable core of <see cref="Handle"/>. Tests / callers can
    /// await this directly to surface failures.
    /// </summary>
    public static async Task HandleAsync(Page currentPage, string key, object? parameter = null)
    {
        if (currentPage?.Navigation?.NavigationStack is null)
        {
            return;
        }

        switch (key)
        {
            case "NewChat":
                // "New chat" should always take the user back to a fresh
                // root, regardless of how deep the stack is, and reset the
                // AIAssistView so the previous conversation isn't shown.
                if (currentPage.Navigation.NavigationStack.Count > 1)
                {
                    await currentPage.Navigation.PopToRootAsync(animated: true);
                }

                // Route through the shared shell view-model so a
                // fresh ChatSession is created and ChatSessionReplaced
                // fires. The host page then re-binds the AIAssistView
                // via AIChatSuggestionViewModel.LoadSession.
                if (ServiceHelper.GetService<AIMainLayoutViewModel>() is { } newChatLayout)
                {
                    newChatLayout.NewChatCommand.Execute(null);
                }
                else if (currentPage.Navigation.NavigationStack.LastOrDefault() is NewChat newChatPage &&
                         newChatPage.BindingContext is AIChatSuggestionViewModel viewModel)
                {
                    viewModel.ResetConversation();
                }
                break;

            case "OpenChat":
                // Tap a recent chat in the drawer pops back
                // to the root NewChat page and binds the chosen session
                // into the AIAssistView via AIMainLayoutViewModel.
                if (currentPage.Navigation.NavigationStack.Count > 1)
                {
                    await currentPage.Navigation.PopToRootAsync(animated: true);
                }

                if (parameter is RecentChatItem recentItem &&
                    ServiceHelper.GetService<AIMainLayoutViewModel>() is { } layoutVm)
                {
                    layoutVm.OpenChatCommand.Execute(recentItem);
                }
                break;

            case "OpenAgentChat":
                // Open a new chat session bound to the tapped agent
                // and carry the agent's Behaviour into the first response.
                if (currentPage.Navigation.NavigationStack.Count > 1)
                {
                    await currentPage.Navigation.PopToRootAsync(animated: true);
                }

                if (parameter is AgentItem agentItem &&
                    ServiceHelper.GetService<AIMainLayoutViewModel>() is { } agentLayoutVm)
                {
                    agentLayoutVm.OpenAgentChatCommand.Execute(agentItem);
                }
                break;

            case "Profile":
                await PushIfNotCurrent(currentPage, typeof(ProfilePage));
                break;

            case "Library":
                await PushIfNotCurrent(currentPage, typeof(LibraryPage));
                break;

            case "Search":
                await PushIfNotCurrent(currentPage, typeof(SearchPage));
                break;

            case "NewAgent":
                // "New agent" opens the Copilot-style prompt page where the
                // user describes the specialist they want. After the AI
                // produces a draft, the page navigates to AgentConfigurePage
                // via the "CreateAgent" key below.
                await PushIfNotCurrent(currentPage, typeof(CreateNewAgentPage));
                break;

            case "CreateAgent":
                // Used both for template cards (from the AIAssistView
                // suggestion rows on CreateNewAgentPage) and for the AI
                // draft that the prompt-driven Copilot flow hands back.
                // Push the configure page pre-filled with whichever input
                // arrived as the navigation parameter.
                AgentConfigurePage? configurePage = null;
                if (currentPage.Handler?.MauiContext?.Services is IServiceProvider services &&
                    services.GetService<AgentConfigurePage>() is { } resolved)
                {
                    configurePage = resolved;
                }
                configurePage ??= new AgentConfigurePage();

                if (parameter is TemplateCardSuggestion template &&
                    configurePage.BindingContext is AgentConfigurePageViewModel vm)
                {
                    vm.AgentVM.InitializeFromTemplate(template);
                }
                else if (parameter is AgentDraft draft &&
                    configurePage.BindingContext is AgentConfigurePageViewModel draftVm)
                {
                    // Copilot prompt-driven flow: AI produced a draft that
                    // we hand off as the form seed.
                    draftVm.InitializeFromDraft(draft);
                }

                await currentPage.Navigation.PushAsync(configurePage, animated: true);
                break;
        }
    }

    private static async Task PushIfNotCurrent(Page currentPage, Type pageType)
    {
        if (currentPage.Navigation.NavigationStack.LastOrDefault() is Page top &&
            top.GetType() == pageType)
        {
            return;
        }

        if (Activator.CreateInstance(pageType) is Page page)
        {
            await currentPage.Navigation.PushAsync(page, animated: true);
        }
    }
}
