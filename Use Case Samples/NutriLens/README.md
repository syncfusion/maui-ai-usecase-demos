# Build a Smart Food-Label Scanner with .NET MAUI and Azure OpenAI

A cross-platform .NET MAUI app that turns a mobile camera scan or desktop image attachment into a personalized ingredient analysis. It uses **Azure OpenAI** to grade products 0–100, explain every flagged ingredient, and generate a daily nutrition insight tailored to the user's saved scan history.

## Highlights

- Snap a label → AI analysis with per-ingredient risk levels, score, and recommendations
- Daily AI insight that reflects products the user has actually scanned
- Trends page with health-score chart, frequent-additive ranking, and 7D / 30D / 90D windows
- Profile page that captures dietary goals, allergies, and chronic-condition flags and feeds them into the prompt
- Responsive layout: single-column on phones, side-by-side on WinUI / MacCatalyst / wide screens
- MVVM with dependency injection; JSON-file scan history + `Preferences`-backed daily-insight cache
- Works on **iOS, Android, Windows, and macOS**

## Syncfusion controls demonstrated

- **SfListView** — recent scans, history, frequent additives
- **SfCharts** — Trends page health-score spark line
- **SfGauges** / **SfProgressBar** — score rings and progress on result page
- **SfButtons** — scan / save / nav actions
- **SfPopup** — add-ingredient dialog on the Review page

## Technologies used

- .NET MAUI (net10.0)
- Syncfusion .NET MAUI 34.2.6 (Buttons, Charts, Core, Gauges, ListView, Popup, ProgressBar)
- CommunityToolkit.Mvvm 8.4.0
- OpenAI 2.13.0 (used by the image-extraction service)
- Azure OpenAI (chat completions endpoint)

## Features

### Scan flow
1. **Scan** opens the camera on Android/iOS and an image-file picker on Windows/macOS. The image is copied into app cache for extraction and into app data when the analysis is saved.
2. The label image is sent to an **ingredient-extraction** service that returns plain text.
3. The extracted text is sent to **Azure OpenAI** with the user's dietary profile; the response is deserialised into `IngredientAnalysisResult` (score 0–100, per-ingredient risk levels, additives, allergens, nutrition).
4. The user lands on a **Review** page where they can edit the ingredient list before the AI analysis, and an **Analyze Result** page with the full breakdown.
5. Saving the result appends it to the local JSON scan history.

### Dashboard
- Greeting, daily AI insight, and three most recent scans
- Each recent-scan card taps into either the static sample analysis (for `Oats`, `Yogurt`, `Chips`) or the persisted `IngredientAnalysisResult`

### Trends
- 7D / 30D / 90D window selector
- Health-score trend chart, top frequent additives, protein and sugar averages
- Plain-language insight comparing the first half vs. second half of the window

### Profile
- Dietary goals, allergy / preference chips, Diabetes / Hypertension toggles
- Avatar picker; the choices are persisted via `UserPreferenceStore` and influence future AI prompts

## Prerequisites

- .NET 10.0 SDK
- Visual Studio 2022 (17.12+) or VS Code with the .NET MAUI workload
- Syncfusion .NET MAUI components (Community / Commercial)
- An **Azure OpenAI** resource with a deployed chat model

> Daily insights use `max_completion_tokens = 800`; label analysis uses `6000`. Both use `reasoning_effort = "minimal"` for reasoning deployments such as `gpt-5-mini`.

## Quick install

**Clone:**

```bash
git clone https://github.com/syncfusion/maui-ai-usecase-demos
cd "Use Case Samples/NutriLens"
```

**Configure Azure OpenAI credentials** in `NutriLens/AIService/AzureOpenAIConfig.cs`, or provide `NUTRILENS_AZURE_OPENAI_ENDPOINT`, `NUTRILENS_AZURE_OPENAI_DEPLOYMENT`, and `NUTRILENS_AZURE_OPENAI_API_KEY` in the app process environment:

```csharp
public const string BaseEndpoint   = "https://YOUR-RESOURCE.openai.azure.com/";
public const string DeploymentName = "YOUR-DEPLOYMENT-NAME";
public const string ApiKey         = "YOUR-AZURE-OPENAI-API-KEY";
```

> The values shipped in the repo are **placeholders** — no real key is committed. The service appends Azure's `/openai/v1/chat/completions` route to a resource-root endpoint. Never commit a real key; production mobile apps should call a trusted backend instead of embedding an Azure key in the client.

**Register Syncfusion license** in `NutriLens/MauiProgram.cs` (uncomment and add your key):

```csharp
Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("YOUR_LICENSE_KEY");
```

**Run:**

```bash
dotnet build -t:Run -f net10.0-windows10.0.19041.0
# or
dotnet build -t:Run -f net10.0-android
```

## How it works

### Project layout

| Folder | Purpose |
|---|---|
| `NutriLens/AIService/` | Azure OpenAI clients + the OpenAI image-extraction service |
| `NutriLens/Services/` | Persistence, history, sample data, image picker, daily-insight pipeline |
| `NutriLens/ViewModels/` | MVVM view models (one per page) |
| `NutriLens/Views/` | ContentPages + desktop header navigation |
| `NutriLens/Models/` | DTOs and small view models |
| `NutriLens/Helpers/` | `AppNavigator`, value converters, `AnalysisNavigationData` handoff slot |

### Navigation

The app uses a single `NavigationPage` rooted at `NutriLensDashboardPage`. Top-level navigation returns to the dashboard before opening another section, while review and analysis pages are pushed for the scan flow. There is no Shell — `AppNavigator` resolves the root `NavigationPage` from `Application.Current.Windows[0].Page`.

### Scan → Review → Analyze → Save

1. `ScanIngredientsViewModel` calls `IImagePickerService` to capture on mobile or attach an image file on desktop.
2. `IIngredientImageExtractionService.ExtractContentAsync` returns the raw text.
3. Both the image and the text are stashed in the static `AnalysisNavigationData.PendingReview` slot (a short-lived handoff bucket; read once and cleared by the Review page).
4. `ReviewIngredientsViewModel.AnalyzeNowAsync` calls `IAzureOpenAIIngredientService.AnalyzeAsync(text, preferences)`.
5. The result is pushed to `AnalyzeIngredientsResultPage` and saved through `IScanHistoryStore.AddAsync`.

### Daily insight pipeline

`DailyInsightGenerator` is the only consumer of `IAzureOpenAIChatService`. It:

1. Looks for today's cached entry; only `Source = "ai"` entries count (fallback text is never cached, so transient failures retry on the next dashboard visit).
2. Builds a context from the user's saved scan history — total scans, average score, healthiest / least healthy product, top three flagged ingredients, and protein / sugar averages.
3. Calls Azure OpenAI with the context and a 2-sentence response instruction.
4. Caches the AI result and returns it. A blank / failed response falls back to a one-liner for that single appearance.

## Troubleshooting

### Desktop image selection

On Windows and MacCatalyst, **Scan** opens the system image-file picker. Mobile camera capture uses the native platform permissions declared in each platform manifest.

### Empty AI response on a reasoning model

If your deployment is a reasoning model (e.g. `gpt-5-mini`), a small `max_completion_tokens` value can be consumed by the internal reasoning step and return empty content. The sample uses 800 tokens for daily insights and 6000 for ingredient analysis, with `reasoning_effort = "minimal"`.

### Cached fallback stuck all day

If the daily insight looks stale, the cache may have been seeded with a non-AI entry. Clear app data or uninstall — the cache is intentionally stored in `Preferences` and has no remote invalidation.

### Path too long exception

If the build fails with a path-too-long error, close Visual Studio and rename the repository to a shorter path, then rebuild.

## Documentation

- [Syncfusion .NET MAUI ListView](https://help.syncfusion.com/maui/listview/getting-started)
- [Syncfusion .NET MAUI Charts](https://help.syncfusion.com/maui/charts/getting-started)
- [Syncfusion .NET MAUI Buttons](https://help.syncfusion.com/maui/buttons/getting-started)
- [Azure OpenAI Service](https://learn.microsoft.com/en-us/azure/ai-services/openai/overview)
- [.NET MAUI](https://learn.microsoft.com/dotnet/maui/)