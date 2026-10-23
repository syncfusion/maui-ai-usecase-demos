# Home Inventory Manager

This sample demonstrates a Home Inventory Manager built with Syncfusion® .NET MAUI controls and a fully mocked AI layer. The app helps users manage household assets, track warranties and AMC contracts, store invoices and documents, monitor asset health, and view maintenance and replacement recommendations without requiring an external AI service.

## Overview

Managing home appliances, electronics, furniture, and other valuable assets can become challenging over time. This sample provides a centralized system for organizing and monitoring household inventory throughout its lifecycle.

The sample helps users:

- Maintain a digital inventory of household assets
- Scan and store invoices and warranty documents
- Track warranty and AMC status
- Monitor asset values
- Receive maintenance reminders
- Predict replacement requirements
- Analyze inventory trends

The AI layer is implemented behind `IAIService` and is mocked in this repository so the demo runs without cloud credentials.

## Controls demonstrated

- Syncfusion® `SfListView`
- Syncfusion® `SfTabView`
- Syncfusion® `SfCharts`
- Syncfusion® `SfCircularProgressBar`
- Syncfusion® `SfButton`
- Syncfusion® `SfTextInputLayout`
- Syncfusion® `SfDatePicker`
- Syncfusion® `SfImageEditor`
- MAUI `Shell` navigation

## Run instructions

1. Open `HomeInventoryManager.slnx` in Visual Studio 2022 with the .NET MAUI workload installed.
2. Restore and build the solution.
3. Select an Android, iOS, Mac Catalyst, or Windows target and run the app.

## Prerequisites

- Visual Studio 2022 with .NET MAUI workload installed
- .NET 10 SDK or later
- Syncfusion® .NET MAUI packages
- Valid Syncfusion® license key

## Troubleshooting

### Path Too Long Exception

If you encounter a **"Path Too Long"** exception while building the project:

1. Close Visual Studio.
2. Rename the repository folder to a shorter name.
3. Delete the `bin`, `obj`, and `.vs` folders.
4. Rebuild the project.

## License

Syncfusion has no liability for any damage or consequence that may arise from using or viewing the samples. The samples are for demonstrative purposes. If you choose to use or access the samples, you agree to not hold Syncfusion liable, in any form, for any damage related to use, for accessing, or viewing the samples.

By accessing, viewing, or using the samples, you acknowledge and agree that Syncfusion's samples will not allow you to seek injunctive relief in any form for any claim related to the sample.

If you do not agree to this, do not view, access, utilize, or otherwise interact with the samples.
