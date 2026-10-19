namespace MauiNotes;

/// <summary>
/// Cross-cutting constants shared by the editor, the format popup and the
/// view model. Centralising them keeps the toolbar's "Monostyled" toggle in
/// sync with the view model's monostyled state check and avoids each callsite
/// re-declaring a font family string that may not exist on every platform.
/// </summary>
internal static class MauiNotesConstants
{
    /// <summary>
    /// Font family used for the "Monostyled" toolbar action.
    /// The CSS-standard <c>monospace</c> keyword is resolved by the engine's
    /// WebView using the platform's native monospace font (Droid Sans Mono /
    /// Roboto Mono on Android, Menlo on iOS, Consolas on Windows). It avoids
    /// the Java.Lang.RuntimeException ("Font asset not found Courier New")
    /// that the previously-used literal "Courier New" triggered on every tap
    /// inside the editor on Android.
    /// </summary>
    public const string MonospaceFontFamily = "monospace";

    /// <summary>
    /// Case-insensitive token the editor's font family is compared against to
    /// decide whether the Monostyled toolbar toggle is in the active state.
    /// The Syncfusion rich text editor reports the resolved CSS keyword back
    /// as "monospace" on every platform, so this single value works
    /// everywhere without per-platform branches.
    /// </summary>
    public const string MonospaceFontFamilyMatcher = "monospace";
}
