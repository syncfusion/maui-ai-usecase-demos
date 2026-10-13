using System.Reflection;
using System.Text.Json;
using System.Diagnostics;

namespace CustomRichTextEditor;

/// <summary>
/// A <see cref="ContentView"/> wrapping the Syncfusion <c>SfRichTextEditor</c>
/// that adds selection capture and automatic AI-result range replacement.
/// </summary>
/// <remarks>
/// <para>AI result application flow (entirely through the engine's own transaction system):</para>
/// <list type="number">
/// <item><see cref="CaptureSelectionAsync"/> returns an immutable
/// <see cref="RichTextSelectionSnapshot"/> (positions + revision + text/html).</item>
/// <item>The application runs its own AI request (this control never calls AI).</item>
/// <item><see cref="ReplaceSelectionAsync"/> replaces EXACTLY the captured range in one
/// transaction — the engine history records one undo entry.</item>
/// <item>Stale snapshots (revision/document-size/text-at-range mismatch) are rejected
/// without modifying the note.</item>
/// </list>
/// <para>
/// Geometry APIs (<see cref="GetSelectionBoundsAsync"/>, <see cref="GetCaretBoundsAsync"/>)
/// are UI-placement-only and never identify replacement content.
/// </para>
/// </remarks>
public class CustomRichTextEditor : ContentView
{
    private const string AdapterScriptResource =
        "CustomRichTextEditor.Resources.Scripts.custom-rte-adapter.js";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly Syncfusion.Maui.RichTextEditor.SfRichTextEditor innerEditor;
    private Microsoft.Maui.Controls.WebView? engineWebView;
    private bool adapterInjected;
    private bool syncingInnerValue;
    private int innerValueChangeCount;
    private readonly object readinessLock = new();
    private Task<bool>? readinessTask;
    private static string? cachedAdapterScript;

    /// <summary>Creates the wrapper and its inner editor.</summary>
    public CustomRichTextEditor()
    {
        innerEditor = new Syncfusion.Maui.RichTextEditor.SfRichTextEditor
        {
            ShowToolbar = false,
            EditorBackgroundColor = Colors.White,
            BorderColor = Colors.Transparent,
            BorderThickness = 0,
            DefaultFont = "system-ui"
        };
        Content = innerEditor;
        innerEditor.Loaded += (_, _) =>
        {
            Debug.WriteLine("[CustomRichTextEditor] inner-editor.Loaded; scheduling adapter readiness.");
            _ = EnsureReadyAsync();
        };

        // Forward the inner editor's FormatChanged so consumers can keep
        // toolbar formatting state synchronized (same contract as the RTE).
        innerEditor.FormatChanged += (s, e) => FormatChanged?.Invoke(this, e);

        // Forward focus tracking on the inner editor.
        innerEditor.Focused += (s, e) => Focused?.Invoke(this, e);
        innerEditor.Unfocused += (s, e) => Unfocused?.Invoke(this, e);

        // Inner value -> wrapper value (user typing must reach the VM binding).
        // Listen for the actual SfRichTextEditor Value property changes
        innerEditor.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(Syncfusion.Maui.RichTextEditor.SfRichTextEditor.Value))
            {
                syncingInnerValue = true;
                try
                {
                    string innerHtml = InnerValue;
                    innerValueChangeCount++;
                    Debug.WriteLine(
                        $"[CustomRichTextEditor] HtmlChanged; count={innerValueChangeCount}; " +
                        $"htmlLength={innerHtml.Length}.");
                    LogHtmlBoundary("inner-editor.Value.changed", innerHtml);
                    SetValue(ValueProperty, innerHtml);
                }
                finally { syncingInnerValue = false; }
            }
        };
    }

    /// <summary>The wrapped Syncfusion editor (for property configuration).</summary>
    public Syncfusion.Maui.RichTextEditor.SfRichTextEditor InnerEditor => innerEditor;

    /// <summary>Value read directly off the inner editor (avoids re-entrant lookups).</summary>
    private string InnerValue =>
        (string)innerEditor.GetValue(Syncfusion.Maui.RichTextEditor.SfRichTextEditor.ValueProperty);

    /// <summary>Occurs when the editor gains focus.</summary>
    public new event EventHandler? Focused;

    /// <summary>Occurs when the editor loses focus.</summary>
    public new event EventHandler? Unfocused;

    /// <summary>Occurs when the formatting status changes (proxied from the inner editor).</summary>
    public event EventHandler<Syncfusion.Maui.RichTextEditor.RichTextEditorFormatChangedEventArgs>? FormatChanged;

    /// <summary>Background color used by the editor and highlight removal.</summary>
    public Color EditorBackgroundColor
    {
        get => innerEditor.EditorBackgroundColor;
        set => innerEditor.EditorBackgroundColor = value;
    }

    /// <summary>Default font family applied to new editor content.</summary>
    public string DefaultFontFamily
    {
        get => innerEditor.DefaultFont;
        set => innerEditor.DefaultFont = value;
    }

    public void Undo() => innerEditor.Undo();
    public void Redo() => innerEditor.Redo();
    public void ToggleBold() => innerEditor.ToggleBold();
    public void ToggleItalic() => innerEditor.ToggleItalic();
    public void ToggleUnderline() => innerEditor.ToggleUnderline();
    public void ToggleStrikethrough() => innerEditor.ToggleStrikethrough();
    public void ToggleBulletList() => innerEditor.ToggleBulletList();
    public void ToggleNumberedList() => innerEditor.ToggleNumberedList();
    public void ApplyTextColor(Color color) => innerEditor.ApplyTextColor(color);
    public void ApplyHighlightColor(Color color) => innerEditor.ApplyHighlightColor(color);
    public void ApplyFontFamily(string fontFamily) => innerEditor.ApplyFontFamily(fontFamily);
    public void ApplyParagraphFormat(
        Syncfusion.Maui.RichTextEditor.RichTextEditorParagraphFormat format) =>
        innerEditor.ApplyParagraphFormat(format);
    public void InsertTable(int rows, int columns) => innerEditor.InsertTable(rows, columns);
    public void InsertHyperlink(string text, string url) => innerEditor.InsertHyperlink(text, url);
    public void InsertImage(Syncfusion.Maui.RichTextEditor.RichTextEditorImageSource source) =>
        innerEditor.InsertImage(source);

    // ---------------- pass-through bindables ----------------

    /// <summary>Identifies the <see cref="Value"/> bindable property.</summary>
    public static readonly BindableProperty ValueProperty = BindableProperty.Create(
        nameof(Value), typeof(string), typeof(CustomRichTextEditor), string.Empty,
        BindingMode.TwoWay, propertyChanged: OnWrapperValueChanged);

    /// <summary>HTML content bound two-way with the editor (parity with RTE Value).</summary>
    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    private static void OnWrapperValueChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        var wrapper = (CustomRichTextEditor)bindable;
        if (wrapper.syncingInnerValue) { return; }
        // Defensively coerce anything non-string (e.g. Int32) to a safe
        // string. On Android the Syncfusion RTE's internal Value-notification
        // pipeline occasionally surfaces a numeric placeholder (commonly 1)
        // during the first measure/arrange pass right after navigation;
        // letting that hit the native value setter would log a
        // "BindableObject: Warning: Cannot convert 1 to type 'System.Int32'"
        // every frame and re-enter the inner editor's value-notification
        // path - both contribute to the editor page ANR.
        string html = newValue switch
        {
            null => string.Empty,
            string s => s,
            _ => newValue.ToString() ?? string.Empty
        };
        // Wrapper -> inner (programmatic content updates from the VM).
        LogHtmlBoundary("value.assigned-to-inner-editor", html);
        wrapper.innerEditor.SetValue(
            Syncfusion.Maui.RichTextEditor.SfRichTextEditor.ValueProperty,
            html);
    }

    /// <summary>Identifies the <see cref="ReadOnly"/> bindable property.</summary>
    public static readonly BindableProperty ReadOnlyProperty = BindableProperty.Create(
        nameof(ReadOnly), typeof(bool), typeof(CustomRichTextEditor), false,
        propertyChanged: OnReadOnlyChanged);

    /// <summary>Locks the inner editor while AI requests are processing.</summary>
    public bool ReadOnly
    {
        get => (bool)GetValue(ReadOnlyProperty);
        set => SetValue(ReadOnlyProperty, value);
    }

    private static void OnReadOnlyChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        var wrapper = (CustomRichTextEditor)bindable;
        wrapper.innerEditor.SetValue(
            Syncfusion.Maui.RichTextEditor.SfRichTextEditor.ReadOnlyProperty,
            (bool)(newValue ?? false));
    }

    /// <summary>Identifies the <see cref="Placeholder"/> bindable property.</summary>
    public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(
        nameof(Placeholder), typeof(string), typeof(CustomRichTextEditor), string.Empty,
        propertyChanged: OnPlaceholderChanged);

    /// <summary>Placeholder text shown by the inner editor when empty.</summary>
    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    private static void OnPlaceholderChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        var wrapper = (CustomRichTextEditor)bindable;
        wrapper.innerEditor.SetValue(
            Syncfusion.Maui.RichTextEditor.SfRichTextEditor.PlaceholderProperty,
            (string?)newValue ?? string.Empty);
    }

    // ---------------- engine plumbing ----------------

    private static string GetAdapterScript()
    {
        if (cachedAdapterScript is not null) { return cachedAdapterScript; }

        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(AdapterScriptResource)
            ?? throw new InvalidOperationException("Adapter script not found: " + AdapterScriptResource);
        using var reader = new StreamReader(stream);
        cachedAdapterScript = reader.ReadToEnd();
        return cachedAdapterScript;
    }

    private Microsoft.Maui.Controls.WebView? FindEngineWebView()
    {
        if (engineWebView?.Handler is not null) { return engineWebView; }

        foreach (var d in innerEditor.GetVisualTreeDescendants())
        {
            if (d is Microsoft.Maui.Controls.WebView wv)
            {
                engineWebView = wv;
                return wv;
            }
        }
        return null;
    }

    /// <summary>Injects the adapter script once the engine instance is available.</summary>
    private async Task<bool> EnsureReadyAsync()
    {
        Task<bool> task;
        lock (readinessLock)
        {
            if (adapterInjected) { return true; }
            task = readinessTask ??= EnsureReadyCoreAsync();
        }

        return await task;
    }

    private async Task<bool> EnsureReadyCoreAsync()
    {
        var swTotal = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            // The inner editor's Loaded callback fires while the page is still
            // mid-layout. Starting the WebView probe loop here would block
            // the navigation transition on Android's heavy WebView cold start
            // (Chromium native library load + AssetManager warmup), which is
            // the root cause of the editor page's 8s+ Davey frames and the
            // "Long monitor contention" ANR warnings. Yield once so the
            // editor's first measure/arrange cycle completes before we touch
            // the engine WebView; subsequent probes then run without holding
            // the UI thread.
            await Task.Yield();

            const int maximumAttempts = 15;
            for (int attempt = 0; attempt < maximumAttempts; attempt++)
            {
                var wv = FindEngineWebView();
                if (wv is not null)
                {
                    try
                    {
                        var swProbe = System.Diagnostics.Stopwatch.StartNew();
                        string? probe = UnwrapWebViewResult(await wv.EvaluateJavaScriptAsync(
                            "typeof window.RichTextEditorInstance !== 'undefined' && " +
                            "!!window.RichTextEditorInstance.headlessEditor ? 'ok' : 'no'"));
                        swProbe.Stop();
                        Debug.WriteLine(
                            $"[CustomRichTextEditor] adapter-probe attempt={attempt + 1}; " +
                            $"probeDurationMs={swProbe.ElapsedMilliseconds}; probeResult={probe ?? "(null)"}.");
                        if (probe == "ok")
                        {
                            var swInject = System.Diagnostics.Stopwatch.StartNew();
                            await wv.EvaluateJavaScriptAsync(GetAdapterScript());
                            swInject.Stop();

                            string? ready = UnwrapWebViewResult(await wv.EvaluateJavaScriptAsync(
                                "typeof window.__customRte !== 'undefined' ? 'ok' : 'no'"));
                            lock (readinessLock)
                                adapterInjected = ready == "ok";
                            Debug.WriteLine(
                                $"[CustomRichTextEditor] adapter-injected attempt={attempt + 1}; " +
                                $"injectDurationMs={swInject.ElapsedMilliseconds}; " +
                                $"adapterReady={adapterInjected}; totalDurationMs={swTotal.ElapsedMilliseconds}.");
                            if (adapterInjected)
                                return true;
                        }
                    }
                    catch (InvalidOperationException ex)
                    {
                        Debug.WriteLine($"[CustomRichTextEditor] WebView readiness probe failed: {ex.Message}");
                    }
                }

                if (attempt + 1 < maximumAttempts)
                {
                    // Task.Yield keeps the dispatcher responsive between
                    // probes; pairing it with a short Delay still paces
                    // attempts but no longer parks the UI thread.
                    await Task.Delay(TimeSpan.FromMilliseconds(100));
                    await Task.Yield();
                }
            }

            Debug.WriteLine(
                $"[CustomRichTextEditor] WebView adapter did not become ready within the retry window; " +
                $"totalDurationMs={swTotal.ElapsedMilliseconds}; attempts={maximumAttempts}.");
            return false;
        }
        finally
        {
            if (!adapterInjected)
            {
                lock (readinessLock)
                    readinessTask = null;
            }
            swTotal.Stop();
            Debug.WriteLine(
                $"[CustomRichTextEditor] adapter-readiness-finished; injected={adapterInjected}; " +
                $"totalDurationMs={swTotal.ElapsedMilliseconds}.");
        }
    }

    private static string EncodeJsString(string value) =>
        JsonSerializer.Serialize(value);

    /// <summary>
    /// Unwraps a WebView2/WKWebView EvaluateJavaScriptAsync return value.
    /// Some platforms return the raw value (a JSON object/array/primitive),
    /// while others double-encode the value as a JSON-quoted string. This
    /// helper unwraps both forms so deserialization never sees a malformed
    /// payload.
    /// </summary>
    private static string? UnwrapWebViewResult(string? raw)
    {
        if (string.IsNullOrEmpty(raw)) { return null; }

        string s = raw.Trim();
        // Some platforms wrap the value in literal quotes plus escape the
        // internal quotes; e.g. "\"{\\\"from\\\":0,\\\"to\\\":0}\"" — try
        // JSON-unquoting (which also un-escapes) repeatedly until stable.
        for (int i = 0; i < 3; i++)
        {
            if (s.Length < 2 || s[0] != '"' || s[^1] != '"') { break; }
            try
            {
                var unq = JsonSerializer.Deserialize<string>(s);
                if (unq is null) { break; }
                s = unq;
            }
            catch (JsonException) { break; }
        }

        // Android WebView can return a JSON.stringify result with quotes
        // escaped, with or without outer string quotes. Remove any remaining
        // bridge-encoding layers before handing it to the payload parser.
        for (int i = 0; i < 3; i++)
        {
            if (s.StartsWith("\\\"", StringComparison.Ordinal) &&
                s.EndsWith("\\\"", StringComparison.Ordinal) &&
                s.Length >= 4)
            {
                s = s[2..^2];
                continue;
            }

            if (s.Length < 4 ||
                (!s.StartsWith("{\\\"", StringComparison.Ordinal) &&
                 !s.StartsWith("[\\\"", StringComparison.Ordinal)))
            {
                break;
            }

            try
            {
                string? unescaped = JsonSerializer.Deserialize<string>("\"" + s + "\"");
                if (!string.IsNullOrWhiteSpace(unescaped) && unescaped != s &&
                    (unescaped.StartsWith('{') || unescaped.StartsWith('[')))
                {
                    s = unescaped;
                    continue;
                }

                break;
            }
            catch (JsonException ex)
            {
                Debug.WriteLine(
                    $"[CustomRichTextEditor] Could not unwrap escaped WebView JSON (length={s.Length}): {ex.Message}");
                break;
            }
        }

        return s;
    }

    private async Task<string?> ExecuteAsync(string script)
    {
        var wv = FindEngineWebView();
        if (wv is null) { return null; }

        try
        {
            return await wv.EvaluateJavaScriptAsync(script);
        }
        catch (InvalidOperationException ex)
        {
            Debug.WriteLine($"[CustomRichTextEditor] WebView JavaScript evaluation failed: {ex}");
            return null;
        }
    }

    // ---------------- public API ----------------

    /// <summary>
    /// Captures the current selection as an immutable, revision-stamped snapshot.
    /// Returns null while the engine is unavailable (load, dispose, etc.).
    /// </summary>
    public Task<RichTextSelectionSnapshot?> CaptureSelectionAsync() =>
        CaptureSnapshotAsync("captureSelection");

    /// <summary>
    /// Returns the last selection captured while the user interacted with the
    /// editor. This reads the stored range and never queries the current DOM
    /// or editor selection, so it remains safe after a toolbar takes focus.
    /// </summary>
    public Task<RichTextSelectionSnapshot?> GetStoredSelectionAsync() =>
        CaptureSnapshotAsync("getStoredSelection");

    /// <summary>
    /// Captures the entire document as a revision-stamped replacement range.
    /// This is useful when an operation should target the full document rather
    /// than a user-selected fragment.
    /// </summary>
    public Task<RichTextSelectionSnapshot?> CaptureDocumentAsync() =>
        CaptureSnapshotAsync("captureDocument");

    private async Task<RichTextSelectionSnapshot?> CaptureSnapshotAsync(string captureFunction)
    {
        if (!await EnsureReadyAsync()) { return null; }

        string? json = await ExecuteAsync(
            "(function(){" +
            " var s = window.__customRte." + captureFunction + "();" +
            " return s ? JSON.stringify(s) : null;" +
            "})()");
        Debug.WriteLine(
            $"[CustomRichTextEditor] {captureFunction} raw WebView result: " +
            $"length={json?.Length ?? 0}; startsWithQuote={json?.StartsWith('"')}; " +
            $"startsWithObject={json?.StartsWith('{')}; startsWithEscapedObject={json?.StartsWith("{\\\"", StringComparison.Ordinal)}.");
        json = UnwrapWebViewResult(json);
        if (string.IsNullOrWhiteSpace(json) || json == "null" || json == "undefined") { return null; }

        CapturePayload? data;
        try
        {
            data = JsonSerializer.Deserialize<CapturePayload>(json, JsonOptions);
        }
        catch (JsonException ex)
        {
            Debug.WriteLine(
                $"[CustomRichTextEditor] {captureFunction} snapshot JSON deserialization failed: " +
                $"length={json.Length}; prefix={json[..Math.Min(json.Length, 48)]}; error={ex}");
            return null;
        }
        if (data is null)
        {
            Debug.WriteLine($"[CustomRichTextEditor] {captureFunction} returned an empty snapshot payload.");
            return null;
        }

        var snapshot = RichTextSelectionSnapshot.Create(
            revision: data.Revision,
            from: data.From,
            to: data.To,
            selectedText: data.SelectedText ?? string.Empty,
            selectedHtml: data.SelectedHtml ?? string.Empty,
            documentFingerprint: data.DocumentFingerprint ?? string.Empty);
        Debug.WriteLine(
            $"[CustomRichTextEditor] {captureFunction} snapshot parsed: " +
            $"range={snapshot}; revision={snapshot.DocumentRevision}; htmlLength={snapshot.SelectedHtml.Length}.");
        return snapshot;
    }

    /// <summary>Gets the selected content as a standalone HTML fragment.</summary>
    public async Task<string> GetSelectedHtmlAsync()
    {
        var snapshot = await CaptureSelectionAsync();
        return snapshot?.SelectedHtml ?? string.Empty;
    }

    /// <summary>
    /// Replaces EXACTLY the captured range with <paramref name="content"/>.
    /// Stale snapshots are rejected without any document modification.
    /// Undo and Redo apply to the whole replacement as one history entry.
    /// </summary>
    public async Task<RichTextReplaceResult> ReplaceSelectionAsync(
        RichTextSelectionSnapshot snapshot,
        string content,
        RichTextContentType contentType)
    {
        if (snapshot is null)
        {
            return RichTextReplaceResult.Failure("snapshot is null");
        }
        if (string.IsNullOrEmpty(content))
        {
            return RichTextReplaceResult.Failure("content is empty");
        }
        if (!await EnsureReadyAsync())
        {
            return RichTextReplaceResult.Failure("editor engine unavailable");
        }

        string typeArg = contentType == RichTextContentType.Html ? "Html" : "PlainText";
        string script =
            "(function(){" +
            " var r = window.__customRte.replaceRange(" +
            "  { revision: " + snapshot.DocumentRevision +
            "  , from: " + snapshot.InternalFrom +
            "  , to: " + snapshot.InternalTo +
            "  , selectedText: " + EncodeJsString(snapshot.SelectedText) +
            "  , documentFingerprint: " + EncodeJsString(snapshot.InternalDocumentFingerprint) +
            "  }," +
            "  " + EncodeJsString(content) + "," +
            "  '" + typeArg + "');" +
            " return JSON.stringify(r);" +
            "})()";

        return await ExecuteOutcomeAsync(script, snapshot);
    }

    /// <summary>
    /// Replaces the entire current document in one editor transaction. This
    /// operation does not depend on a user selection or a host-side snapshot.
    /// </summary>
    public async Task<RichTextReplaceResult> ReplaceDocumentAsync(
        string content,
        RichTextContentType contentType)
    {
        if (string.IsNullOrEmpty(content))
            return RichTextReplaceResult.Failure("content is empty");
        if (!await EnsureReadyAsync())
            return RichTextReplaceResult.Failure("editor engine unavailable");

        string script =
            "(function(){" +
            " var r = window.__customRte.replaceDocument(" +
            "  " + EncodeJsString(content) + "," +
            "  '" + RichRichTextContentType(contentType) + "');" +
            " return JSON.stringify(r);" +
            "})()";

        return await ExecuteOutcomeAsync(script);
    }

    /// <summary>
    /// Restores a previous replacement only when the current document still
    /// matches the state immediately after that replacement.
    /// </summary>
    public Task<RichTextReplaceResult> RestoreReplacementAsync(RichTextReplaceReceipt receipt)
    {
        ArgumentNullException.ThrowIfNull(receipt);
        return ReplaceSelectionAsync(
            receipt.AppliedRange,
            receipt.OriginalHtml,
            RichTextContentType.Html);
    }

    /// <summary>Inserts content at the current caret (empty selection).</summary>
    public async Task<RichTextReplaceResult> InsertAtCaretAsync(
        string content,
        RichTextContentType contentType)
    {
        if (string.IsNullOrEmpty(content))
        {
            return RichTextReplaceResult.Failure("content is empty");
        }
        if (!await EnsureReadyAsync())
        {
            return RichTextReplaceResult.Failure("editor engine unavailable");
        }

        string typeArg = RichRichTextContentType(contentType);
        string script =
            "(function(){" +
            " var r = window.__customRte.insertAtCaret(" +
            "  " + EncodeJsString(content) + ",'" + typeArg + "');" +
            " return JSON.stringify(r);" +
            "})()";

        return await ExecuteOutcomeAsync(script);
    }

    /// <summary>
    /// Reads a fresh HTML serialization from the live ProseMirror document.
    /// Unlike <see cref="Value"/>, this does not rely on the asynchronous
    /// Syncfusion-to-bindable-property change notification having completed.
    /// </summary>
    public async Task<string?> GetCurrentHtmlAsync()
    {
        if (!await EnsureReadyAsync()) { return null; }

        string? raw = await ExecuteAsync("window.RichTextEditor.getHtml()");
        string? html = UnwrapWebViewResult(raw);
        LogHtmlBoundary("get-current-html.result", html);
        return html;
    }

    private static void LogHtmlBoundary(string stage, string? content)
    {
        if (content is null)
        {
            Debug.WriteLine($"[CustomRichTextEditor][HTML] {stage}; value=null.");
            return;
        }

        Debug.WriteLine(
            $"[CustomRichTextEditor][HTML] {stage}; length={content.Length}.");
    }

    private static string RichRichTextContentType(RichTextContentType contentType) =>
        contentType == RichTextContentType.Html ? "Html" : "PlainText";

    private async Task<RichTextReplaceResult> ExecuteOutcomeAsync(
        string script,
        RichTextSelectionSnapshot? replacedSnapshot = null)
    {
        var webView = FindEngineWebView();
        if (webView is null)
        {
            return RichTextReplaceResult.Failure("editor WebView unavailable");
        }

        string? raw;
        try
        {
            raw = await webView.EvaluateJavaScriptAsync(script);
        }
        catch (InvalidOperationException ex)
        {
            return RichTextReplaceResult.Failure("editor JavaScript execution failed: " + ex.Message);
        }

        string? json = UnwrapWebViewResult(raw);
        if (string.IsNullOrWhiteSpace(json) || json == "null" || json == "undefined")
        {
            return RichTextReplaceResult.Failure("editor returned no operation result");
        }

        ReplacePayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<ReplacePayload>(json, JsonOptions);
        }
        catch (JsonException ex)
        {
            return RichTextReplaceResult.Failure("editor returned an invalid operation result: " + ex.Message);
        }
        if (payload is null)
        {
            return RichTextReplaceResult.Failure("editor returned an empty operation result");
        }

        if (payload.Ok)
        {
            RichTextReplaceReceipt? receipt = null;
            if (replacedSnapshot is not null)
            {
                var appliedRange = RichTextSelectionSnapshot.Create(
                    payload.Revision,
                    payload.AppliedFrom,
                    payload.AppliedTo,
                    payload.AppliedText ?? string.Empty,
                    string.Empty,
                    payload.DocumentFingerprint ?? string.Empty);
                receipt = new RichTextReplaceReceipt(appliedRange, replacedSnapshot.SelectedHtml);
            }

            return RichTextReplaceResult.Success(payload.Revision, receipt);
        }
        if (payload.Stale) { return RichTextReplaceResult.Stale(payload.Revision); }
        return RichTextReplaceResult.Failure(payload.Error ?? "range replacement failed");
    }

    /// <summary>
    /// Returns the selection rectangles (multi-line selections produce multiple),
    /// relative to the editor's top-left corner, in device-independent pixels.
    /// For UI placement only — never for identifying content to replace.
    /// </summary>
    public async Task<IReadOnlyList<Rect>> GetSelectionBoundsAsync(
        RichTextSelectionSnapshot snapshot)
    {
        if (snapshot is null) { return Array.Empty<Rect>(); }
        if (!await EnsureReadyAsync()) { return Array.Empty<Rect>(); }

        string? json = UnwrapWebViewResult(await ExecuteAsync(
            "JSON.stringify(window.__customRte.getSelectionBounds())"));
        if (string.IsNullOrWhiteSpace(json) || json == "null" || json == "undefined")
        { return Array.Empty<Rect>(); }

        List<RectPayload>? rects;
        try
        {
            rects = JsonSerializer.Deserialize<List<RectPayload>>(json, JsonOptions);
        }
        catch (JsonException)
        {
            return Array.Empty<Rect>();
        }
        if (rects is null) { return Array.Empty<Rect>(); }

        return rects
            .Where(r => r is not null)
            .Select(r => new Rect(r!.X, r.Y, r.Width, r.Height))
            .ToList();
    }

    /// <summary>Returns the caret rectangle relative to the editor's top-left, or null.</summary>
    public async Task<Rect?> GetCaretBoundsAsync()
    {
        if (!await EnsureReadyAsync()) { return null; }

        string? json = UnwrapWebViewResult(await ExecuteAsync(
            "(function(){" +
            " var c = window.__customRte.getCaretBounds();" +
            " return c ? JSON.stringify(c) : null;" +
            "})()"));
        if (string.IsNullOrWhiteSpace(json) || json == "null" || json == "undefined")
        { return null; }

        RectPayload? p;
        try
        {
            p = JsonSerializer.Deserialize<RectPayload>(json, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
        return p is null ? null : new Rect(p.X, p.Y, p.Width, p.Height);
    }

    // ---------------- bridge payloads ----------------

    private sealed class CapturePayload
    {
        public long Revision { get; set; }
        public int From { get; set; }
        public int To { get; set; }
        public string? SelectedText { get; set; }
        public string? SelectedHtml { get; set; }
        public string? DocumentFingerprint { get; set; }
    }

    private sealed class ReplacePayload
    {
        public bool Ok { get; set; }
        public bool Stale { get; set; }
        public string? Error { get; set; }
        public long Revision { get; set; }
        public int AppliedFrom { get; set; }
        public int AppliedTo { get; set; }
        public string? AppliedText { get; set; }
        public string? DocumentFingerprint { get; set; }
    }

    private sealed class RectPayload
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
    }
}