# LLM Routing

![LLM Routing demo](LLM_Routing/Resources/Images/LLM-Routing.gif)

A .NET MAUI chat sample that uses `gpt-4.1-nano` to classify query complexity, then routes the request to `gpt-4.1-nano`, `gpt-4.1-mini`, or `gpt-5.6-luna`.

## Overview

The chat adds the selected model, input/output token counts, estimated cost, and generated text to each response. A supported image attachment can be sent with the routed query.

## AI Capabilities

- Complexity classification with the low-tier model.
- Low, standard, and high-tier model routing.
- Text and image-assisted answer generation.
- Token and estimated-cost telemetry.

## Key Features

- Chat composer with attachment support.
- Routing status and typing state.
- Markdown response rendering.
- Response limits for large model output.
- Per-model Azure credentials and optional pricing variables.

## Application Workflow

1. The user sends text, optionally with an image attachment.
2. `MainPageViewModel` adds the request and a routing response item.
3. `AzureOpenAIService.SelectModelAsync` uses `gpt-4.1-nano` for classification.
4. The result maps to the low, standard, or high model.
5. The selected model receives text or text-plus-image input.
6. Token counts and cost are calculated and rendered with the response.

## AI Workflow

```text
User Query
    ↓
gpt-4.1-nano Classification
    ↓
LOW / STANDARD / HIGH
    ↓
Selected Azure OpenAI Model
    ↓
Chat Completion
    ↓
Text + Token Usage + Estimated Cost
```

## AI Architecture

- **View:** `MainPage.xaml` and `MainPage.xaml.cs`.
- **ViewModel:** `MainPageViewModel`.
- **Service:** `AzureOpenAIService`.
- **Models:** `AIAssistResponseItem` and `ChatItems`.
- **Support:** `CostCalculator` and `AIAssistTemplateSelector`.

## Syncfusion Controls Used

- `SfAIAssistView`
- `SfMarkdownViewer`
- MAUI `ActivityIndicator`

## Prerequisites

- .NET 10 SDK.
- A .NET MAUI workload for the target platform.
- Azure OpenAI deployments for the configured model identifiers.
- Syncfusion AIAssistView, Core, and MarkdownViewer packages.

## Configuration

### AI Configuration

The service reads shared and per-model environment variables.

| Variable | Purpose |
| --- | --- |
| `AZURE_API_BASE_<MODEL>` | Per-model Azure endpoint |
| `AZURE_API_KEY_<MODEL>` | Per-model Azure API key |
| `AZURE_API_BASE` | Shared endpoint fallback |
| `AZURE_API_KEY` | Shared API-key fallback |
| `AZURE_PRICE_INPUT_<MODEL>` | Optional input price per 1K tokens |
| `AZURE_PRICE_OUTPUT_<MODEL>` | Optional output price per 1K tokens |

The model suffix replaces `.`, `-`, and spaces with `_`. Examples include `GPT_4_1_NANO`, `GPT_4_1_MINI`, and `GPT_5_6_LUNA`.

Without credentials, the service uses placeholder endpoint/key values and live requests fail.

## Getting Started

Run from `Use Case Samples/LLM_Routing`:

```sh
dotnet restore LLM_Routing/LLM_Routing.csproj

dotnet build LLM_Routing/LLM_Routing.csproj -t:Run -f net10.0-android
dotnet build LLM_Routing/LLM_Routing.csproj -t:Run -f net10.0-ios
dotnet build LLM_Routing/LLM_Routing.csproj -t:Run -f net10.0-maccatalyst
dotnet build LLM_Routing/LLM_Routing.csproj -t:Run -f net10.0-windows10.0.19041.0
```

## Project Structure

```text
LLM_Routing/
├── Converters/
├── Models/
├── Platforms/
├── Resources/
├── Services/
├── ViewModels/
├── App.xaml
├── AppShell.xaml
├── MainPage.xaml
└── MauiProgram.cs
```

## Troubleshooting

- **Azure request fails:** Verify shared/per-model endpoint and key variables.
- **No image analysis:** Only the first supported image attachment is read; maximum size is 10 MB.
- **Unexpected route:** The low model must return `LOW`, `STANDARD`, or `HIGH`; unknown values route to low tier.
- **Unexpected cost:** Set per-model price variables to match the Azure deployment billing.

## Responsible AI Considerations

- User prompts and attachments are sent to Azure OpenAI.
- Routing sends the query once to the classifier and again to the selected model.
- Do not include sensitive data in prompts or images.

## Related Documentation

- [Syncfusion AIAssistView for .NET MAUI](https://help.syncfusion.com/maui/aiassistview/getting-started)
- [Syncfusion MarkdownViewer for .NET MAUI](https://help.syncfusion.com/maui/markdownviewer/getting-started)
- [Azure OpenAI Documentation](https://learn.microsoft.com/azure/ai-services/openai/)

## Supported Platforms

- Android
- iOS
- macOS (Mac Catalyst)
- Windows

## License

Syncfusion controls require a valid Syncfusion license for production use.
