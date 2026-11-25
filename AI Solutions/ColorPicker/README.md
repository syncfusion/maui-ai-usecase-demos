# AI-Assisted-Color-Palette-Generator-using-MAUI-ColorPicker
Discover how to build a AI-Assisted color Palette generator using Syncfusion .NET MAUI components.

## Features
* AI-generated color palette from a style prompt
* Live preview of primary, secondary, accent, background, and text colors
* Syncfusion color picker for palette selection
* Floating text input for style description
* Responsive MAUI UI for Android, iOS, Mac Catalyst, and Windows

## Technologies Used
* .NET MAUI
* Syncfusion MAUI Inputs
* Syncfusion MAUI Cards
* Azure OpenAI
* MVVM pattern

## Project Structure
* MainPage.xaml
* MainPage.xaml.cs
* MainViewModel.cs
* SmartColorPaletteService.cs
* AzureBaseService.cs
* ColorPaletteResponse.cs
* MauiProgram.cs

## Key Highlights
* AI-powered color palette generation from a style prompt
* Syncfusion SfColorPicker with palette support
* Live preview of generated theme colors
* MVVM-based architecture
* Cross-platform MAUI support
* Automatic palette refresh after generation

## How It Works
1. Enter a style description such as modern, minimal, pastel, or vibrant.
2. Tap Generate Palette.
3. The app sends the prompt to the AI service.
4. The AI returns a palette response in JSON format.
5. The app updates the color picker and live preview with the generated colors.

## Prerequisites
* .NET 10 SDK
* MAUI workload
* Syncfusion MAUI package access
* Azure OpenAI credentials or configured AI backend

## Troubleshooting
* If the app does not start, verify that MainViewModel and MainPage are registered in DI.
* If the color picker does not update, ensure PaletteColors matches the control’s expected type.
* If AI results do not appear, confirm the service credentials and API configuration.
* If bindings fail, check BindingContext and property change notifications.
* If the color picker does not open, verify IsOpen is set and the control load event is wired correctly.

## Setup
* Restore NuGet packages.
* Configure Azure OpenAI settings in the app’s service layer.
* Build and run on your target platform.

## Notes
* The color picker opens automatically when the page loads.
* The palette preview updates after generation.
* PaletteColors should be kept in the same type expected by SfColorPicker.

## License
Syncfusion controls are subject to the [Syncfusion License](https://www.syncfusion.com/sales/licensing). A free community license is available for qualifying individuals and small businesses.