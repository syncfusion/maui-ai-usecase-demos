# AI-Powered Multi-Select ComboBox in .NET MAUI

This sample demonstrates AI-driven selection with the Syncfusion .NET MAUI ComboBox. The application integrates Azure OpenAI to understand a natural-language request and automatically select the most relevant fruits in a multi-select ComboBox.

## Overview

Multi-select experiences often require users to identify multiple items from a long list. This sample makes that process smarter by allowing the user to describe their intent in plain language, while the AI maps the request to the available fruit list and updates the ComboBox selection automatically.

## Key Features

- Syncfusion .NET MAUI ComboBox integration
- Azure OpenAI-powered intent-based selection
- AI matching against a fixed fruit menu
- Up to 10 matching fruit selections per response
- Natural-language request understanding
- Loading, error, clear, and selection states in the UI
- Checkbox-based dropdown rows with token-style selected values
- Cross-platform .NET MAUI application

## Sample

```xaml
<editors:SfComboBox x:Name="comboBox"
                    WidthRequest="500"
                    SelectionMode="Multiple"
                    IsEditable="False"
                    IsFilteringEnabled="False"
                    DisplayMemberPath="Name"
                    TextMemberPath="Name"
                    Placeholder="{Binding ComboHint}"
                    MultiSelectionDisplayMode="Token"
                    TokensWrapMode="Wrap"
                    ItemsSource="{Binding Fruits}"
                    SelectedItems="{Binding SelectedItems, Mode=TwoWay}">
    <editors:SfComboBox.ItemTemplate>
        <DataTemplate x:DataType="local:FruitModel">
            <Grid ColumnDefinitions="0.9*,0.1*">
                <Label Text="{Binding Name}" />
                <CheckBox IsChecked="{Binding IsSelected, Mode=TwoWay}" />
            </Grid>
        </DataTemplate>
    </editors:SfComboBox.ItemTemplate>
</editors:SfComboBox>
```

## Output

The AI-powered intent-based selection analyzes the user's input and suggests items that best match the intended meaning.

## Prerequisites

Before running the sample, ensure that you have:

- Visual Studio 2022 with .NET MAUI workload installed
- .NET 9 or later
- Azure OpenAI resource and deployment
- Syncfusion .NET MAUI ComboBox package
- Valid Syncfusion license key

## Troubleshooting

### Path Too Long Exception

If you encounter a "Path Too Long" exception while building the project:

1. Close Visual Studio.
2. Rename the repository folder to a shorter name.
3. Delete the `bin`, `obj`, and `.vs` folders.
4. Rebuild the project.

### General Troubleshooting

1. Verify that all components are correctly installed.
2. Check the Azure OpenAI configuration if issues arise during setup or runtime.
3. Monitor error messages carefully, following any suggestions provided by the application.

## License

Syncfusion has no liability for any damage or consequence that may arise from using or viewing the samples. The samples are for demonstrative purposes. If you choose to use or access the samples, you agree to not hold Syncfusion liable, in any form, for any damage related to use, for accessing, or viewing the samples. By accessing, viewing, or using the samples, you acknowledge and agree Syncfusion's samples will not allow you to seek injunctive relief in any form for any claim related to the sample. If you do not agree to this, do not view, access, utilize, or otherwise interact with Syncfusion's samples.