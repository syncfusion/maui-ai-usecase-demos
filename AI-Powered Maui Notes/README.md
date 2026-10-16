# AI-Powered Maui Notes with .NET MAUI RichTextEditor and Azure OpenAI

A cross-platform .NET MAUI notes application built around the **Syncfusion RichTextEditor**. Notes are organized in a tree of folders and notes, edited in a full rich text editor (formatting toolbar, text/highlight palettes, tables, links, images), and enhanced with an **AI writing assistant** powered by **Azure OpenAI** that edits the selected text (or the whole note) in place — with Revert, Retry, Undo, Redo, and Done controls.

The sample is fully self-contained: notes are persisted to a local JSON file, and the first run seeds realistic sample content (Work / Personal / Ideas folders) so the AI features can be tried immediately.

## Highlights

- **Syncfusion RichTextEditor** as the central control: rich formatting, palette-based text color and highlight, tables, links, images, and native HTML access
- **AI writing actions** on the selected text or the whole note: Grammar Check, Proofread, Rewrite, Summarize, Key Points, Convert to List, Convert to Table, Friendly Tone, Professional Tone, Concise, and a free-form Custom instruction
- **Safe in-place AI edits**: the AI result is applied through the editor's selection model with an undo receipt, so Revert, Retry, Undo, and Redo always work; stale selections (content changed while the request was in flight) are detected and the result is not applied
- **Robust AI client**: environment-variable configuration, HTTPS-only endpoint validation, input size limits, status-code-specific error messages, cancellation, and prompt-injection-hardened system prompt
- Notes organization with **Syncfusion TreeView**: folders, create/rename/duplicate/move/delete, note locking, Today/Yesterday/week groupings
- **MVVM architecture** with the built-in MAUI dependency injection — no external DI framework
- Works across **Android, iOS, macOS, and Windows**

## Prerequisites

- .NET 10.0 SDK
- Visual Studio 2022 (17.12+) or VS Code with the .NET MAUI workload
- A Syncfusion .NET MAUI Community or Commercial license key
- An active **Azure OpenAI** resource with a deployed chat model (e.g., `gpt-4.1`)

## Setup

The sample reads all credentials from **environment variables** (no values are hardcoded):

| Variable | Description |
|---|---|
| `SYNCFUSION_LICENSE_KEY` | Your Syncfusion license key (required at startup) |
| `MAUINOTES_AZURE_OPENAI_ENDPOINT` | Absolute HTTPS endpoint of your Azure OpenAI resource, e.g. `https://<resource>.openai.azure.com` |
| `MAUINOTES_AZURE_OPENAI_DEPLOYMENT` | The name of the deployed model, e.g. `gpt-4.1` |
| `MAUINOTES_AZURE_OPENAI_API_KEY` | The API key for the resource |

Example (PowerShell, per development session):

```powershell
$env:SYNCFUSION_LICENSE_KEY   = "YOUR_SYCFUSION_LICENSE_KEY"
$env:MAUINOTES_AZURE_OPENAI_ENDPOINT = "https://YOUR_RESOURCE.openai.azure.com"
$env:MAUINOTES_AZURE_OPENAI_DEPLOYMENT = "gpt-4.1"
$env:MAUINOTES_AZURE_OPENAI_API_KEY = "YOUR_AZURE_OPENAI_API_KEY"
```

If the AI variables are not set, the notes app still works — tapping an AI action shows the message:
*“AI editing is not configured. Set MAUINOTES_AZURE_OPENAI_API_KEY for local development.”*

**Run:** open `MauiNotes.slnx` and start the `MauiNotes` project with your chosen platform target (Android, iOS, macOS, or Windows).

## How to Use the AI Assistant

1. Open a note and (optionally) select a range of text in the editor.
2. Tap the **AI** button in the editor toolbar to open the writing-tools popup.
3. Choose a built-in action or type a custom instruction and send it.
4. A review popup appears while the request is in flight (spinner), then shows the result with **Revert / Retry / Undo / Redo / Done / Cancel**.
5. If something goes wrong (bad credentials, quota exceeded, content changed mid-flight), the error message is shown in the popup and Retry becomes available.

If no text is selected, the AI action applies to the entire note.

## Project Structure

```
MauiNotes/
├── CustomRichTextEditor/           # Reusable RichTextEditor wrapper (selection capture, in-place replace, undo receipts)
└── MauiNotes/
    ├── Models/                     # Note, tree nodes, AI request/action models
    ├── Services/
    │   ├── AI/                     # IAiEditingService + Azure OpenAI implementation
    │   └── Persistence/            # Local JSON note store
    ├── ViewModels/                 # NotesViewModel (partial classes by concern: AI, NoteTree, NoteContent, Editor, Persistence)
    ├── Views/                      # Mobile / desktop page shells
    └── Controls/                   # Editor host, toolbar, popups (AI, format, palettes, node actions)
```

## Key Components (API)

| Member | Description |
|---|---|
| `NotesViewModel.BeginAiAssistantAsync(editor)` | Captures the editor selection (or the whole document) and opens the writing-tools popup |
| `NotesViewModel.ProcessAiAsync(...)` | Runs the AI request, validates the response, applies the in-place replacement, and exposes the undo receipt |
| `AzureOpenAiEditingService.GenerateAsync(request, ct)` | Sends the chat-completions request to Azure OpenAI with input limits, error mapping, and cancellation |
| `AzureOpenAiOptions` | Resolves endpoint/deployment/key from environment variables; enforces HTTPS and builds the `openai/v1/chat/completions` URI |
| `CustomRichTextEditor.ReplaceSelectionAsync(snapshot, html, type)` | Applies AI HTML to the captured selection range and returns an undo receipt |
| `CustomRichTextEditor.RestoreReplacementAsync(receipt)` | Restores the pre-AI content (used by Revert and Retry) |
| `JsonNotePersistenceService` | Saves/loads the whole note tree to a local JSON file (autosaved after edits) |

## Why Use This

- **Productivity:** Edit, rewrite, summarize, and restructure note content with one tap, without leaving the editor
- **Safety:** AI results are applied through an undoable editor operation — nothing is ever applied silently or lost
- **Clean Architecture:** Full MVVM with DI makes every layer (AI service, persistence, view model) independently testable
- **Rich UI:** Native rich text editing with Syncfusion controls on all platforms, no WebView dependency in the app pages
