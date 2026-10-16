namespace NutriLens.Services;

/// <summary>
/// Captures or picks an image of a food label and copies it into the app cache
/// so the file outlives MAUI's transient picker temp directory.
/// </summary>
public interface IImagePickerService
{
    Task<FileResult?> CapturePhotoAsync(CancellationToken cancellationToken = default);
    Task<FileResult?> PickPhotoAsync(CancellationToken cancellationToken = default);
}

public sealed class ImagePickerService : IImagePickerService
{
    public async Task<FileResult?> CapturePhotoAsync(
        CancellationToken cancellationToken = default)
    {
        if (IsDesktop)
            return await PickImageFileAsync();

        if (!MediaPicker.Default.IsCaptureSupported)
            throw new InvalidOperationException("Camera capture is not available on this device.");

        var photo = await MediaPicker.Default.CapturePhotoAsync(
            new MediaPickerOptions { Title = "Capture food label" });

        return await CopyToAppCacheAsync(photo);
    }

    public async Task<FileResult?> PickPhotoAsync(
        CancellationToken cancellationToken = default)
    {
        if (IsDesktop)
            return await PickImageFileAsync();

        var photos = await MediaPicker.Default.PickPhotosAsync(
            new MediaPickerOptions { Title = "Select food label" });

        var photo = photos.FirstOrDefault();
        return await CopyToAppCacheAsync(photo);
    }

    private static bool IsDesktop =>
        DeviceInfo.Platform == DevicePlatform.WinUI ||
        DeviceInfo.Platform == DevicePlatform.MacCatalyst;

    private static async Task<FileResult?> PickImageFileAsync()
    {
        var file = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Attach food label image",
            FileTypes = FilePickerFileType.Images
        });

        return await CopyToAppCacheAsync(file);
    }

    // MAUI deletes the temp capture files once the picker closes. Copy to a
    // path we own so downstream pages can still open the FileResult.
    private static async Task<FileResult?> CopyToAppCacheAsync(FileResult? file)
    {
        if (file is null)
            return null;

        var targetDir = Path.Combine(
            FileSystem.CacheDirectory, "captures");
        Directory.CreateDirectory(targetDir);

        var target = Path.Combine(
            targetDir,
            $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}");

        await using var source = await file.OpenReadAsync();
        await using var destination = File.Create(target);
        await source.CopyToAsync(destination);

        return new FileResult(target);
    }
}