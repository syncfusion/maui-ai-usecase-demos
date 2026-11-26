# NutriLens

An AI-assisted .NET MAUI food-label analysis sample that extracts label text, analyzes ingredients, stores scan history, and presents trends.

## Overview

Users capture or select a product image, review extracted ingredients, and receive a structured Azure OpenAI assessment personalized with saved dietary preferences.

## AI Capabilities

- Food-label image text extraction.
- Structured ingredient and nutrition analysis.
- Profile-aware recommendation generation.
- Uncached fallback text for transient daily-insight failures.

## Key Features

- Capture or select a label image.
- Review extracted text and ingredient items.
- View score, category, confidence, allergens, additives, and nutrition.
- Save scan history locally (`scan_history.json` in the app data directory).
- View trends and profile-driven insights.
- Open predefined sample-product analyses.

## Application Workflow

1. `ScanIngredientsViewModel` captures or selects an image.
2. `IngredientImageExtractionService` requests text extraction from Azure OpenAI.
3. `ReviewIngredientsViewModel` presents extracted text and ingredient items.
4. `AzureOpenAIIngredientService` combines reviewed text with `UserDietaryPreference`.
5. The result is rendered by `AnalyzeIngredientsResultPage`.
6. `JsonScanHistoryStore` stores the scan.
7. History and trends read combined saved and sample scans.

## AI Workflow

```text
Label Image
    ↓
Azure OpenAI Image Extraction
    ↓
Reviewed Ingredients and Profile
    ↓
Azure OpenAI Structured Analysis
    ↓
IngredientAnalysisResult
    ↓
Result, History, and Trends
```

## AI Architecture

- **Views:** Dashboard, scan, review, result, history, trends, and profile.
- **ViewModels:** Dashboard, scan, review, history, trends, and profile view models. The result page is code-behind driven.
- **Services:** `AIService/` holds image extraction, ingredient analysis, and chat completion; `Services/` holds the daily-insight generator and cache, image picker, scan history, user preferences, and sample data.
- **Models:** Ingredient, nutrition, profile, history, trend, and cache data.
- **Sample data:** `SampleIngredientAnalysisService` provides static demo products.

## Syncfusion Controls Used

- `SfListView`
- `SfCartesianChart`
- `SfRadialGauge`
- `SfCircularProgressBar` and `SfLinearProgressBar`
- `SfButton`
- `SfChipGroup`
- `SfSwitch`
- `SfPopup`
- `SfTextInputLayout`
- `SfAvatarView`
- `SfBusyIndicator`

## Prerequisites

- .NET 10 SDK.
- .NET MAUI workloads for target platforms.
- Syncfusion MAUI packages referenced by `NutriLens.csproj`.
- Camera or file-picker support (on Windows, the image picker uses a file/gallery picker instead of the camera).
- Azure OpenAI credentials for live extraction and analysis.

## Configuration

### AI Configuration

`AzureOpenAIConfig` reads:

| Variable | Required | Purpose |
| --- | --- | --- |
| `NUTRILENS_AZURE_OPENAI_ENDPOINT` | Live AI | Azure OpenAI endpoint |
| `NUTRILENS_AZURE_OPENAI_DEPLOYMENT` | Live AI | Deployment name |
| `NUTRILENS_AZURE_OPENAI_API_KEY` | Live AI | API key |

The endpoint must be an absolute HTTPS URL, and the deployment must support image input. If a variable is not set, the placeholder constant in `AzureOpenAIConfig.cs` is used, which fails validation until replaced. A Syncfusion license key can be registered in `MauiProgram.cs` (commented line).

## Getting Started

Run from `Use Case Samples/NutriLens`. The iOS and Mac Catalyst targets build only on macOS, and the Windows target only on Windows:

```sh
dotnet restore NutriLens/NutriLens.csproj

dotnet build NutriLens/NutriLens.csproj -t:Run -f net10.0-android
dotnet build NutriLens/NutriLens.csproj -t:Run -f net10.0-ios
dotnet build NutriLens/NutriLens.csproj -t:Run -f net10.0-maccatalyst
dotnet build NutriLens/NutriLens.csproj -t:Run -f net10.0-windows10.0.19041.0
```

## Project Structure

```text
NutriLens/
├── AIService/
├── Helpers/
├── Models/
├── Platforms/
├── Resources/
├── Services/
├── ViewModels/
├── Views/
├── App.xaml
├── MainPage.xaml
└── MauiProgram.cs
```

## Troubleshooting

- **Configuration exception:** Set endpoint, deployment, and API-key variables.
- **Extraction fails:** Verify image quality and vision-capable deployment.
- **Build fails:** Verify .NET 10 SDK and target workloads.
- **Fallback insight appears:** The live daily-insight request failed; the app retries later.

## Responsible AI Considerations

- AI results may be incomplete or inaccurate.
- Do not use results as medical or allergy guidance.
- Images and extracted text are sent to Azure OpenAI in live mode.
- Verify ingredient findings against the physical package.

## Related Documentation

- [.NET MAUI Documentation](https://learn.microsoft.com/dotnet/maui/)
- [Azure OpenAI Documentation](https://learn.microsoft.com/azure/ai-services/openai/)
- [Syncfusion .NET MAUI Controls](https://help.syncfusion.com/maui/introduction/overview)

## Supported Platforms

- Android
- iOS
- macOS (Mac Catalyst)
- Windows

## License

Syncfusion controls require a valid Syncfusion license for production use.
