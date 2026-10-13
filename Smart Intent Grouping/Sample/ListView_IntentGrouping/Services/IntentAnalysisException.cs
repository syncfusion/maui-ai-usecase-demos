namespace ListView_IntentGrouping.Services;

/// <summary>
/// Thrown by <see cref="AzureOpenAIIntentAnalyzer"/> when the
/// Azure OpenAI request fails in a way the view-model should
/// surface to the user (auth, deployment, non-JSON response, etc.).
/// The view-model catches this exception and shows its message in
/// the status banner instead of letting a raw
/// <c>RequestFailedException</c> bubble up.
/// </summary>
public class IntentAnalysisException : Exception
{
    public IntentAnalysisException(string message) : base(message) { }
    public IntentAnalysisException(string message, Exception inner) : base(message, inner) { }
}
