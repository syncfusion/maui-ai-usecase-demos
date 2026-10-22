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
            var path = GetCachedFilePath(bytes);
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
    /// Writes <paramref name="bytes"/> to a deterministic PNG file under the
    /// app cache directory and returns its absolute path. The file name is a
    /// SHA-256 hash of the bytes so the same image content maps to the same
    /// file across converter invocations (and across <c>SfRotator</c> item
    /// recycling), and we avoid re-decoding the base64 string on every
    /// binding pass.
    /// </summary>
    private static string GetCachedFilePath(byte[] bytes)
    {
        var cacheDir = FileSystem.Current.CacheDirectory;
        Directory.CreateDirectory(cacheDir);

        // SHA-256 over the raw bytes is cheap and produces a stable filename
        // that won't collide across distinct images but will reuse the same
        // file when the same image is re-bound (e.g. swiping back and forth
        // in SfRotator, or re-navigating to the page).
        var hashHex = System.Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        var path = Path.Combine(cacheDir, $"wa_base64_{hashHex}.bin");

        if (!File.Exists(path))
        {
            File.WriteAllBytes(path, bytes);
        }

        return path;
    }
}
