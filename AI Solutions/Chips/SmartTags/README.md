# AI-Powered Tag Generator in .NET MAUI

This sample demonstrates AI-powered tag generation with the Syncfusion .NET MAUI Chips control. The application integrates Azure OpenAI to turn a natural-language description into relevant, context-aware tags that can be displayed as clean chip items.

## Overview

Tag generation is useful in content workflows, feature planning, and product discovery scenarios where users describe intent and need a short, structured set of keywords. This sample uses Azure OpenAI to interpret the description and return only tags from a curated vocabulary, so the results remain accurate, consistent, and UI-friendly.

The AI-generated tags are displayed as chips with a polished loading state and a wrap layout that supports multiple rows without clipping. This makes the sample useful as a reference for smart labeling, content classification, and metadata suggestions inside MAUI applications.

## Key Features

- Syncfusion .NET MAUI Chips integration
- Azure OpenAI-powered tag generation
- Vocabulary-constrained output for valid chip labels
- Case-insensitive parsing with canonical tag names and duplicate removal
- Up to 12 accepted tags per response
- Natural-language input for feature and content descriptions
- Loading, empty, and results states in the UI
- Wrapping chip layout for responsive display
- Clear and regenerate workflow for repeated prompts
- Cross-platform .NET MAUI application

## Sample

```xaml
<syncfusion:SfChipGroup Grid.Row="0"
    x:Name="chipsControl"
    ItemsSource="{Binding Tags}"
    DisplayMemberPath="Name"
    ChipType="Action"
    ChipBackground="{StaticResource BrandAccent}"
    ChipTextColor="White"
    HorizontalOptions="Fill"
    VerticalOptions="Start">
    <syncfusion:SfChipGroup.ChipLayout>
        <FlexLayout Direction="Row"
                    Wrap="Wrap"
                    JustifyContent="Start"
                    AlignItems="Start"/>
    </syncfusion:SfChipGroup.ChipLayout>
</syncfusion:SfChipGroup>
```

## AI integration

`AzureOpenAITagService` uses Azure.AI.OpenAI 2.1.0. It creates an `AzureOpenAIClient`, gets the configured chat deployment, and sends system and user messages with `CompleteChatAsync`. Cancellation is passed through to the SDK. Authentication, missing-deployment, and empty-response errors are surfaced to the view model and shown in the UI.

The system prompt constrains the model to `TagVocabulary`. The view model then validates each returned line against that vocabulary, canonicalizes names, removes duplicates, and limits the displayed results to 12 tags.

## Output

The AI-generated response is converted into tag items and rendered dynamically.

## Requirements to run the demo

To run the demo, refer to https://help.syncfusion.com/maui/system-requirements

## Prerequisites

Before running the sample, ensure that you have:

- Visual Studio 2022 with .NET MAUI workload installed
- .NET 9 or later
- Azure OpenAI resource and deployment
- Syncfusion .NET MAUI Chips package
- Valid Syncfusion license key

## Troubleshooting

### Path too long exception

If you encounter a "Path Too Long" exception while building the project, close Visual Studio, rename the repository folder to a shorter name, and rebuild the project.

### General Troubleshooting

1. Make sure all necessary SDKs and tools are up to date.
2. Double-check the Azure OpenAI setup to ensure that all values are correctly configured.
3. In case of any errors during execution, review the error messages displayed in the application for guidance.

## License

Syncfusion has no liability for any damage or consequence that may arise from using or viewing the samples. The samples are for demonstrative purposes. If you choose to use or access the samples, you agree to not hold Syncfusion liable, in any form, for any damage related to use, for accessing, or viewing the samples. By accessing, viewing, or using the samples, you acknowledge and agree Syncfusion's samples will not allow you to seek injunctive relief in any form for any claim related to the sample. If you do not agree to this, do not view, access, utilize, or otherwise interact with Syncfusion's samples.