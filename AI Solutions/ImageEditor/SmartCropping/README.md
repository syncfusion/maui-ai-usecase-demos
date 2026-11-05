# Smart Cropping

A .NET MAUI sample that uses **Azure AI Vision Image Analysis** to detect the most important visual content in an image and suggest a smart crop region.

## Overview

This sample demonstrates how AI can help users crop images more accurately by analyzing visual content and proposing a crop focused on the primary subject.

- **What the sample does:** Lets the user open an image, analyze it with Azure AI Vision Image Analysis, preview a suggested crop, and apply or reset the crop.
- **Who it is for:** Developers building image editing or content-aware cropping experiences in .NET MAUI.
- **What AI capability it uses:** Azure AI Vision Image Analysis for object, region, and content understanding.
- **What outcome the user gets:** A faster, guided crop workflow that highlights the most relevant part of the image.

## Key Features

- Open a local image from the device
- Analyze the image with Azure AI Vision Image Analysis
- Preview an AI-suggested crop overlay
- Apply, adjust, reset, and save the cropped result

## AI Flow

1. User opens an image and selects a crop intent.
2. The app captures the image bytes and prepares the analysis request.
3. Azure AI Vision Image Analysis processes the image.
4. The app interprets the response and maps it to a crop region.
5. The crop suggestion is rendered as an overlay in the UI.
6. If needed, the user can retry, reset, or manually adjust the crop.

## Controls Used

- **Syncfusion SfImageEditor** — displays and edits the selected image.
- **Syncfusion SfComboBox** — lets the user choose the crop intent.
- **Button** — triggers open, analyze, apply, reset, and save actions.
- **Border / Grid / StackLayout** — arranges the editor, overlay, and action layout.

## Architecture

The sample is organized around a simple MVVM flow:

- **View / Page** — hosts the editor, overlay, and action buttons.
- **ViewModel** — stores state, commands, and UI workflow logic.
- **AI service** — sends image bytes to Azure AI Vision Image Analysis and returns crop guidance.
- **Models** — carry crop-region and response data between layers.
- **Supporting UI components** — render status, overlay, and editor interactions.

Data flows from the page to the view model, then to the Azure AI Vision service. The returned analysis is converted into a crop region and rendered back in the UI through the image editor controller bridge.

## Prerequisites

- .NET 10 SDK
- .NET MAUI workload
- Visual Studio 2022 with .NET MAUI support, if using Visual Studio
- Syncfusion MAUI packages and a valid Syncfusion license key
- Azure AI Vision endpoint and API key

## AI Configuration

The sample is configured to use Azure AI Vision Image Analysis through environment variables.

### Default Behavior

- **Real cloud AI service** is used when the Azure Vision endpoint and key are configured.
- If configuration is missing or invalid, the sample shows an error when AI analysis is requested.

### Environment Variables / Secrets

| Variable | Required | Description |
| --- | --- | --- |
| `AZURE_VISION_ENDPOINT` | yes | Azure AI Vision endpoint URL |
| `AZURE_VISION_KEY` | yes | Azure AI Vision API key |

### Notes

- Do not commit keys to source control.
- Use environment variables, user secrets, or secure storage.
- If configuration is missing, the app will still open, but AI analysis will fail until valid values are supplied.

## Run

Build and run the sample from Visual Studio or a MAUI-supported .NET SDK environment with the desired platform target selected.

- Windows
- Android
- iOS
- Mac Catalyst

## Output

The final UI shows an image editor with an AI crop suggestion overlay, crop intent selection, and actions to apply, reset, or save the image.

## Project Structure

- `Views/` — UI pages and layouts
- `ViewModel/` — presentation logic
- `Models/` — data structures
- `AIService/` — Azure AI Vision integration
- `Helpers/` — overlay rendering helpers
- `Resources/` — styles, images, and assets

## Troubleshooting

**Issue 1: Missing AI configuration**  
Cause: Required environment variables or secrets are not set.  
Fix: Add `AZURE_VISION_ENDPOINT` and `AZURE_VISION_KEY`, then restart the app.

**Issue 2: Build or workload errors**  
Cause: Missing SDK, workload, or incompatible package version.  
Fix: Verify the required .NET SDK and install the MAUI workload.

**Issue 3: AI response is unexpected**  
Cause: The image may not contain clear subjects, or the analysis request may be incomplete.  
Fix: Try another image or confirm the intent selection and Azure configuration.

## Security and Privacy Notes

- Do not hard-code API keys.
- Avoid sending unnecessary sensitive data to the AI service.
- Minimize prompt or image context to only what is needed.
- Prefer secure storage for credentials.
- Review external API calls for data exposure.

## Supported Platforms

- Android
- iOS
- Windows
- macOS

## License

See the repository license for usage terms.
