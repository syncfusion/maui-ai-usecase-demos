# AI-Powered Feedback in .NET MAUI Rating

This sample demonstrates AI-powered customer support feedback rating in a .NET MAUI application. It uses Azure OpenAI to analyze written feedback and suggest a satisfaction rating displayed with the Syncfusion .NET MAUI Rating control.

## Overview

Customer feedback often describes a support experience in natural language rather than as a numeric score. This sample evaluates the feedback sentiment and aspects such as issue resolution, response time, and helpfulness, then suggests a rating from 0 to 5.

The rating updates as the user enters feedback. A brief typing pause prevents unnecessary AI requests while the user is still writing. If Azure OpenAI is not configured, the app shows a "Configuration Required" alert at startup so the cause is obvious.

## Key Features

   - Syncfusion .NET MAUI Rating control integration
   - Azure OpenAI-powered feedback analysis
   - Suggested satisfaction rating from 0.0 to 5.0
   - Rating updates as feedback is entered
   - Debounced requests while typing
   - Request-version guard so a slow AI response cannot overwrite a newer one
   - Loading indicator and visible error message while feedback is being analyzed
   - Cross-platform .NET MAUI application

## Sample

```xaml
<inputLayout:SfTextInputLayout CharMaxLength="300"
                               ShowCharCount="True">
    <Editor Placeholder="Describe your customer support experience..."
            TextChanged="OnFeedbackTextChanged" />
</inputLayout:SfTextInputLayout>

<rating:SfRating ItemCount="5"
                 ItemSize="32"
                 Value="{Binding Rating, Mode=OneWay}"
                 IsReadOnly="True" />
```

The Rating control is bound to `MainViewModel.Rating`. `MainViewModel` owns the debounce, the per-request cancellation token, and the request-version guard.

## Output

![AI Rating Feedback](AIRatingFeedback.gif)

## Requirements to run the demo

To run the demo, refer to https://help.syncfusion.com/maui/system-requirements

## Prerequisites

Before running the sample, ensure that you have:

- Visual Studio with .NET MAUI workload installed
- .NET 10 SDK or later
- Azure OpenAI resource and chat deployment
- Valid Syncfusion license key (community licence is acceptable for evaluation)

### Azure OpenAI credentials

Set the following environment variables before launching the app:

| Variable | Description |
|----------|-------------|
| `AZURE_OPENAI_ENDPOINT` | Your Azure OpenAI endpoint, e.g. `https://your-resource.openai.azure.com/` |
| `AZURE_OPENAI_DEPLOYMENT_NAME` | Name of the chat deployment |
| `AZURE_OPENAI_API_KEY` | API key for the Azure OpenAI resource |

You can set them in your IDE launch profile, in your shell, or in the project's debug properties. The app reads the variables at startup and surfaces a "Configuration Required" alert if any value is missing or the endpoint is malformed.

### Syncfusion license key

Set the `SYNCFUSION_LICENSE_KEY` environment variable to your license key before launching the app. Without a valid license, Syncfusion controls render a watermark or trial dialog.

## Rating scale

The model returns a single decimal between `0.0` and `5.0`:

- `0.0` — not enough information, gibberish, or unrelated text
- `0.1–1.0` — extremely dissatisfied
- `1.1–2.0` — dissatisfied
- `2.1–3.0` — mixed / neutral
- `3.1–4.0` — generally satisfied
- `4.1–5.0` — highly satisfied

The rating is clamped into that range before being applied to the UI.

## Data flow

`MainPage` listens for `Editor.TextChanged`, delegates to `MainViewModel.OnFeedbackChanged`, which:

1. Cancels the previous in-flight request.
2. Increments a request version stamp.
3. Waits 500 ms (debounce) and re-checks the version.
4. Calls `OpenAIService.GetSuggestedRatingAsync` with the cancellation token.
5. After the response, re-checks the version before publishing the rating, so a stale response cannot overwrite a newer one.

If the call fails, `ErrorMessage` is set and rendered as a red label below the editor.

## Troubleshooting

### Path too long exception

If you encounter a "Path Too Long" exception while building the project, close Visual Studio, rename the repository folder to a shorter name, and rebuild the project.

## License

Syncfusion has no liability for any damage or consequence that may arise from using or viewing the samples. The samples are for demonstrative purposes. If you choose to use or access the samples, you agree to not hold Syncfusion liable, in any form, for any damage related to use, for accessing, or viewing the samples. By accessing, viewing, or seeing the samples, you acknowledge and agree Syncfusion's samples will not allow you seek injunctive relief in any form for any claim related to the sample. If you do not agree to this, do not view, access, utilize, or otherwise do anything with Syncfusion's samples.