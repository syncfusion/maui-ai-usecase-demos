# SmartDailyDecisionCompanion

SmartDailyDecisionCompanion is a .NET MAUI sample app that helps users prioritize daily work with a simple AI-assisted decision experience. It includes mobile and desktop layouts, task categorization, and short AI-generated insights that support choosing what to do today.

## Features
- Mobile-first UI for Android and iOS
- Desktop layout for Windows and MacCatalyst
- Shared MVVM design with a single viewmodel across layouts
- Task lists for Do Today, Postpone, and Watch
- AI-powered insights for:
  - Mentorship guidance
  - Urgency
  - Importance
  - Positive momentum
  - Delay risk
  - Focus estimate
- Add Task popup with Topic, Description, Deadline, Project Type, and Priority
- Accept and Postpone recommendation actions
- Accessibility improvements and shared list item templates
- Stable AI insight loading with stale-response protection

## Prerequisites
- .NET SDK compatible with the solution (.NET 10.0 or later)
- .NET MAUI workload installed (`dotnet workload install maui`)
- A Syncfusion license (free community license available for qualifying individuals and small businesses)
- An active **Azure OpenAI** resource with a deployed chat model

## Azure OpenAI Setup

The sample ships with placeholder credentials, so **AI features will not work until you replace them**.

**1. Create the Azure OpenAI resource and deployment**

Sign in to the [Azure Portal](https://portal.azure.com), create an **Azure OpenAI** resource, and deploy a chat completion model (for example `gpt-4o-mini`).

**2. Copy the three values you need**

| Value | Where to find it |
|---|---|
| Endpoint | On the resource's **Overview** page ("Endpoint"), or **Keys and Endpoint** → Endpoint. It must end with a trailing `/`. |
| Deployment name | The name you gave the model when you deployed it (not the model name). |
| API key | On the **Keys and Endpoint** page ("KEY 1" / "KEY 2"). |

**3. Paste them into the sample**

Open `SmartDailyDecisionCompanion/Services/AzureBaseService.cs` and replace the three placeholder constants near the top of the file:

```csharp
// TODO: Add your Azure OpenAI endpoint, deployment name, and API key before running the sample.
private const string endpoint      = "https://<your-resource>.openai.azure.com/";
private const string deploymentName = "<your-deployment-name>";
private const string key           = "<your-api-key>";
```

**4. Register your Syncfusion license**

Open `SmartDailyDecisionCompanion/MauiProgram.cs` and uncomment the registration call:

```csharp
Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("YOUR_LICENSE_KEY");
```

If the credentials are missing or rejected, the app validates them on startup and shows an alert instead of silently failing. The check is performed once per app launch from `MainPage.OnAppearing`, which awaits `MainViewModel.InitializeAsync()`.

## Quick Start
Build:
- dotnet restore
- dotnet build SmartDailyDecisionCompanion.slnx

Run:
- Use the target platform that matches your environment, such as Android, iOS Simulator, Windows, or MacCatalyst.

## How to Use
1. Open the app. The Today tab lists tasks grouped as Do Today, Postpone, or Watch, with an AI-generated pressure summary and focus estimate.
2. Tap the floating + button to add a new task.
3. Tap any task to open the Insights tab and read the AI guidance for it (mentorship, urgency, importance, momentum, and delay risk).
4. From the Insights tab, tap **Accept Recommendation** to move the selected task to the top of Do Today, or **Postpone** to move it to the top of Postpone.
5. Either action applies the change and returns you to the Today tab, where the updated lists and recalculated focus estimate are shown.

## Project Structure
- SmartDailyDecisionCompanion/MainPage.xaml — main UI for mobile and desktop layouts
- SmartDailyDecisionCompanion/ViewModels/MainViewModel.cs — task state, categorization, AI analysis, and commands
- SmartDailyDecisionCompanion/Models/TaskItem.cs — task model
- SmartDailyDecisionCompanion/Services/IAzureAIService.cs — AI service contract
- SmartDailyDecisionCompanion/Services/AzureBaseService.cs — Azure OpenAI transport and credential validation
- SmartDailyDecisionCompanion/Resources/Styles/Styles.xaml — shared styles and the reusable task item templates

## Notes
- `IAzureAIService` is registered in `MauiProgram.cs` and injected into `MainViewModel`, so the transport can be swapped for a different backend without touching the viewmodel.
- Azure OpenAI is called over `HttpClient` directly, so no Azure SDK package is required.
- Project Type and Priority are freeform inputs in the current sample.
- Mentorship and the other insight fields ignore stale async responses, so rapid task selection stays stable.
- The five per-task insight prompts (mentorship, urgency, importance, momentum, delay risk) ask for plain text and are parsed as plain text, with a fenced code block or surrounding quotes stripped before display. The whole-list analyses behave differently by design: pressure requests a small JSON schema and parses it, while recommendation asks for bare task topics and parses them line by line.

## Key Highlights
- Clean responsive MAUI UI with separate mobile and desktop experiences
- Shared viewmodel keeps task state consistent across layouts
- AI-assisted recommendations help users decide what to do next
- Insight cards summarize urgency, importance, momentum, and delay risk
- Task rows are defined once per category as keyed `DataTemplate` resources and reused by both layouts
- Credential validation is awaited explicitly rather than fired from a constructor

## Troubleshooting
- If a "missing or incorrect" alert appears, the endpoint, deployment name, or API key in `Services/AzureBaseService.cs` is still a placeholder, or Azure rejected it.
- If insights stay on Analyzing, verify the endpoint includes the trailing slash and that the deployment name matches your deployed model.
- If AI responses are empty, confirm the network connection and that the Azure OpenAI resource is reachable from your machine.
- If a task selection seems stale, reselect the task to refresh the insight cards.
- If the app shows a Syncfusion license warning, register your key in `MauiProgram.cs` as described above.
- If build errors appear, restore packages and ensure the correct .NET MAUI workload is installed.

## License
Syncfusion controls are subject to the [Syncfusion License](https://www.syncfusion.com/sales/licensing). A free community license is available for qualifying individuals and small businesses.