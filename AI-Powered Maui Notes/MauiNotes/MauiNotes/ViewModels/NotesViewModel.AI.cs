using System.Text.Json;
using System.Net;
using System.Text.RegularExpressions;
using System.Diagnostics;
using CustomRichTextEditor;
using MauiNotes.Models;
using MauiNotes.Models.AI;

namespace MauiNotes.ViewModels;

public partial class NotesViewModel
{
    public async Task<bool> BeginAiAssistantAsync(CustomRichTextEditor.CustomRichTextEditor editor)
    {
        if (IsAiProcessing || IsAiPopupOpen)
        {
            Debug.WriteLine(
                $"[AI] Reusing active AI session; processing={IsAiProcessing}; " +
                $"popupOpen={IsAiPopupOpen}; snapshotAvailable={aiSelectedSnapshot is not null}.");
            return true;
        }

        AiError = string.Empty;
        AiStatus = string.Empty;
        AiCustomInstruction = string.Empty;
        aiEditor = editor;
        aiSourceNote = SelectedNote;
        aiSourceHtml = string.Empty;
        SetAiSelectedSnapshot(null, "Starting a new AI session");
        aiReplaceWholeNote = false;
        aiSelectedText = string.Empty;
        aiSelectedHtml = string.Empty;
        IsAiPopupOpen = false;

        try
        {
            // Prefer the immutable selection already cached by the editor,
            // then inspect the live selection before falling back to the
            // entire note. A missing user selection is not an AI launch error.
            SetAiSelectedSnapshot(await editor.GetStoredSelectionAsync(), "Read editor's stored selection");
            if (aiSelectedSnapshot is not { HasSelection: true })
                SetAiSelectedSnapshot(await editor.CaptureSelectionAsync(), "Captured live editor selection");

            aiReplaceWholeNote = aiSelectedSnapshot is not { HasSelection: true };
            if (aiReplaceWholeNote)
                SetAiSelectedSnapshot(await editor.CaptureDocumentAsync(), "Captured whole-document target");
        }
        catch (InvalidOperationException ex)
        {
            Debug.WriteLine($"[AI] Selection capture failed: {ex}");
            AiError = ex.Message;
        }

        if (!aiReplaceWholeNote && aiSelectedSnapshot is { HasSelection: true })
        {
            aiSelectedText = aiSelectedSnapshot.SelectedText;
            aiSelectedHtml = aiSelectedSnapshot.SelectedHtml;
        }
        Debug.WriteLine(
            $"[AI] Selection captured: {(aiReplaceWholeNote ? "whole document" : "selected range")}; " +
            $"aiSelectedSnapshot={(aiSelectedSnapshot is null ? "null" : "available")}; " +
            $"revision={aiSelectedSnapshot?.DocumentRevision}; " +
            $"textLength={aiSelectedText.Length}; htmlLength={aiSelectedHtml.Length}.");

        if (aiSelectedSnapshot is null)
        {
            AiError = "The editor selection could not be captured. Wait for the editor to finish loading, then try again.";
            Debug.WriteLine("[AI] Selection capture failed; no AI action can be started without a replacement snapshot.");
        }

        if (aiSourceNote is not null)
        {
            try
            {
                string rawEditorHtml = await editor.GetCurrentHtmlAsync() ?? string.Empty;
                LogHtmlBoundary("ai-popup.editor-html.raw", rawEditorHtml, aiSourceNote.Id);
                LogHtmlBoundary("ai-popup.note-content.before-sync", aiSourceNote.Content, aiSourceNote.Id);
                aiSourceHtml = NormalizeEscapedHtml(rawEditorHtml);
                LogHtmlBoundary("ai-popup.editor-html.normalized", aiSourceHtml, aiSourceNote.Id);
                if (aiSourceHtml.Length > 0 && aiSourceNote.Content != aiSourceHtml)
                    UpdateNoteFromEditor(
                        aiSourceNote,
                        aiSourceHtml,
                        DateTime.Now,
                        updateModifiedDate: false);
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[AI] Editor HTML capture failed after snapshot capture: {ex}");
                AiError = ex.Message;
            }
        }

        IsAiPopupOpen = true;
        Debug.WriteLine($"[AI] Writing-tools popup opened; aiSelectedSnapshot={(aiSelectedSnapshot is null ? "null" : "available")}.");
        return true;
    }

    private async Task StartAiRequestAsync(AiEditingAction action, string customInstruction)
    {
        if (!IsAiPopupOpen || !IsAiActionEnabled)
        {
            Debug.WriteLine(
                $"[AI] Request not started: popupOpen={IsAiPopupOpen}; actionEnabled={IsAiActionEnabled}; " +
                $"noteSelected={SelectedNote is not null}; persistenceAvailable={isPersistenceAvailable}.");
            return;
        }

        if (action == AiEditingAction.Custom)
        {
            customInstruction = customInstruction.Trim();
            if (string.IsNullOrWhiteSpace(customInstruction))
                return;
        }

        if (aiSourceNote is null || aiSourceNote.IsLocked)
        {
            AiError = "Select an editable note before using AI.";
            return;
        }

        if (aiSelectedSnapshot is null)
        {
            AiError = "The selected content is no longer available. Close the AI tools and capture the selection again.";
            Debug.WriteLine("[AI] Action rejected because aiSelectedSnapshot is null.");
            return;
        }
        RichTextSelectionSnapshot selectedSnapshot = aiSelectedSnapshot;

        if (aiReplaceWholeNote)
        {
            try
            {
                aiSourceHtml = aiEditor is null
                    ? aiSourceNote.Content
                    : await aiEditor.GetCurrentHtmlAsync() ?? aiSourceNote.Content;
            }
            catch (InvalidOperationException ex)
            {
                AiError = ex.Message;
                return;
            }

            aiSelectedHtml = aiSourceHtml;
            aiSelectedText = ExtractAiPlainText(aiSourceHtml);
        }

        if (string.IsNullOrWhiteSpace(aiSelectedHtml) ||
            (aiReplaceWholeNote && string.IsNullOrWhiteSpace(aiSelectedText)))
        {
            AiError = "This note has no content for the selected AI action.";
            return;
        }

        if (string.IsNullOrWhiteSpace(aiSourceHtml))
        {
            AiError = "The editor content is not available yet. Try again.";
            return;
        }

        if (aiEditor is not { } editor || aiSourceNote is not { } sourceNote)
        {
            AiError = "The active editor or note is no longer available.";
            return;
        }

        string sourceHtml = aiSourceHtml;
        string selectedText = aiSelectedText;
        string selectedHtml = aiSelectedHtml;
        bool replaceWholeNote = aiReplaceWholeNote;

        AiError = string.Empty;
        AiStatus = "Generating AI edit...";
        Debug.WriteLine(
            $"[AI] Starting {action} request: wholeDocument={replaceWholeNote}; " +
            $"aiSelectedSnapshot=available; revision={selectedSnapshot.DocumentRevision}; " +
            $"textLength={selectedText.Length}; htmlLength={selectedHtml.Length}.");
        aiLastAction = action;
        aiLastCustomInstruction = customInstruction;
        aiAppliedReceipt = null;
        aiAppliedNote = null;
        SetHasAiReviewResult(false);
        OnPropertyChanged(nameof(CanRetryAiRequest));
        IsAiProcessing = true;
        IsAiPopupOpen = false;
        Debug.WriteLine("[AI] Writing-tools popup closed after action selection; snapshot retained for processing.");
        if (!IsAiProcessing)
        {
            Debug.WriteLine("[AI] Processing was cancelled while the writing-tools popup was closing.");
            return;
        }

        await ProcessAiAsync(
            action,
            customInstruction,
            selectedSnapshot,
            editor,
            sourceNote,
            sourceHtml,
            selectedText,
            selectedHtml,
            replaceWholeNote);
    }

    private async Task ProcessAiAsync(
        AiEditingAction action,
        string customInstruction,
        RichTextSelectionSnapshot snapshot,
        CustomRichTextEditor.CustomRichTextEditor editor,
        NoteModel sourceNote,
        string sourceHtml,
        string selectedText,
        string selectedHtml,
        bool replaceWholeNote)
    {
        bool applied = false;

        aiCancellation?.Dispose();
        var requestCancellation = new CancellationTokenSource();
        aiCancellation = requestCancellation;
        int requestVersion = ++aiRequestVersion;
        Debug.WriteLine(
            $"[AI] Process entered: action={action}; aiSelectedSnapshot={(aiSelectedSnapshot is null ? "null" : "available")}; " +
            $"snapshotRevision={snapshot.DocumentRevision}; requestVersion={requestVersion}.");

        try
        {
            Debug.WriteLine(
                $"[AI] Sending {action} request; snapshotRevision={snapshot.DocumentRevision}; " +
                $"hasSelection={snapshot.HasSelection}.");
            string result = await aiEditingService.GenerateAsync(
                new AiEditingRequest(action, selectedText, selectedHtml, customInstruction),
                requestCancellation.Token);
            LogHtmlBoundary("ai-response.raw", result, sourceNote.Id);
            if (requestVersion != aiRequestVersion)
            {
                Debug.WriteLine($"[AI] Ignoring response for obsolete requestVersion={requestVersion}.");
                return;
            }

            result = NormalizeEscapedHtml(result);
            LogHtmlBoundary("ai-response.normalized", result, sourceNote.Id);
            Debug.WriteLine(
                $"[AI] Request completed; response received for {action}: resultLength={result.Length}; " +
                $"revision={snapshot.DocumentRevision}; replacingWholeDocument={replaceWholeNote}.");
            if (!ReferenceEquals(sourceNote, SelectedNote))
            {
                AiError = "The selected note changed before the response completed. No result was applied.";
                return;
            }

            if (sourceNote.IsLocked || sourceNote.Content != sourceHtml)
            {
                AiError = "The note changed or was locked before the response completed. No result was applied.";
                return;
            }

            LogHtmlBoundary("editor-replacement.argument", result, sourceNote.Id);
            RichTextReplaceResult replacement = await editor.ReplaceSelectionAsync(
                snapshot,
                result,
                RichTextContentType.Html);
            Debug.WriteLine(
                $"[AI] Replacement returned: succeeded={replacement.Succeeded}; stale={replacement.IsStaleSelection}; " +
                $"snapshotRevision={snapshot.DocumentRevision}; fieldSnapshotAvailable={aiSelectedSnapshot is not null}.");
            if (!replacement.Succeeded)
            {
                Debug.WriteLine(
                    $"[AI] Editor replacement failed: stale={replacement.IsStaleSelection}; " +
                    $"error={replacement.ErrorMessage ?? "(none)"}.");
                AiError = replacement.IsStaleSelection
                    ? "The selected content changed while AI was processing. No result was applied."
                    : replacement.ErrorMessage ?? "The AI result could not be applied.";
                return;
            }
            Debug.WriteLine("[AI] Editor replacement succeeded; reading updated HTML.");

            string? updatedHtml = await editor.GetCurrentHtmlAsync();
            LogHtmlBoundary("editor.html.after-ai-replacement", updatedHtml, sourceNote.Id);
            if (updatedHtml is null)
            {
                AiError = "The editor applied the AI result, but could not confirm the updated note. Review the note before continuing.";
                return;
            }

            UpdateNoteFromEditor(sourceNote, updatedHtml, DateTime.Now);
            LogHtmlBoundary("note.content.after-ai-update", sourceNote.Content, sourceNote.Id);
            aiAppliedReceipt = replacement.UndoReceipt;
            aiAppliedNote = sourceNote;
            SetHasAiReviewResult(aiAppliedReceipt is not null);
            AiStatus = "AI edit applied.";
            Debug.WriteLine(
                $"[AI] Updated note from editor: htmlLength={updatedHtml.Length}; " +
                $"undoReceipt={(aiAppliedReceipt is null ? "unavailable" : "available")}; " +
                $"aiSelectedSnapshotAvailableAfterApply={aiSelectedSnapshot is not null}.");
            applied = true;
        }
        catch (OperationCanceledException)
        {
            if (requestVersion == aiRequestVersion)
                AiError = requestCancellation.IsCancellationRequested
                    ? "The AI request was cancelled."
                    : "The AI request timed out. Try again.";
        }
        catch (HttpRequestException ex)
        {
            AiError = ex.Message;
        }
        catch (InvalidOperationException ex)
        {
            Debug.WriteLine($"[AI] Editor HTML capture threw: {ex}");
            AiError = ex.Message;
        }
        catch (ArgumentException ex)
        {
            AiError = ex.Message;
        }
        catch (JsonException)
        {
            AiError = "The AI service returned a response that could not be read. Try again.";
        }
        finally
        {
            requestCancellation.Dispose();
            if (requestVersion == aiRequestVersion)
            {
                IsAiProcessing = false;
                aiCancellation = null;
                if (!applied)
                {
                    SetHasAiReviewResult(false);
                    aiAppliedReceipt = null;
                }
                Debug.WriteLine(
                    $"[AI] Request finished: applied={applied}; error={(string.IsNullOrWhiteSpace(AiError) ? "(none)" : AiError)}; " +
                    $"aiSelectedSnapshotAvailable={aiSelectedSnapshot is not null}.");
            }
        }
    }

    private async Task RevertAiResultAsync()
    {
        if (!CanUseAiReviewActions || aiAppliedReceipt is null ||
            aiAppliedNote is null || aiEditor is null)
            return;

        Debug.WriteLine(
            $"[AI] Explicit Revert action restoring AI replacement; noteContentLength={aiAppliedNote.Content?.Length ?? 0}.");
        RichTextReplaceResult restored =
            await aiEditor.RestoreReplacementAsync(aiAppliedReceipt);
        if (!restored.Succeeded)
        {
            AiError = restored.IsStaleSelection
                ? "The note changed after the AI edit, so it could not be reverted safely."
                : restored.ErrorMessage ?? "The AI edit could not be reverted.";
            return;
        }

        string? html = await aiEditor.GetCurrentHtmlAsync();
        if (html is null)
        {
            AiError = "The AI edit was reverted, but the updated note could not be confirmed.";
            return;
        }

        UpdateNoteFromEditor(aiAppliedNote, html, DateTime.Now);
        AiStatus = "AI edit reverted.";
        aiAppliedReceipt = null;
        SetHasAiReviewResult(false);
        ClearAiSelection("AI result reverted");
    }

    private async Task RetryAiRequestAsync()
    {
        if (!CanRetryAiRequest || aiEditor is null || aiSourceNote is null ||
            aiLastAction is null)
            return;

        if (!HasAiReviewResult)
        {
            AiError = string.Empty;
            IsAiPopupOpen = true;
            await StartAiRequestAsync(aiLastAction.Value, aiLastCustomInstruction);
            return;
        }

        if (aiAppliedReceipt is null || aiAppliedNote is null)
            return;

        RichTextReplaceResult restored =
            await aiEditor.RestoreReplacementAsync(aiAppliedReceipt);
        if (!restored.Succeeded || restored.UndoReceipt is null)
        {
            AiError = restored.IsStaleSelection
                ? "The note changed after the AI edit, so the request cannot be retried safely."
                : restored.ErrorMessage ?? "The AI edit could not be reverted for retry.";
            return;
        }

        string? html = await aiEditor.GetCurrentHtmlAsync();
        if (html is null)
        {
            AiError = "The original note could not be read to retry the AI request.";
            return;
        }

        UpdateNoteFromEditor(aiAppliedNote, html, DateTime.Now);
        SetAiSelectedSnapshot(restored.UndoReceipt.AppliedRangeSnapshot, "Restored snapshot for retry");
        aiAppliedReceipt = null;
        SetHasAiReviewResult(false);
        IsAiPopupOpen = true;
        await StartAiRequestAsync(aiLastAction.Value, aiLastCustomInstruction);
    }

    private async Task ApplyEditorHistoryActionAsync(bool undo)
    {
        if (!CanUseAiReviewActions || aiEditor is null || aiAppliedNote is null)
            return;

        if (undo)
            aiEditor.Undo();
        else
            aiEditor.Redo();

        string? html = await aiEditor.GetCurrentHtmlAsync();
        if (html is not null)
            UpdateNoteFromEditor(aiAppliedNote, html, DateTime.Now);
    }

    private void SetHasAiReviewResult(bool value)
    {
        if (hasAiReviewResult == value)
            return;

        hasAiReviewResult = value;
        OnPropertyChanged(nameof(HasAiReviewResult));
        OnPropertyChanged(nameof(CanUseAiReviewActions));
        OnPropertyChanged(nameof(CanRetryAiRequest));
        OnPropertyChanged(nameof(CanFinishAiReview));
    }

    private static string ExtractAiPlainText(string html)
    {
        string withBreaks = Regex.Replace(
            html,
            @"</(?:p|h[1-6]|li|tr|div)\s*>|<br\s*/?>",
            "\n",
            RegexOptions.IgnoreCase);
        string withoutTags = Regex.Replace(withBreaks, "<[^>]+>", " ");
        return WebUtility.HtmlDecode(withoutTags).Trim();
    }

    private void UpdateNoteFromEditor(
        NoteModel note,
        string html,
        DateTime modifiedDate,
        bool updateModifiedDate = true)
    {
        LogHtmlBoundary("editor-content.update.input", html, note.Id);
        string normalizedHtml = NormalizeEscapedHtml(html);
        LogHtmlBoundary("editor-content.update.normalized", normalizedHtml, note.Id);
        editorHtml = normalizedHtml;
        note.Content = normalizedHtml;
        LogHtmlBoundary("note.content.after-editor-update", note.Content, note.Id);
        if (ReferenceEquals(note, SelectedNote))
        {
            UpdateNoteTitleFromEditor(normalizedHtml);
            Debug.WriteLine(
                $"[HTML] note.metadata.after-editor-update; noteId={note.Id}; " +
                $"titleLength={note.Title.Length}; previewLength={note.PreviewText.Length}; " +
                $"titleContainsEscapedTag={Regex.IsMatch(note.Title, @"\\+u003[cC]", RegexOptions.IgnoreCase)}; " +
                $"previewContainsEscapedTag={Regex.IsMatch(note.PreviewText, @"\\+u003[cC]", RegexOptions.IgnoreCase)}.");
            if (updateModifiedDate)
            {
                note.ModifiedDate = modifiedDate;
                OnPropertyChanged(nameof(EditorTimestampText));
            }
        }
        ScheduleAutosave();
    }

    private void CancelAiProcessing()
    {
        if (!IsAiProcessing)
        {
            if (HasAiReviewResult)
            {
                Debug.WriteLine(
                    $"[AI] Cancel selected after result application; closing review only. " +
                    $"noteContentLength={aiAppliedNote?.Content?.Length ?? SelectedNote?.Content?.Length ?? 0}; no restore requested.");
                CloseAiAssistant();
            }
            return;
        }

        Debug.WriteLine("[AI] Cancel selected while request is active; cancelling the pending request.");
        aiRequestVersion++;
        CancellationTokenSource? cancelledRequest = aiCancellation;
        aiCancellation = null;
        cancelledRequest?.Cancel();
        IsAiProcessing = false;
        AiStatus = string.Empty;
        ClearAiSelection("AI processing cancelled");
    }

    private async Task CancelAiReviewAsync()
    {
        if (IsAiProcessing)
        {
            CancelAiProcessing();
            CloseAiAssistant();
            return;
        }

        CloseAiAssistant();
    }

    private void CloseAiAssistant()
    {
        Debug.WriteLine(
            $"[AI] Closing AI assistant UI; processing={IsAiProcessing}; hasResult={HasAiReviewResult}; " +
            $"noteContentLength={SelectedNote?.Content?.Length ?? 0}. No editor restoration is performed.");
        if (IsAiProcessing)
            CancelAiProcessing();
        IsAiPopupOpen = false;
        AiCustomInstruction = string.Empty;
        AiError = string.Empty;
        AiStatus = string.Empty;
        aiAppliedReceipt = null;
        SetHasAiReviewResult(false);
        ClearAiSelection("AI workflow closed");
    }

    private void ClearAiSelection(string reason)
    {
        Debug.WriteLine(
            $"[AI] Clearing captured AI context: reason={reason}; " +
            $"snapshotAvailable={aiSelectedSnapshot is not null}.");
        aiSourceNote = null;
        aiEditor = null;
        aiSourceHtml = string.Empty;
        SetAiSelectedSnapshot(null, reason);
        aiReplaceWholeNote = false;
        aiSelectedText = string.Empty;
        aiSelectedHtml = string.Empty;
        aiAppliedNote = null;
    }

    private void SetAiSelectedSnapshot(RichTextSelectionSnapshot? snapshot, string reason)
    {
        aiSelectedSnapshot = snapshot;
        Debug.WriteLine(
            $"[AI] aiSelectedSnapshot {(snapshot is null ? "cleared" : "stored")}: reason={reason}; " +
            $"revision={snapshot?.DocumentRevision}; hasSelection={snapshot?.HasSelection}; " +
            $"selectedTextLength={snapshot?.SelectedText.Length ?? 0}; selectedHtmlLength={snapshot?.SelectedHtml.Length ?? 0}.");
    }
}
