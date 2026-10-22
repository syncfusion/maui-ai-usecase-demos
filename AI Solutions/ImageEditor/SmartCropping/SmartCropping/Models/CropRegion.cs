using System.Text.Json.Serialization;

namespace SmartCropping;

/// <summary>
/// Represents the rectangular crop region returned by the Azure AI service.
/// </summary>
/// <remarks>
/// Coordinates are expressed in the original image pixel space. Use
/// <see cref="IsValid"/> to confirm a returned region is non-empty, fully
/// contained inside its parent image, and safe to apply.
/// </remarks>
public sealed class CropRegion
{
    /// <summary>
    /// Gets or sets the horizontal position of the region's left edge in
    /// original image pixels.
    /// </summary>
    [JsonPropertyName("x")]
    public double X { get; set; }

    /// <summary>
    /// Gets or sets the vertical position of the region's top edge in
    /// original image pixels.
    /// </summary>
    [JsonPropertyName("y")]
    public double Y { get; set; }

    /// <summary>
    /// Gets or sets the width of the region in original image pixels.
    /// </summary>
    [JsonPropertyName("width")]
    public double Width { get; set; }

    /// <summary>
    /// Gets or sets the height of the region in original image pixels.
    /// </summary>
    [JsonPropertyName("height")]
    public double Height { get; set; }

    /// <summary>
    /// Returns <see langword="true"/> when the crop region is fully
    /// contained inside an image of the supplied dimensions and has a
    /// positive size on both axes.
    /// </summary>
    /// <param name="imageWidth">The width of the original image in pixels.</param>
    /// <param name="imageHeight">The height of the original image in pixels.</param>
    /// <returns><see langword="true"/> when the region is valid for the image; otherwise <see langword="false"/>.</returns>
    public bool IsValid(double imageWidth, double imageHeight)
    {
        if (double.IsNaN(X) || double.IsNaN(Y) ||
            double.IsNaN(Width) || double.IsNaN(Height))
        {
            return false;
        }

        if (Width <= 0 || Height <= 0)
        {
            return false;
        }

        if (X < 0 || Y < 0)
        {
            return false;
        }

        if (X + Width > imageWidth + 0.5)
        {
            return false;
        }

        if (Y + Height > imageHeight + 0.5)
        {
            return false;
        }

        return true;
    }
}
