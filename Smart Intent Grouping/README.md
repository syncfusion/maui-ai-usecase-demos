# AI-Powered Intent-Based Feedback Grouping with .NET MAUI ListView

This sample demonstrates an AI-powered feedback analysis and grouping solution built with **.NET MAUI**, **Syncfusion ListView**, and **Azure OpenAI**. The application analyzes customer feedback and support tickets, understands the user's natural-language grouping request, and automatically organizes records into meaningful groups.

## Overview

Customer feedback and support tickets often contain large volumes of information that are difficult to review manually. This sample uses Azure OpenAI to analyze feedback records and intelligently group them based on user intent.

Users can enter natural language commands such as:

- "group by feedbacks"
- "group by ratings"
- "group by priority"
- "group by month"

The AI engine analyzes the request, processes the feedback dataset, and dynamically organizes records into corresponding groups within a Syncfusion ListView.

The solution helps users:

- Analyze customer feedback faster
- Organize large ticket collections
- Prioritize issues by severity
- Review feedback sentiment
- Identify trends across time periods
- Improve customer support workflows

## Sample

```xaml
<syncfusion:SfListView
    ItemsSource="{Binding Items}"
    IsStickyGroupHeader="True">

    <syncfusion:SfListView.GroupDescriptor>
        <syncfusion:GroupDescriptor PropertyName="GroupKey"/>
    </syncfusion:SfListView.GroupDescriptor>

</syncfusion:SfListView>
```

## Natural Language Grouping

Users can type prompts such as:

```text
group by feedbacks
group by ratings
group by priority
group by month
```

The application interprets the request and automatically groups the feedback items using AI-assisted analysis. 【1-031b7b】

## How It Works

1. The application loads a feedback dataset.
2. Azure OpenAI analyzes the feedback records.
3. Users enter a natural-language grouping request.
4. The AI engine determines the grouping intent.
5. Feedback records are categorized into appropriate groups.
6. Syncfusion ListView displays grouped data with customized headers and summaries. 【1-031b7b】

## Azure OpenAI Integration

The sample uses Azure OpenAI to perform intent analysis and determine how records should be organized. If valid Azure OpenAI credentials are unavailable, the application falls back to a local rule-based analyzer. 【2-2ca05e】【1-031b7b】

## Output

![ListView Intent Grouping](ListView_Windows.gif)

## Requirements to Run the Demo

To run the sample, ensure the following prerequisites are installed:

- Visual Studio 2022
- .NET MAUI workload
- Syncfusion .NET MAUI ListView
- Azure OpenAI Resource
- Azure OpenAI Deployment
- Valid Azure OpenAI API Key and Endpoint

For .NET MAUI requirements, refer to:

https://help.syncfusion.com/maui/system-requirements

## Azure OpenAI Configuration

Configure the Azure OpenAI settings in:

```csharp
Services/AzureBaseService.cs
```

Update the following values:

```csharp
internal const string Endpoint = "Your Endpoint";
internal const string DeploymentName = "Your deployment name";
internal const string Key = "API key";
```

The application validates these settings during startup and displays helpful diagnostic information when credentials are missing or invalid.

## Troubleshooting

### Invalid Azure OpenAI Credentials

If the application reports credential errors:

- Verify the Azure OpenAI Endpoint.
- Verify the Deployment Name.
- Verify the API Key.
- Restart the application after updating the credentials.

The application displays a startup diagnostic banner showing the active endpoint, deployment name, and a safe fingerprint of the configured API key. This information can help identify configuration mismatches between the Azure OpenAI resource and the application settings. If credentials are invalid or missing, the application falls back to its local rule-based analyzer and displays a descriptive error message explaining the issue.

### Path too long exception

If you encounter a "Path Too Long" exception while building the project, close Visual Studio, rename the repository folder to a shorter name, and rebuild the project.

## License

Syncfusion has no liability for any damage or consequence that may arise from using or viewing the samples. The samples are for demonstrative purposes. If you choose to use or access the samples, you agree to not hold Syncfusion liable, in any form, for any damage related to use, for accessing, or viewing the samples. By accessing, viewing, or seeing the samples, you acknowledge and agree Syncfusion's samples will not allow you seek injunctive relief in any form for any claim related to the sample. If you do not agree to this, do not view, access, utilize, or otherwise do anything with Syncfusion's samples.