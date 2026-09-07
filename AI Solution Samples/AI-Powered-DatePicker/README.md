# Build an AI-Powered Smart Date Picker with .NET MAUI and Azure OpenAI

A cross-platform .NET MAUI application that combines natural-language date requests with the **Syncfusion SfDatePicker** and **Azure OpenAI**. Enter requests such as “next Friday” or “the first business day of next month”, and the app resolves the request to a calendar date before displaying it in the native date picker.

## Highlights

- Resolves natural-language date requests with Azure OpenAI
- Supports relative dates, weekdays, recurring holidays, and business-day requests
- Validates AI responses and accepts only unambiguous `yyyy-MM-dd` dates
- Includes a local fallback for common date requests when an AI response cannot be used
- Opens the Syncfusion date picker with the resolved date selected
- Uses MVVM and dependency injection for a clean, testable structure
- Works across iOS, Android, Windows, and macOS

## Prerequisites

- .NET 10.0+
- Visual Studio 2022 (17.12+) or VS Code with the .NET MAUI workload
- Syncfusion .NET MAUI Picker (Community or Commercial license)
- An active Azure OpenAI resource with a deployed model, such as `gpt-5-mini`

## Quick Install

**Clone:**
```bash
git clone https://github.com/syncfusion/maui-ai-usecase-demos
cd "maui-ai-usecase-demos/AI Solution Samples/AI-Powered-DatePicker/Sample"
dotnet build SmartAIDatePicker.slnx
```

**Configure Azure OpenAI credentials** in `Sample/SmartAIDatePicker/AIService/AzureOpenAIService.cs`:
```csharp
private const string BaseEndpoint = "YOUR_AZURE_OPENAI_ENDPOINT";
private const string DeploymentName = "YOUR_DEPLOYMENT_NAME";
private const string ApiKey = "YOUR_API_KEY";
```

Use a secure configuration or secret store for credentials in production. Do not commit API keys to source control.

**Run the app** with the target framework for your platform, for example:
```bash
dotnet build SmartAIDatePicker/SmartAIDatePicker.csproj -f net10.0-windows10.0.19041.0
dotnet run --project SmartAIDatePicker/SmartAIDatePicker.csproj -f net10.0-windows10.0.19041.0
```

## How It Works

1. Enter a natural-language date request in the editor.
2. Select the search button to send the request to Azure OpenAI.
3. The view model validates the response and falls back to local date rules when needed.
4. The resolved date appears in the selected-date card and opens in the Syncfusion date picker.

Example requests include:

- `today`
- `tomorrow`
- `next Friday`
- `two weeks from now`
- `the first business day of next month`
- `last working day of this month`
- `Christmas`
- `Thanksgiving`
- `Diwali`

## Key Components (API)

| Member | Description |
|---|---|
| `DatePickerViewModel.SearchCommand` | Starts date resolution for the text entered by the user |
| `DatePickerViewModel.OpenPickerCommand` | Opens the Syncfusion date picker for manual selection |
| `DatePickerViewModel.GetDateFromAIAsync(request)` | Sends the request to the AI service, validates the result, and applies local fallback rules |
| `DatePickerViewModel.CalculateDate(request)` | Resolves supported common requests locally when the AI result is unavailable or invalid |
| `AzureOpenAIService.GetCompletion(prompt, cancellationToken)` | Sends the date-resolution prompt to Azure OpenAI and returns the model response |
| `MauiProgram.CreateMauiApp()` | Configures Syncfusion MAUI support and registers the AI service and view model with dependency injection |
| `MainPage` | Provides the natural-language editor, selected-date summary, and `SfDatePicker` control |

## Why Use This

- **Natural interaction:** Users can describe dates in everyday language instead of navigating through calendar controls.
- **Reliable results:** Strict prompting, response validation, and local fallback rules prevent vague or invalid responses from changing the selected date.
- **Native experience:** Syncfusion `SfDatePicker` provides a familiar date-selection dialog across supported platforms.
- **Clean architecture:** The AI service is accessed through `IAzureOpenAIService`, allowing the implementation to be replaced or tested independently.
- **Extensible rules:** Additional holidays, relative-date phrases, or calendar conventions can be added in the view model.

## Resources

- Syncfusion .NET MAUI Date Picker docs: https://help.syncfusion.com/maui/datepicker/getting-started
- Azure OpenAI Service: https://learn.microsoft.com/azure/ai-services/openai/overview
- Azure OpenAI .NET client library: https://learn.microsoft.com/dotnet/api/overview/azure/ai.openai-readme
- .NET MAUI: https://dotnet.microsoft.com/apps/maui

## Support

For current Syncfusion customers, the newest version of Essential Studio is available from the [license and downloads page](https://www.syncfusion.com/account/downloads). If you are not yet a customer, you can try the [30-day free trial](https://www.syncfusion.com/downloads/maui) to evaluate the controls used in this sample.

For questions, contact Syncfusion through the [support forums](https://www.syncfusion.com/forums), [feedback portal](https://www.syncfusion.com/feedback/maui), or [support portal](https://support.syncfusion.com).

## Troubleshooting

**Path Too Long Exception**

If you encounter a path too long exception when building this example project, close Visual Studio and rename the repository to a shorter path, then rebuild the project.

**Azure OpenAI request errors**

Confirm that the endpoint uses the Azure OpenAI API base URL, the deployment name matches an available model deployment, and the API key is active. The app will use its local fallback rules only for requests implemented by `CalculateDate`.
