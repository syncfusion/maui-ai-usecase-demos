using System.Globalization;

namespace AIChatSample.Converters;

/// <summary>
/// Converts a file path or URL string into an <see cref="ImageSource"/> so an
/// <c>Image.Source</c> binding can render an image selected at runtime (e.g.
/// via <c>FilePicker</c>). Returns <c>null</c> for null/whitespace input.
/// </summary>
/// <remarks>
/// Handles three categories of input:
/// <list type="bullet">
///   <item><description>Absolute http/https/content URIs — the platform handler
///   loads them via <see cref="ImageSource.FromUri"/>.</description></item>
///   <item><description>Bare local paths ("C:\Users\foo\bar.png", "/var/.../foo.png")
///   and <c>file://</c> URIs returned by <c>FilePicker.PickAsync</c> — read the
///   bytes through a <see cref="StreamImageSource"/>. This is the only
///   cross-platform way to render files outside the MAUI app bundle on
///   Windows: <see cref="ImageSource.FromFile(string)"/> only resolves
///   in-package assets there, and <see cref="ImageSource.FromUri(Uri)"/> is
///   unreliable for arbitrary <c>file://</c> URIs on the WinUI
///   <c>BitmapImage</c> handler.</description></item>
/// </list>
/// </remarks>
public class StringToImageSourceConverter : IValueConverter
{
    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string s || string.IsNullOrWhiteSpace(s))
        {
            return null;
        }

        // Absolute http(s) / content URIs — the platform handler loads them
        // directly. Android's FilePicker returns a content:// URI here,
        // which BitmapImage can stream on Android.
        if (Uri.TryCreate(s, UriKind.Absolute, out var uri) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps ||
             uri.Scheme == "content"))
        {
            return ImageSource.FromUri(uri);
        }

        // Resolve to a local file path. Handles bare paths ("C:\...",
        // "/var/...") and file:// URIs alike.
        string? localPath = null;
        if (Uri.TryCreate(s, UriKind.Absolute, out var fileUri) &&
            fileUri.Scheme == Uri.UriSchemeFile)
        {
            localPath = fileUri.LocalPath;
        }
        else
        {
            localPath = s;
        }

        // Read the bytes ourselves. FromFile only resolves in-package
        // assets on Windows, and FromUri's file:// handling is unreliable
        // for arbitrary local paths on the WinUI handler. StreamImageSource
        // works everywhere the file is readable from disk.
        if (!string.IsNullOrEmpty(localPath) && File.Exists(localPath))
        {
            var path = localPath;
            return new StreamImageSource
            {
                Stream = _ => Task.FromResult<Stream>(File.OpenRead(path))
            };
        }

        // Last-resort fallback for relative paths or symbolic URIs.
        return ImageSource.FromFile(s);
    }

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }
}
