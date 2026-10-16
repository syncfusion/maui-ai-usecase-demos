using System.Globalization;

namespace AIChatSample.Converters;

/// <summary>
/// Converts a file path or URL string into an <see cref="ImageSource"/> so an
/// <c>Image.Source</c> binding can render an image selected at runtime (e.g.
/// via <c>FilePicker</c>). Returns <c>null</c> for null/whitespace input.
/// Handles http/https URLs, <c>file://</c> / <c>content://</c> URIs (e.g.
/// Android FilePicker results) and raw local paths returned by
/// <see cref="Microsoft.Maui.Storage.FilePicker"/> on Windows. On Windows,
/// <c>ImageSource.FromFile</c> only resolves paths that live inside the
/// app package, so we wrap arbitrary local paths in a <c>file:///</c> URI
/// which <c>ImageSource.FromUri</c> loads reliably cross-platform.
/// </summary>
public class StringToImageSourceConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string s || string.IsNullOrWhiteSpace(s))
        {
            return null;
        }

        if (Uri.TryCreate(s, UriKind.Absolute, out var uri))
        {
            // http(s) / file / content — all work via FromUri on every
            // target platform (Windows, Android, iOS, MacCatalyst).
            return ImageSource.FromUri(uri);
        }

        // Bare local path (e.g. "C:\Users\...\foo.png" from the Windows
        // FilePicker, or "/var/.../foo.png" on Mac Catalyst). Build a
        // file:// URI so ImageSource.FromUri is used; passing a raw path
        // to FromFile fails for files outside the app bundle on Windows.
        try
        {
            var fileUri = new Uri(s);
            return ImageSource.FromUri(fileUri);
        }
        catch
        {
            return ImageSource.FromFile(s);
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }
}
