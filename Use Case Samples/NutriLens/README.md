# NutriLens

An AI-assisted .NET MAUI food-label analysis sample that extracts label text, analyzes ingredients, stores scan history, and presents trends.

## Overview

Users capture or select a product image, review extracted ingredients, and receive a structured Azure OpenAI assessment personalized with saved dietary preferences.

## AI Capabilities

### Label Image Extraction
- `IngredientImageExtractionService` sends the label photo to the Azure OpenAI deployment as a base64 image with `detail` set to `high`. The prompt asks the model to read the label edge to edge, so text near borders and corners is not skipped.
- The prompt asks for two labeled sections only: `Ingredients` (the full statement in printed order, with sub-ingredients kept under their parent) and `Nutrition Facts` (serving size, servings per container, calories, and every nutrient row with its amount and % Daily Value).
- The model is told not to invent or estimate values. Unreadable characters are marked `[unclear]`, and a section that is not visible is returned as `Not visible`. This gives the user a faithful transcript instead of a guess.
- The returned text is not analyzed at this step. The user reviews it first, which keeps a bad read from turning into a wrong score.

### Personalized Ingredient Analysis
- `AzureOpenAIIngredientService` sends the reviewed ingredient list to the model with a system prompt that says to return only valid JSON, not to invent facts, and to use empty arrays for missing values.
- The prompt includes the saved profile from `UserDietaryPreference`: dietary goals, allergies and preferences, and health considerations. If no profile is saved, the prompt asks for a standard neutral analysis.
- The prompt has a rule for each profile option. Reduce Sugar flags sugars, syrups, and sweeteners. High Protein highlights protein. Low Carb flags high-carb ingredients. Heart Health looks at saturated fat, trans fat, and sodium. Nut-free, Gluten-free, Vegan, and Dairy-free look for the matching ingredients. Diabetes focuses on glycemic impact, and Hypertension focuses on sodium and blood pressure.
- The model must return one JSON object that maps to `IngredientAnalysisResult`. It holds a 0-100 `score`, `scoreExplanation`, `confidence`, `recommendation`, `summary`, positive and negative attributes, `metabolicConflict`, alternative recommendations, allergens, artificial colors, additives, preservatives, nutrition values, and a per-ingredient `ingredientBreakdown`.
- Each item in `ingredientBreakdown` has a name, purpose, `riskLevel` (High, Moderate, Low, or Unknown), and a short explanation.

### Response Validation
- The service accepts the reply as plain text or as an array of text parts, and removes a Markdown code fence if the model adds one.
- The score is clamped to 0-100. The app then sets the category from the score, so it does not trust the model's `category` text: 80 and above is Excellent, 60 and above is Good, 40 and above is Moderate, 20 and above is Poor, and anything lower is Unknown.
- Missing lists are replaced with empty lists and missing nutrition data with an empty `NutritionInfo`, so the result pages do not fail on incomplete JSON.
- An empty, filtered, or malformed reply and an HTTP error each raise a specific error message. The review page shows that message in an "Analysis Error" alert.

### Daily Nutrition Insight
- `DailyInsightGenerator` asks the model for a short insight of at most two sentences, using `AzureOpenAIChatService`. The system prompt asks for plain text with no Markdown, no bullets, and no preamble.
- The prompt is built from the user's saved scans: scan count, average score, healthiest and least healthy product, the last five products, the three most frequently flagged High or Moderate risk ingredients, and average protein and sugar per serving. The insight can therefore name real products and scores.
- If there are no scans, the prompt asks for a welcoming insight that encourages the first scan.
- The prompt uses a fixed placeholder identity ("Alex") with a goal of reducing sugar and a health focus of diabetes-friendly eating. It does not read the saved profile.

### Insight Caching and Fallbacks
- A successful insight is cached for the day in `Preferences` with the source `ai`. Later dashboard visits on the same day reuse it and make no new AI call.
- If the AI call fails or returns no text, the generator returns a fixed fallback sentence about fiber and lean protein. This fallback is not cached, so the next dashboard visit tries the AI again.
- If the generator itself throws, the dashboard shows a second fixed sentence about high-fiber whole foods and pairing carbohydrates with protein.
- Sample products (Oats, Yogurt, Chips) never call the AI. They open predefined analyses from `SampleIngredientAnalysisService`.

## Key Features

### Scanning a Label
`ScanIngredientsPage` is the entry point for a new analysis.
- The **SCAN** button captures a photo with the camera on Android and iOS. The gallery button picks an existing image.
- On Windows and Mac Catalyst, both actions open a file picker for an image file instead of the camera.
- The chosen image is copied to the app cache so it stays available after the picker closes. The page shows it with a scan-frame overlay.
- An `SfBusyIndicator` in the status pill runs while text is being extracted, and a status message shows the progress and any error. A flash toggle button is on the page, but it only changes its own icon.

### Reviewing Extracted Ingredients
`ReviewIngredientsPage` lets the user correct the text before it is analyzed.
- The extracted text is split locally into ingredient items, and the nutrition rows are added as items too. Each item shows a name and a description, and the page shows a count such as "5 Components Found".
- Each item has a delete button. An `SfPopup` form with two `SfTextInputLayout` fields adds a missing ingredient. The form blocks empty names and duplicates.
- **Retake** returns to the scan page. **Analyze Now** sends the list with the saved profile to the AI. An overlay shows the status while the request runs, and a message appears if the list is empty.
- On desktop or a wide window (above 900 px), the scanned image appears in a panel next to the ingredient list.

### Scan Result Page
`AnalyzeIngredientsResultPage` shows the analysis.
- An `SfRadialGauge` with a `RangePointer` shows the 0-100 score, with the score and the category in the center.
- The page shows the profile used, the recommendation, and the summary. It also shows a Positive Attributes card and a Metabolic Conflict card.
- An Ingredient Breakdown list shows each ingredient with its purpose, explanation, and a risk badge. The badge colors are red for High, amber for Moderate, green for Low, and gray for Unknown.
- A disclaimer states that the analysis is for education and is not medical advice.
- **Save Result** stores the scan and opens History. On phones the cards stack in one column. On larger screens they use two columns.

### Dashboard
- The dashboard shows a greeting based on the time of day. The name "Alex" is fixed in code.
- A **Scan an Item** card starts a scan, and a **DAILY AI INSIGHT** card shows the insight.
- An `SfListView` lists the three newest scans with image, score, rating, and time. Tapping one opens its analysis. **VIEW ALL** opens History.
- Phones have a bottom navigation bar with Home, History, Trends, Profile, and a central SCAN button. Desktop layouts use a header navigation bar instead.

### Scan History
- `HistoryPage` lists saved scans and the three bundled sample scans (Yogurt, Chips, Oats) in an `SfListView`, newest first. Each row shows the product image, name, scan time, and a circular score ring (`SfCircularProgressBar`).
- An `SfTextInputLayout` search box filters by product name. An `SfChipGroup` of choice chips filters by date: All Scans, Today, Yesterday, Last Week, and Last Month. A line shows how many scans match.
- Scans are saved to `scan_history.json` in the app data directory. The label image is copied to a `scan-images` folder. Saving the same product with the same score again updates the existing entry.
- The list reloads each time the page appears, so a new scan shows at once. Tapping a row opens the saved analysis.

### Health Trends
- `TrendsPage` has a 7D, 30D, and 90D period selector. If no scan falls in the chosen period, it uses all scans.
- An `SfCartesianChart` with a `LineSeries` plots the score of up to the 12 latest scans, with markers and tooltips. The page also shows the average score with a percentage change between the older and newer halves of the period.
- Protein Intake and Sugar Intake cards use `SfLinearProgressBar` to show the average per serving, based on the nutrition values in saved scans.
- A Frequent Additives card lists the three most common additives or preservatives and the share of scans that contain them. If none were recorded, it lists ingredients rated High risk.
- This data is computed locally from the saved results. The trend page makes no AI call.

### Profile and Preferences
- `ProfilePage` has `SfChipGroup` filter chips for dietary goals (Reduce Sugar, High Protein, Low Carb, Heart Health) and for allergies and preferences (Nut-Free, Gluten-Free, Vegan, Dairy-Free).
- Two `SfSwitch` controls turn on Diabetes and Hypertension considerations.
- Every change is saved at once to `Preferences` on the device and is used in the next analysis. The page text says health details stay on the device, although the selected options are sent to Azure OpenAI in the analysis prompt.
- The user can choose a profile photo, which an `SfAvatarView` shows. The avatar shows the initials "Alex" when no photo is set.
- **SIGN OUT** asks for confirmation and returns to the dashboard. It does not clear any data.

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
