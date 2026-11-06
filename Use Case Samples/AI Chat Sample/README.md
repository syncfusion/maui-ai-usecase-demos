# AI Chat Sample

## Overview

This sample demonstrates a customizable multi-agent AI chat experience built with .NET MAUI and Syncfusion UI components.

The sample focuses on:

- Creating and managing agents
- Customizing agent branding and avatar appearance
- Rendering a chat experience with Syncfusion AI AssistView patterns

## Syncfusion Controls Demonstrated

- **SfAIAssistView** — chat surface, follow-up suggestions, response loader, custom header template
- **SfListView** — drawer rows, agent rows, recent-chats list
- **SfExpander** — agent / recent-chats accordion sections in the navigation drawer
- **SfPopup** — agent logo / avatar editor modal
- **SfChipGroup** — Configure / Preview mode selector
- **SfAvatarView** — agent avatars in the configure form (provided via the `Syncfusion.Maui.Core` assembly)
- **SfButton** — primary actions in the agent configure / create flows

## Prerequisites

- .NET 10.0 SDK
- The MAUI workload: `dotnet workload install maui-android maui-windows`

## Run

```bash
cd AIChatSample
dotnet build AIChatSample.slnx -t:Run -f net10.0-android
# or
dotnet build AIChatSample.slnx -t:Run -f net10.0-windows10.0.19041.0
```

## AI Configuration

The sample ships with `MockAIService` registered by default, so it runs with **zero configuration** out of the box. The mock returns a deterministic reply and a usable `AgentDraft` so the Copilot flow can be exercised without any cloud credentials.

To use a real Azure OpenAI deployment, set the following environment variables before launching the app:

| Variable | Required | Description |
| --- | --- | --- |
| `AZURE_OPENAI_ENDPOINT` | yes | Azure OpenAI endpoint URL (e.g. `https://your-resource.openai.azure.com/`). Must be `https://`. |
| `AZURE_OPENAI_KEY` | yes | Azure OpenAI API key |
| `AZURE_OPENAI_DEPLOYMENT` | yes | Chat-completions deployment name |
| `AZURE_OPENAI_TIMEOUT_SECONDS` | no | Hard request timeout (default 100) |

`MauiProgram` reads these values with `Environment.GetEnvironmentVariable(...)`. When all three required variables are non-empty, the host registers `AzureOpenAIService`; otherwise it falls back to `MockAIService` so the sample stays runnable. Invalid configuration (for example, an `http://` endpoint) is caught and logged, and the sample falls back to the mock rather than crashing.

**Do not commit API keys to source control.** Use environment variables, user-secrets, or your platform's secure storage.

## Attachments

The editor's paperclip button opens the system file picker, but for this
showcase the **file name is inlined into the prompt as `[Attached: name]`
text only** — no image bytes are sent to the model. The chip above the
editor shows the picked file name and the attachment is cleared after the
next prompt is sent. If you want to forward real attachments, replace
`AIChatSuggestionViewModel.OnAttachFileAsync` and the inlining at the
call site with a provider that streams the bytes to your AI service.

## Supported Platforms

The harness is written for the sample's MAUI target matrix and is intended for Android and Windows validation. On Android the editor exposes a microphone affordance; the cross-platform fallback is a typed prompt, but `RECORD_AUDIO` is declared in `Platforms/Android/AndroidManifest.xml` so a real speech-to-text provider can be wired in without further manifest changes.
