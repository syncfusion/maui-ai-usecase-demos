using System.ComponentModel;
using System.Runtime.CompilerServices;
using AIRatingFeedback.Services;

namespace AIRatingFeedback.ViewModels;

/// <summary>
/// ViewModel for the AI feedback-rating page. Owns the AI request lifecycle
/// (debounce, cancellation, request-version tracking) and exposes bindable
/// state for the view.
/// </summary>
public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly OpenAIService _openAIService;
    private CancellationTokenSource? _debounceCts;
    private int _requestVersion;

    private double _rating;
    private bool _isAnalyzing;
    private string? _errorMessage;

    public MainViewModel(OpenAIService openAIService)
    {
        _openAIService = openAIService;
    }

    /// <summary>
    /// Current suggested rating, between 0 and 5. Bound to <c>SfRating.Value</c>.
    /// </summary>
    public double Rating
    {
        get => _rating;
        private set => SetField(ref _rating, value);
    }

    /// <summary>
    /// <c>true</c> while an AI request is in flight (after debounce, before
    /// completion or cancellation).
    /// </summary>
    public bool IsAnalyzing
    {
        get => _isAnalyzing;
        private set => SetField(ref _isAnalyzing, value);
    }

    /// <summary>
    /// Last error to surface in the UI. Cleared when the user edits feedback.
    /// </summary>
    public string? ErrorMessage
    {
        get => _errorMessage;
        private set => SetField(ref _errorMessage, value);
    }

    /// <summary>
    /// Called when the editor text changes. Cancels any in-flight request,
    /// resets the visible rating, and (re)starts a debounced AI evaluation.
    /// </summary>
    public void OnFeedbackChanged(string? text)
    {
        // Only cancel the in-flight request here. The CancellationTokenSource is
        // disposed inside EvaluateAsync's finally block, where the token is no
        // longer being awaited. Disposing it here while EvaluateAsync is still
        // awaiting the token causes ObjectDisposedException on Backspace/Delete.
        _debounceCts?.Cancel();

        Rating = 0;
        IsAnalyzing = false;
        ErrorMessage = null;

        var trimmed = text?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
        {
            return;
        }

        var cts = new CancellationTokenSource();
        _debounceCts = cts;

        _ = EvaluateAsync(trimmed, cts);
    }

    private async Task EvaluateAsync(string feedback, CancellationTokenSource cts)
    {
        // Stamp this request with a version so a slow response cannot overwrite
        // a newer one. Combined with the per-keystroke CancellationTokenSource,
        // this gives both an immediate cancel (cancellation) and a fallback
        // (version check) for responses that arrive after a newer request started.
        int requestVersion = Interlocked.Increment(ref _requestVersion);
        CancellationToken cancellationToken = cts.Token;

        try
        {
            await Task.Delay(500, cancellationToken);

            if (requestVersion != Volatile.Read(ref _requestVersion) || cancellationToken.IsCancellationRequested)
            {
                return;
            }

            IsAnalyzing = true;
            ErrorMessage = null;

            double rating = await _openAIService.GetSuggestedRatingAsync(feedback, cancellationToken);

            // A newer request started while we were awaiting. Discard this result.
            if (requestVersion != Volatile.Read(ref _requestVersion) || cancellationToken.IsCancellationRequested)
            {
                return;
            }

            Rating = rating;
        }
        catch (OperationCanceledException)
        {
            // The user typed again — the newer request will drive the UI.
        }
        catch (Exception ex)
        {
            if (requestVersion == Volatile.Read(ref _requestVersion))
            {
                ErrorMessage = ex.Message;
                Rating = 0;
            }
        }
        finally
        {
            if (requestVersion == Volatile.Read(ref _requestVersion))
            {
                IsAnalyzing = false;
                // This run is the current one and has finished using the token.
                // Clear the field first so a concurrent OnFeedbackChanged that
                // already assigned a new CTS isn't overwritten, then dispose.
                _debounceCts = null;
                cts.Dispose();
            }
            else
            {
                // A newer request has already replaced _debounceCts. Dispose
                // our own (now-cancelled) source so it isn't leaked.
                cts.Dispose();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}