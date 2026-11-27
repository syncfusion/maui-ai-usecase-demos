# AI-Powered Dynamic Form Generation in .NET MAUI DataForm

This sample demonstrates AI-powered dynamic form generation with the Syncfusion® .NET MAUI DataForm. The application integrates Azure OpenAI to analyze free-text user input, identify meaningful fields, and automatically generate a fully editable form at runtime.

## Overview

Creating forms manually can be time-consuming when the structure depends on user requirements. This sample uses Azure OpenAI to intelligently interpret unstructured text and transform it into a structured form definition.

The AI-powered form generation engine analyzes:

- Field names
- Field values
- Data types
- Missing field information
- User intent
- Form structure

Based on the AI response, corresponding DataForm editors are created dynamically in the Syncfusion® .NET MAUI DataForm.

For example, if the user enters:

```text
Name John
Age 28
Email john@example.com
Phone
```

The AI generates appropriate DataForm fields for:

- Name
- Age
- Email
- Phone

Fields with missing values remain editable so users can complete the information later.

The generated form is displayed directly in the Syncfusion® .NET MAUI DataForm, allowing users to review and modify the generated content.

## Key Features

- Syncfusion® .NET MAUI DataForm integration
- Azure OpenAI-powered form generation
- Dynamic field creation from natural language input
- Automatic field type detection
- Editable generated forms
- Support for text, number, email, and phone fields
- Intelligent value extraction
- Handling incomplete user data
- Runtime DataForm item generation
- Cross-platform .NET MAUI application

## Sample

```xml
<dataform:SfDataForm
    x:Name="dataForm"
    AutoGenerateItems="False"
    ValidationMode="PropertyChanged"
    LayoutType="TextInputLayout">

    <dataform:SfDataForm.TextInputLayoutSettings>
        <dataform:TextInputLayoutSettings
            ShowHelperText="True"/>
    </dataform:SfDataForm.TextInputLayoutSettings>

</dataform:SfDataForm>
```

## Output

The AI-generated response is converted into DataForm items and rendered dynamically.

Example input:

```text
Name John
Age 28
Email john@example.com
Phone
```

Generated form:

```text
Name     : John
Age      : 28
Email    : john@example.com
Phone    :
```

Users can continue editing the generated fields directly from the DataForm.

## Prerequisites

Before running the sample, ensure that you have:

- Visual Studio 2022 with .NET MAUI workload installed
- .NET 9 or later
- Azure OpenAI resource and deployment
- Syncfusion® .NET MAUI DataForm package
- Valid Syncfusion® license key

## Troubleshooting

### Path Too Long Exception

If you encounter a **"Path Too Long"** exception while building the project:

1. Close Visual Studio.
2. Rename the repository folder to a shorter name.
3. Delete the `bin`, `obj`, and `.vs` folders.
4. Rebuild the project.

### General Troubleshooting

1. Verify that all required SDKs and tools are up to date.
2. Check the Azure OpenAI setup to ensure that all values are correctly configured before running the demo.
3. If any error occurs during runtime, restart the application and give it another try to refresh everything.

## License

Syncfusion has no liability for any damage or consequence that may arise from using or viewing the samples. The samples are for demonstrative purposes. If you choose to use or access the samples, you agree to not hold Syncfusion liable, in any form, for any damage related to use, for accessing, or viewing the samples. By accessing, viewing, or using the samples, you acknowledge and agree Syncfusion's samples will not allow you to seek injunctive relief in any form for any claim related to the sample. If you do not agree to this, do not view, access, utilize, or otherwise interact with Syncfusion's samples.