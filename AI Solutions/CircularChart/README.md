# AI-Powered-Natural-Language-Grouping-in-Circular-Chart
The AI-Powered Natural Language Grouping feature in Syncfusion .NET MAUI SfCircularChart enables users to reorganize and summarize doughnut chart data using simple conversational prompts. 

## Configuration

The sample calls Azure OpenAI to turn a prompt into a grouping. Set these three values in `SmartCircularChart/Services/AzureBaseService.cs` before you run it:

- `Endpoint`: the endpoint from the Azure portal, for example `https://<resource-name>.openai.azure.com/`.
- `DeploymentName`: the name of your chat model deployment.
- `ApiKey`: a key from the same Azure OpenAI resource.

Do not commit your key. Client-side keys are for local experiments only.

## How it works

1. `MainPage` passes the prompt to `CircularChartViewModel`.
2. `AzureOpenAIGroupingExtractionService` sends the prompt and the current chart data to Azure OpenAI. It returns a `GroupingResponse` with `groupMode` (`Value`, `Percentage` or `Angle`) and `groupTo`.
3. `GroupingResponseValidator` checks the response.
4. The view model sets `GroupMode` and `GroupTo`. The doughnut series binds to them and merges the small slices into "Others".

If the request fails, the chart stays unchanged and the status line under the prompt shows the reason.
