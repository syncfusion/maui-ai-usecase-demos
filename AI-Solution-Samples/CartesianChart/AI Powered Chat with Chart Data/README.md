# AI-powered Chat with Chart Data

A .NET MAUI sample application that combines Syncfusion charts with an AI assistant for chart-related questions.

## Overview

This sample displays laptop manufacturing data using two charts:
- Production Units by year
- Units Sold by year

It also includes an AI assistant powered by Azure OpenAI so users can ask questions about the chart data.

## Features

- .NET MAUI cross-platform app for Android, iOS, Mac Catalyst, and Windows
- Syncfusion Cartesian chart visualizations
- Syncfusion AI AssistView integration
- Chat-based analysis of chart data
- Platform-aware layout for desktop and mobile experiences
- Shared view model for chart data and assistant interactions

## Key Highlights

- Interactive dashboard with two chart types for production and sales trends
- AI assistant that answers questions using the displayed chart data
- Cross-platform experience with platform-aware navigation behavior
- Sample-friendly architecture that is easy to extend for new charts or prompts
- Graceful offline fallback when Azure credentials are not valid

## Project Structure

- App.xaml / AppShell.xaml: application startup and shell navigation
- MainPage.xaml: dashboard with charts and assistant entry point
- MobileAssistViewPage.xaml: mobile assistant page
- ViewModels/ChartViewModel.cs: chart data and assistant command logic
- Services/AzureBaseService.cs: Azure OpenAI client setup and validation
- Services/IAzureAIService.cs: AI service contract
- Models/ChartPoint.cs: chart data model
- Helper/ServiceHelper.cs: lightweight service provider access helper

## Prerequisites

- .NET 10 SDK with .NET MAUI workloads
- Syncfusion MAUI packages referenced by the project
- Azure OpenAI endpoint, deployment name, and API key configured in the sample

## Running the Sample

1. Restore NuGet packages.
2. Build the solution.
3. Run the app on your target platform.
4. Open the AI assistant and ask a question about production or sales.

## Notes

- The sample uses randomly generated chart data for demonstration purposes.
- If Azure credentials are unavailable or invalid, the app shows an alert and continues with offline chart data.
- The assistant is intended for sample/demo usage and should be hardened before production deployment.

## Recommended Improvements

- Move secrets to secure configuration or secure storage
- Expand unit test coverage for the view model and AI service
- Centralize repeated styles and templates into shared resource dictionaries
- Replace any remaining code-behind UI state with bindable view-model state

## Troubleshooting

- If the app shows an alert on startup, verify the Azure endpoint, deployment name, and API key.
- If the assistant returns no response, confirm that Azure OpenAI access is available and the deployment is active.
- If charts do not render correctly, make sure the Syncfusion MAUI packages and fonts are restored successfully.
- If the mobile assistant page does not open, confirm Shell routing and platform-specific navigation behavior.
- If build errors appear after package restore, clean the solution and rebuild the MAUI workloads.

## License

Syncfusion controls are subject to the [Syncfusion License](https://www.syncfusion.com/sales/licensing). A free community license is available for qualifying individuals and small businesses.