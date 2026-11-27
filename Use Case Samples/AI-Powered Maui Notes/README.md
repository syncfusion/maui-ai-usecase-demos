# AI-Powered Maui Notes

A cross-platform .NET MAUI notes app that uses Azure OpenAI to edit and improve note content directly in a rich text editor.

## Overview

The sample demonstrates how to add AI-assisted writing tools to a personal notes app while keeping the user in control of edits. Users can organize notes and folders, select text (or work on an entire note), request an AI transformation, and review or undo the result.

- **What the sample does:** Organizes and edits locally persisted notes, with AI writing actions available from the editor.
- **Who it is for:** Developers exploring AI integration, rich text editing, and note organization in .NET MAUI.
- **AI capability:** Azure OpenAI chat completions for grammar checking, proofreading, rewriting, summarizing, tone changes, and content restructuring.
- **Outcome:** Users get a proposed edit applied in place with review, revert, retry, undo, and redo options.

## Key Features

- Organize folders and notes, including creating, renaming, duplicating, moving, deleting, and locking notes.
- Edit notes with rich text formatting, text and highlight colors, tables, links, and images.
- Apply built-in AI actions or a custom instruction to selected text or the entire note.
- Review AI changes safely with selection checks, cancellation, revert, retry, undo, and redo.
- Persist notes locally as JSON and seed sample notes on first launch.

## AI Flow

1. The user selects text in a note or starts an AI action without a selection to target the whole note.
2. The app captures the editor selection or document as HTML.
3. The AI service sends the requested edit and selected HTML to the configured Azure OpenAI deployment over HTTPS.
4. The app converts Markdown responses to semantic HTML, sanitizes formatted content, and checks that the source note and selection have not changed.
5. The editor applies formatted HTML or inserts plain text while preserving the selected content's existing formatting; the user can review and manage the change.
6. If configuration or a request fails, the app displays an error and provides retry where appropriate; the user can cancel a pending request or revert an applied edit.

## Controls Used

- **Syncfusion .NET MAUI RichTextEditor** — edits formatted note content and provides selection, HTML, and editing history support.
- **Syncfusion .NET MAUI TreeView** — displays the nested folder and note organization.
- **Syncfusion .NET MAUI ListView** — presents note and folder collections in the app UI.
- **Syncfusion .NET MAUI Popup** — hosts AI writing tools, formatting palettes, and review actions.
- **Syncfusion .NET MAUI Toolbar and Buttons** — provide editor and note actions.

## Architecture

The app uses MAUI dependency injection and an MVVM design. The page and control layer binds to `NotesViewModel`, which coordinates note organization, editor state, and AI actions. `IRichTextEditorController` keeps editor-specific types behind an adapter. `IAiEditingService` abstracts AI generation; `AzureOpenAiEditingService` sends requests to Azure OpenAI through a typed `HttpClient`. `AiResponseFormatter` converts Markdown responses and preserves plain-text insertion behavior, while `AiHtmlSanitizer` filters formatted content before it reaches the editor. `AzureOpenAiOptions` reads the AI configuration from environment variables. `JsonNotePersistenceService` asynchronously stores and loads the note tree in the app's local data directory using source-generated JSON metadata.

Data flows from the editor to the view model and AI service, then back through a guarded editor replacement; note changes are autosaved through the persistence service.

## Prerequisites

- .NET 10 SDK.
- .NET MAUI workload for the target platform(s).
- Visual Studio 2022 (17.12 or later) with the .NET MAUI workload, or VS Code with the corresponding .NET MAUI tooling.
- Syncfusion .NET MAUI packages (version 35.1.37 in this sample) and a valid Syncfusion Community or Commercial license key.
- An Azure OpenAI resource with a deployed chat model that supports the Azure OpenAI v1 chat completions API.

## AI Configuration

The sample uses a real Azure OpenAI service; it does not include mock AI or offline AI behavior. For local development, set the following environment variables before launching the app. A Syncfusion license key can also be supplied through the environment.

| Variable | Required | Description |
|---|---|---|
| `MAUINOTES_AZURE_OPENAI_ENDPOINT` | yes | Absolute HTTPS endpoint of the Azure OpenAI resource. |
| `MAUINOTES_AZURE_OPENAI_API_KEY` | yes | Azure OpenAI resource API key. |
| `MAUINOTES_AZURE_OPENAI_DEPLOYMENT` | yes | Name of the deployed chat model. |
| `SYNCFUSION_LICENSE_KEY` | optional | Syncfusion license key used to register the Syncfusion controls. Replace the source placeholder only with an appropriate development value; do not commit a real key. |

The app's typed `HttpClient` timeout is set to 60 seconds in code. These environment variables work only when they are present in the process environment; Android and iOS apps cannot inherit your workstation's environment variables. In this sample, AI actions are unavailable on those devices unless an appropriate configuration mechanism is provided. Do not embed an Azure key in a mobile app or appsettings file shipped with it. For mobile or production deployments, put the Azure key behind an authenticated proxy backend and have the app call that backend; this sample does not include the proxy implementation. The AI endpoint must use HTTPS. If AI configuration is missing, notes and local editing remain available and AI actions show a configuration message.

## Default Behavior

- **AI:** Real Azure OpenAI chat completions; no mock or local/offline fallback is included.
- **Notes:** Stored locally in the app data directory as JSON. First launch seeds sample folders and notes.
- **Missing AI configuration:** The notes app remains usable, and AI actions display an explanatory configuration error.

## Run

Build and run from the sample directory with the target platform framework. For example:

```sh
dotnet build MauiNotes.slnx -t:Run -f net10.0-android
```

For other supported targets, use the corresponding framework:

```sh
dotnet build MauiNotes.slnx -t:Run -f net10.0-ios
dotnet build MauiNotes.slnx -t:Run -f net10.0-maccatalyst
dotnet build MauiNotes.slnx -t:Run -f net10.0-windows10.0.19041.0
```

Set the required environment variables in the app's launch environment before running on a development host if you want to use AI features. The environment-variable approach is not a mobile-device configuration mechanism.

## Output

![AI-Powered Maui Notes running on Windows](MauiNotes_windows.gif)

## Project Structure

- `MauiNotes/MauiNotes/Views/` — mobile and desktop pages and layouts.
- `MauiNotes/MauiNotes/ViewModels/` — presentation logic for notes, editor state, and AI workflows.
- `MauiNotes/MauiNotes/Models/` — note, folder, and AI request/action data structures.
- `MauiNotes/MauiNotes/Services/AI/` — AI service abstraction, Azure OpenAI implementation, and configuration.
- `MauiNotes/MauiNotes/Services/Persistence/` — local JSON note persistence.
- `MauiNotes/MauiNotes/Controls/` — editor host, toolbars, and AI/formatting popups.
- `MauiNotes/CustomRichTextEditor/` — rich text editor wrapper, selection handling, and HTML adapter.
- `MauiNotes/MauiNotes/Resources/` — styles, images, fonts, and other assets.

## Troubleshooting

**Issue: Missing AI configuration**

Cause: One or more Azure OpenAI environment variables are unset, or the endpoint is not an absolute HTTPS URL.

Fix: Set the endpoint, API key, and deployment name in the app's environment, then restart it.

**Issue: Build or workload errors**

Cause: The required .NET SDK or platform MAUI workload is missing or incompatible.

Fix: Verify the .NET 10 SDK and install the MAUI workload for the target platform. Confirm that the installed Syncfusion package versions are compatible.

**Issue: AI response is unexpected or the request fails**

Cause: The prompt, selected input, endpoint, deployment, credentials, or service availability may be incorrect.

Fix: Check the content and action sent to AI, verify the endpoint/deployment/key, and review the displayed service error. Retry when the service is available.

**Issue: An AI result is not applied**

Cause: The note, lock state, or editor selection changed while the AI request was in progress.

Fix: No stale result is applied; reopen the AI tools, select the intended content again, and retry.

## Security and Privacy Notes

- Do not hard-code or commit Azure OpenAI or Syncfusion license keys.
- Use environment variables or user secrets only for local development. Do not package credentials in a mobile client.
- For mobile or production use, route AI requests through an authenticated backend proxy that holds the Azure credential server-side; this sample does not supply that backend.
- Send only the note content needed for the requested edit; selected content or the full note is sent to Azure OpenAI depending on the user's selection.
- Review your Azure OpenAI data handling and privacy requirements before sending sensitive note content.
- The AI endpoint is validated as HTTPS, and the service limits request size; avoid including unnecessary sensitive information in notes submitted for AI processing.

## Supported Platforms

- Android
- iOS
- Windows
- macOS (Mac Catalyst)
