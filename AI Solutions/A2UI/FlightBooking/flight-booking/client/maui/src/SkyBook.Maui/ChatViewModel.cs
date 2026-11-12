using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Syncfusion.Maui.Chat;

namespace SkyBook.Maui;

/// <summary>Chat state for <see cref="SfChat"/>: messages, the two authors and the typing indicator.</summary>
public sealed class ChatViewModel : INotifyPropertyChanged
{
  public ChatViewModel()
  {
    CurrentUser = new Author { Name = "You" };
    Bot = new Author { Name = "SkyBook Assistant", Avatar = "skybook_ai.png" }; // AI icon (Resources/Images)
    TypingIndicator = new ChatTypingIndicator { Authors = [Bot], Text = "SkyBook is typing..." };
  }

  public ObservableCollection<object> Messages { get; } = [];
  public Author CurrentUser { get; }
  public Author Bot { get; }
  public ChatTypingIndicator TypingIndicator { get; }

  private bool _showTyping;
  public bool ShowTypingIndicator { get => _showTyping; private set { _showTyping = value; OnChanged(); } }

  public void AddUserMessage(string text) =>
      Messages.Add(new TextMessage { Author = CurrentUser, Text = text, DateTime = DateTime.Now });

  public void AddBotMessage(string text) =>
      Messages.Add(new TextMessage { Author = Bot, Text = text, DateTime = DateTime.Now });

  public void ShowTyping() => ShowTypingIndicator = true;
  public void HideTyping() => ShowTypingIndicator = false;
  public void Clear() { Messages.Clear(); ShowTypingIndicator = false; }

  public event PropertyChangedEventHandler? PropertyChanged;
  private void OnChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}
