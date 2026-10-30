using System.Collections.Concurrent;
using System.Globalization;
using System.Security.Cryptography;

namespace WardrobeAI.Converters;

/// <summary>
/// Converts a base64 string into a MAUI <see cref="ImageSource"/> for display in
/// <c>SfAvatarView</c> / <c>Image</c> / <c>SfRotator</c> item templates.
/// Returns null on null/empty/invalid input so the consumer can fall back to
/// its default content.
///
/// <para><b>Why we materialise to a temp file instead of using
/// <c>ImageSource.FromStream</c>:</b> <c>ImageSource.FromStream</c> invokes
/// the supplied factory lazily and disposes the returned stream after the
/// native image loader reads it. Inside an <c>SfRotator</c> item template the
/// factory can be called multiple times across recycling, and on Windows the
/// stream is consumed before the image finishes decoding — the result is a
/// blank white tile even though the converter was called. Writing the
/// decoded bytes to a deterministic temp PNG and returning
/// <c>ImageSource.FromFile</c> gives the native loader a real file path that
/// it can decode cleanly on every platform, including inside <c>SfRotator</c>.</para>
/// </summary>
public sealed class Base64ToImageSourceConverter : IValueConverter
{
    private const int MaxCachedFiles = 500;
    private static readonly ConcurrentDictionary<string, Task> CachedFiles = new(StringComparer.Ordinal);

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string base64 || string.IsNullOrWhiteSpace(base64))
        {
            return null;
        }

        byte[] bytes;
        try
        {
            bytes = System.Convert.FromBase64String(base64);
        }
        catch (FormatException)
        {
            return null;
        }

        if (bytes.Length == 0)
        {
            return null;
        }

        try
        {
            var hashHex = System.Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
            var path = Path.Combine(FileSystem.Current.CacheDirectory, $"wa_base64_{hashHex}.bin");
            _ = EnsureCachedFileAsync(path, bytes);
            return Microsoft.Maui.Controls.ImageSource.FromFile(path);
        }
        catch
        {
            return null;
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();

    /// <summary>
    /// Writes image bytes to a deterministic cache file and evicts older
    /// converter files when the cache exceeds its cap.
    /// </summary>
    private static Task EnsureCachedFileAsync(string path, byte[] bytes)
    {
        var cacheDir = FileSystem.Current.CacheDirectory;
        return CachedFiles.GetOrAdd(path, _ => Task.Run(() =>
        {
            Directory.CreateDirectory(cacheDir);
            if (!File.Exists(path))
            {
                File.WriteAllBytes(path, bytes);
            }
            EvictOldFiles(cacheDir, path);
        }));
    }

    private static void EvictOldFiles(string cacheDir, string keepPath)
    {
        var files = new DirectoryInfo(cacheDir).GetFiles("wa_base64_*.bin")
            .Where(file => !string.Equals(file.FullName, keepPath, StringComparison.Ordinal))
            .OrderByDescending(file => file.LastWriteTimeUtc)
            .Skip(MaxCachedFiles - 1)
            .ToArray();
        foreach (var file in files)
        {
            file.Delete();
            CachedFiles.TryRemove(file.FullName, out _);
        }
    }
}
