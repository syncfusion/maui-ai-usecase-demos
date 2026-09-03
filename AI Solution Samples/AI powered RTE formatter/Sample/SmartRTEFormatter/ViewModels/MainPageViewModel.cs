using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SmartRTEFormatter.Services;

namespace SmartRTEFormatter.ViewModels;

public sealed class MainPageViewModel : INotifyPropertyChanged
{
    private readonly IAIFormattingService formattingService;
    private string inputText = string.Empty;
    private string formattedHtml = string.Empty;
    private string rawOutput = string.Empty;
    private bool isFormatting;
    private string errorMessage = string.Empty;

    public MainPageViewModel(IAIFormattingService formattingService)
    {
        this.formattingService = formattingService;
        FormatCommand = new Command(
            async () => await FormatAsync(),
            () => !IsFormatting && !string.IsNullOrWhiteSpace(InputText));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string InputText
    {
        get => inputText;
        set
        {
            if (SetProperty(ref inputText, value))
            {
                ((Command)FormatCommand).ChangeCanExecute();
            }
        }
    }

    public string FormattedHtml
    {
        get => formattedHtml;
        private set => SetProperty(ref formattedHtml, value);
    }

    public string RawOutput
    {
        get => rawOutput;
        private set => SetProperty(ref rawOutput, value);
    }

    public bool IsFormatting
    {
        get => isFormatting;
        private set
        {
            if (SetProperty(ref isFormatting, value))
            {
                OnPropertyChanged(nameof(IsFormatButtonVisible));
                ((Command)FormatCommand).ChangeCanExecute();
            }
        }
    }

    public bool IsFormatButtonVisible => !IsFormatting;

    public string ErrorMessage
    {
        get => errorMessage;
        private set => SetProperty(ref errorMessage, value);
    }

    public ICommand FormatCommand { get; }

    private async Task FormatAsync()
    {
        var input = InputText?.Trim();
        if (string.IsNullOrWhiteSpace(input))
        {
            return;
        }

        IsFormatting = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await formattingService.FormatAsync(input);
            FormattedHtml = result.HtmlContent;
            RawOutput = result.StructuredJson;

            // Refresh the binding when the same content is formatted again.
            OnPropertyChanged(nameof(FormattedHtml));
            OnPropertyChanged(nameof(RawOutput));
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
        }
        finally
        {
            IsFormatting = false;
        }
    }

    private bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(storage, value))
        {
            return false;
        }

        storage = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged(string? propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}