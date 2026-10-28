# Smart Cropping

A .NET MAUI sample that uses **Azure OpenAI Vision** to analyze an image and
suggest one rectangular crop region around its most important visual content.
The suggestion is previewed on top of a **Syncfusion SfImageEditor** before the
crop is applied.

## What it does

- Loads a sample image (or one you pick with **Open Image**).
- Sends the image to **Azure OpenAI** and asks the model for a single
  rectangular crop region.
- Shows the suggestion as a blue-bordered overlay on top of the editor. The
  area outside the crop is dimmed.
- Lets you **Apply Crop**, **Adjust Manually** (launches the editor's built-in
  crop UI), **Reset**, or **Save** the result.

## Solution layout

```
SmartCropping/
├── SmartCropping.slnx
└── SmartCropping/
    ├── AIService/
    │   ├── AzureBaseService.cs
    │   ├── ISmartCropService.cs
    │   └── SmartCropService.cs
    ├── Converters/
    │   ├── BoolToObjectConverter.cs
    │   └── InverseBoolConverter.cs
    ├── Helpers/
    │   └── CropOverlayView.cs          # GraphicsView-based overlay renderer
    ├── Models/
    │   ├── CropRegion.cs
    │   └── SmartCropResponse.cs
    ├── Resources/
    │   ├── Images/sample_image.png     # Bundled sample
    │   └── Styles/                     # Color + style resources
    ├── ViewModel/
    │   ├── IImageEditorController.cs   # View-model ↔ editor bridge
    │   └── SmartCroppingViewModel.cs   # CommunityToolkit.Mvvm
    ├── Views/
    │   ├── SmartCropping.xaml
    │   └── SmartCropping.xaml.cs       # Implements IImageEditorController
    ├── App.xaml(.cs)
    ├── AppShell.xaml(.cs)
    ├── MauiProgram.cs
    └── SmartCropping.csproj
```

## How to run

1. Open the solution in Visual Studio 2022 / VS Code with the .NET MAUI
   workload installed.
2. Set your Azure OpenAI endpoint, deployment name, and API key in
   `AIService/AzureBaseService.cs` (or replace the constants with your own
   configuration mechanism).
3. Optionally replace the Syncfusion license key in `App.xaml.cs`.
4. Select the **Windows Machine**, **Android**, **iOS**, or **Mac Catalyst**
   target and run.

## MVVM and AI service pattern

- **ViewModel** (`SmartCroppingViewModel`) owns all state and commands using
  `CommunityToolkit.Mvvm` (`[ObservableProperty]`, `[RelayCommand]`).
- **AIService** (`SmartCropService`) wraps the Azure OpenAI Vision call and
  returns a strongly typed `SmartCropResponse` (`CropRegion`).
- **View** (`SmartCropping.xaml.cs`) implements `IImageEditorController`, which
  lets the view model call `Crop`, `SaveEdits`, `Reset`, and `Save` on
  `SfImageEditor` without taking a direct dependency on Syncfusion types.

## Workflow

1. **Open Image** loads a `FileResult` and waits for `ImageLoaded`.
2. **Smart Crop** sends the original image bytes to Azure OpenAI, validates
   the returned `cropRegion` against `OriginalImageSize`, and shows the
   preview overlay.
3. **Apply Crop** calls `imageEditor.Crop(rect, false)` followed by
   `SaveEdits()`.
4. **Adjust Manually** calls `imageEditor.Crop(ImageCropType.Free)` to open
   the built-in crop UI.
5. **Reset** cancels any pending edit, reloads the original image source, and
   re-enables Smart Crop.
6. **Save** calls `imageEditor.Save(ImageFileType.Png)` to invoke the OS
   save dialog.
