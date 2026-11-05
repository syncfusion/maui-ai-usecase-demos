# NutriLens

Smart food-label scanning for personalized ingredient analysis with .NET MAUI and Azure OpenAI.

## Overview

NutriLens helps people quickly understand what is in a packaged food product by scanning a label image and turning the extracted ingredients into an AI-generated nutrition assessment.

- **What the sample does:** lets the user capture or attach a product label, extract the text, analyze ingredients, and review a score, warnings, and nutrition guidance.
- **Who it is for:** health-conscious shoppers, people managing dietary restrictions, and anyone who wants a faster way to review packaged food labels.
- **What AI capability it uses:** Azure OpenAI for ingredient analysis and personalized insight generation.
- **What outcome the user gets:** a clear, actionable summary of product risk, ingredient concerns, and practical recommendations based on the user's profile and saved scan history.

## Key Features

- Image-based food-label capture and ingredient extraction
- AI-generated ingredient risk summary with product score
- Personalized insights based on allergies, dietary goals, and health flags
- Scan history, trends, and recent results for comparison over time

## AI Flow

1. User captures a food-label image or selects an existing image.
2. The app extracts text from the label and prepares the user's dietary profile and scan context.
3. Azure OpenAI processes the prompt and returns an ingredient analysis response.
4. The app interprets the response into score, warnings, and nutrition details.
5. The result is rendered in the review and analysis UI.
6. If the AI response is unavailable, the app falls back to local sample behavior or cached history where applicable.

## Controls Used

- **SfListView** — displays recent scans, history, and frequent ingredients.
- **SfCharts** — visualizes health-score trends and summary data.
- **SfGauges** / **SfProgressBar** — shows score and progress-style status indicators.
- **SfButtons** — supports scan, save, and navigation actions.
- **SfPopup** — provides in-app dialogs for editing or reviewing ingredient content.

## Architecture

NutriLens follows an MVVM-based structure with separate pages, view models, services, and models.

- **View / Page** — presents the dashboard, scan flow, review screen, result view, profile, and trends pages.
- **ViewModel** — coordinates UI state, user actions, and AI requests.
- **AI service** — sends prompts to Azure OpenAI and returns structured analysis responses.
- **Models** — represent ingredient analysis, user preferences, and scan history data.
- **Supporting UI components** — Syncfusion controls render lists, charts, progress, popups, and navigation elements.

## Prerequisites

- .NET SDK 10.0
- .NET MAUI workload
- Visual Studio 2022 17.12+ or a compatible .NET MAUI development environment
- Syncfusion .NET MAUI packages and a valid Syncfusion license
- Azure OpenAI resource, deployment, and API credentials

## AI Configuration

NutriLens is configured to use Azure OpenAI for label analysis and daily insights. The app expects an Azure OpenAI endpoint, deployment name, and API key to be supplied through the sample configuration or environment variables.

### Default Behavior

- **Primary mode:** real cloud AI service
- **Fallback behavior:** if AI configuration is missing or unavailable, the sample can continue with local cached content or sample data where applicable

### Environment Variables / Secrets

| Variable | Required | Description |
| --- | --- | --- |
| `NUTRILENS_AZURE_OPENAI_ENDPOINT` | yes | Azure OpenAI resource endpoint |
| `NUTRILENS_AZURE_OPENAI_DEPLOYMENT` | yes | Deployed chat model name |
| `NUTRILENS_AZURE_OPENAI_API_KEY` | yes | Azure OpenAI API key |
| `NUTRILENS_AZURE_OPENAI_TIMEOUT_SECONDS` | no | Optional request timeout |

**Notes**

- Do not commit keys to source control.
- Use environment variables, user secrets, or secure storage.
- If configuration is missing, the app should fall back to local sample behavior or cached data when possible.

## Run

```bash
dotnet build -t:Run -f net10.0-windows10.0.19041.0
```

If you want to target another supported platform, use the matching framework name for that platform in the same command.

## Output

Add a screenshot, GIF, or short visual summary of the final UI here.

## Project Structure

- `Views/` — UI pages and layouts
- `ViewModels/` — presentation logic
- `Models/` — data structures
- `Services/` — AI and helper services
- `Resources/` — styles, images, and assets

## Troubleshooting

### Issue 1: Missing AI configuration
Cause: Required environment variables or secrets are not set.
Fix: Add the required configuration values and restart the app.

### Issue 2: Build or workload errors
Cause: Missing SDK, workload, or incompatible package version.
Fix: Verify the required .NET version and install the needed MAUI workload.

### Issue 3: AI response is unexpected
Cause: Prompt, input, or service configuration may be incomplete.
Fix: Check the prompt/context being sent and confirm the AI settings.

## Security and Privacy Notes

- Do not hard-code API keys.
- Avoid sending unnecessary sensitive data to the AI service.
- Minimize prompt content to only what is needed.
- Prefer secure storage for credentials.
- Review any external API calls for data exposure.

## Supported Platforms

- Android
- iOS
- Windows
- macOS

## License

Add the repository license or vendor disclaimer here.