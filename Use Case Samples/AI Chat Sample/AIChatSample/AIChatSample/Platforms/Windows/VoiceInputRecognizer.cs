#if WINDOWS
using Windows.Media.SpeechRecognition;

namespace AIChatSample;

/// <summary>
/// Windows-specific voice input backed by
/// <c>Windows.Media.SpeechRecognition.SpeechRecognizer</c> (the WinRT
/// runtime API available in WinUI 3 / MAUI on Windows). Falls back to
/// the typed-prompt default when no recognizer is installed on the
/// device or the user denies microphone access.
/// </summary>
public static class VoiceInputRecognizer
{
    public static bool IsNativeRecognitionSupported => true;

    public static async Task<string?> RecognizeAsync(string? existingText, CancellationToken ct = default)
    {
        try
        {
            using var recognizer = new SpeechRecognizer();
            var constraint = new SpeechRecognitionTopicConstraint(
                SpeechRecognitionScenario.Dictation, "dictation");
            recognizer.Constraints.Add(constraint);
            await recognizer.CompileConstraintsAsync();

            // Fire a one-shot recognition so the user can dictate one
            // utterance. The UI's IsListening state is shown while we
            // wait, and the recognized text is appended to the editor.
            var session = recognizer.RecognizeAsync();
            var result = await session;
            if (result?.Status != SpeechRecognitionResultStatus.Success) return existingText;
            var recognized = result.Text;
            if (string.IsNullOrWhiteSpace(recognized)) return existingText;
            return string.IsNullOrEmpty(existingText)
                ? recognized
                : (existingText + " " + recognized).Trim();
        }
        catch
        {
            // Fallback: show the typed prompt so the feature works even
            // when native recognition fails or microphone permission is
            // denied.
            var page = Microsoft.Maui.Controls.Application.Current?.Windows
                .FirstOrDefault()?.Page;
            if (page is null) return existingText;
            var prompt = await page.DisplayPromptAsync(
                title: "Voice input",
                message: "Speak or type your message. Recognized text will be added to the editor.",
                accept: "Use",
                cancel: "Cancel",
                placeholder: "Type the message…",
                initialValue: existingText ?? string.Empty,
                keyboard: Microsoft.Maui.Keyboard.Text);
            return prompt;
        }
    }
}
#endif
