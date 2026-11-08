using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using SupportReplyAIDraft.Services;
using Syncfusion.Maui.Chat;

namespace SupportReplyAIDraft.ViewModels;

public sealed partial class SupportChatViewModel : ObservableObject
{
    private readonly ConversationService _conversationService;

    public SupportChatViewModel(ConversationService conversationService)
    {
        _conversationService = conversationService;

        CurrentUser = _conversationService.SupportAuthor;
        Messages = _conversationService.CreateConversation();

        SelectMessageCommand = new Command<TextMessage>(SelectMessage);
        DraftReplyCommand = new Command<TextMessage>(DraftReply, CanActivateDraftReply);
        NativeSendCommand = new Command<object>(_ => { }, _ => false);
    }

    public ObservableCollection<object> Messages { get; }
    public Author CurrentUser { get; }

    [ObservableProperty]
    private TextMessage? selectedMessage;

    public ICommand SelectMessageCommand { get; }
    public ICommand DraftReplyCommand { get; }
    public ICommand NativeSendCommand { get; }

    public bool CanDraftReply => SelectedMessage is not null;

    public string ComposerPlaceholder => "Select a customer message to begin.";

    partial void OnSelectedMessageChanged(TextMessage? value)
    {
        OnPropertyChanged(nameof(CanDraftReply));
        (DraftReplyCommand as Command)?.ChangeCanExecute();
    }

    private void SelectMessage(TextMessage? message)
    {
        if (message is null || !_conversationService.IsCustomerMessage(message))
        {
            return;
        }

        SelectedMessage = message;
    }

    private bool CanActivateDraftReply(TextMessage? message)
    {
        return SelectedMessage is not null
            && message is not null
            && ReferenceEquals(message, SelectedMessage)
            && _conversationService.IsCustomerMessage(message);
    }

    private void DraftReply(TextMessage? message)
    {
        if (!CanActivateDraftReply(message))
        {
            return;
        }

        var diagnostic = $"Draft Reply activated for: {message!.Text}";
        Debug.WriteLine(diagnostic);
        Console.WriteLine(diagnostic);
    }
}
