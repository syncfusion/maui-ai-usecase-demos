# LiteLLM_Routing

A .NET MAUI chat sample that routes customer queries to one of three Azure OpenAI models — `gpt-4.1-nano` (low), `gpt-4.1-mini` (standard), `gpt-5.6-luna` (high) — and renders the selected model's response in a Syncfusion `SfAIAssistView` bubble.

## Routing flow

1. The user query is sent to the low-end **nano** model, which classifies the request as `LOW`, `STANDARD`, or `HIGH`.
2. The query is forwarded to the matching model.
3. The response is rendered in the AIAssistView response bubble, with the chosen model name displayed in the bubble header.

## Configuration

### Shared fallback credentials

If a per-model credential is not set, the service falls back to:

| Variable | Purpose |
| --- | --- |
| `AZURE_API_BASE` | Azure OpenAI resource URI, e.g. `https://YOUR-RESOURCE.openai.azure.com/` |
| `AZURE_API_KEY` | Azure OpenAI API key |

### Per-model credentials

You can give each GPT model its own Azure OpenAI resource. Set env vars whose suffix
matches the model name (uppercased, with `.`, `-`, ` ` replaced by `_`):

| Variable pattern | Example |
| --- | --- |
| `AZURE_API_BASE_<MODEL>` | `AZURE_API_BASE_GPT_4_1_NANO` |
| `AZURE_API_KEY_<MODEL>`  | `AZURE_API_KEY_GPT_4_1_MINI`  |

The lookup order for each model is:

1. `AZURE_API_BASE_<MODEL>` / `AZURE_API_KEY_<MODEL>` (per-model)
2. `AZURE_API_BASE` / `AZURE_API_KEY` (shared)
3. Dummy placeholder values (so the app still compiles and runs)

### Per-model pricing (optional, accurate cost)

The cost shown in the response footer is computed from per-1K-token USD prices. To make
the rate accurate to your billing plan, set:

| Variable pattern | Example |
| --- | --- |
| `AZURE_PRICE_INPUT_<MODEL>`  | `AZURE_PRICE_INPUT_GPT_4_1_NANO` |
| `AZURE_PRICE_OUTPUT_<MODEL>` | `AZURE_PRICE_OUTPUT_GPT_4_1_LUNA` |

If a per-model price is not set, built-in defaults are used. Set the values as decimal
numbers (e.g. `0.0004`).

## Project layout

```
LLM_Routing/
├─ Models/
│  └─ ChatItems.cs           # AIAssistResponseItem (UI item with ModelHeader)
├─ Services/
│  ├─ AzureOpenAIService.cs  # Routing + response generation via Azure.AI.OpenAI
│  ├─ AIAssistTemplateSelector.cs  # UI template selector
│  └─ CostCalculator.cs      # Per-model pricing (env-driven) and cost formatting
├─ ViewModels/
│  └─ MainPageViewModel.cs   # MVVM: commands, chat flow, no business logic
├─ MainPage.xaml(.cs)        # Chat UI wired to the view-model
└─ MauiProgram.cs            # DI registrations
```
