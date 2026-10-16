namespace AIChatSample;

// Provide the default (non-Windows) implementation. The Windows-specific
// implementation lives under Platforms/Windows and is compiled only when
// the WINDOWS symbol is defined, so exclude this file in that case to
// avoid duplicate member definitions.
#if !WINDOWS

/// <summary>
/// Cross-platform helper that captures voice input and returns the
/// recognized text. The Windows implementation uses the WinUI speech
/// recognizer; every other platform falls back to a prompt-based stub so
/// the feature still works (and the UI can still be exercised) in
/// environments without a microphone.
/// </summary>
public static class VoiceInputRecognizer
{
    /// <summary>
    /// True when the platform's native speech-to-text is wired up. The
    /// default (non-Windows) implementation does not provide native
    /// recognition.
    /// </summary>
    public static bool IsNativeRecognitionSupported => false;

    /// <summary>
    /// Captures voice input and returns the recognized text. The optional
    /// <paramref name="existingText"/> is concatenated with the result so
    /// successive recordings append to the editor instead of replacing it.
    /// </summary>
    public static async Task<string?> RecognizeAsync(string? existingText, CancellationToken ct = default)
    {
        // Default fallback: prompt the user for the text the mic would
        // have produced. This keeps the feature working on every platform
        // and during automated UI tests where no microphone is available.
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

#endif
