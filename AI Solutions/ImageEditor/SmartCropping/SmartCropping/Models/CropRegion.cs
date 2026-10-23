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

    /// <summary>
    /// Returns a copy of this region expanded outward by the given fraction
    /// of the region's own width and height, then clamped to the supplied
    /// image bounds.
    /// </summary>
    /// <remarks>
    /// This is used as a safety net on top of the AI suggestion so that a
    /// slightly tight AI bounding box — one that clips an animal's tail or
    /// a person's hand — gets a few percent of breathing room on every side
    /// before the preview is shown to the user. The expanded region is
    /// clamped so it never escapes the image.
    /// </remarks>
    /// <param name="fraction">
    /// Fraction of the current width and height to add on every side.
    /// Use a small positive value like 0.05–0.10 for a margin of a few
    /// percent of the subject's size. Must be non-negative.
    /// </param>
    /// <param name="imageWidth">Width of the original image in pixels.</param>
    /// <param name="imageHeight">Height of the original image in pixels.</param>
    /// <returns>A new <see cref="CropRegion"/> with the expanded bounds.</returns>
    public CropRegion ExpandedBy(double fraction, double imageWidth, double imageHeight)
    {
        if (double.IsNaN(fraction) || fraction < 0)
        {
            fraction = 0;
        }

        double padX = Width * fraction;
        double padY = Height * fraction;

        double newX = X - padX;
        double newY = Y - padY;
        double newWidth = Width + (padX * 2);
        double newHeight = Height + (padY * 2);

        // Clamp to image bounds.
        if (newX < 0)
        {
            newWidth += newX;        // shrink width by the overdraw on the left
            newX = 0;
        }

        if (newY < 0)
        {
            newHeight += newY;
            newY = 0;
        }

        if (newX + newWidth > imageWidth)
        {
            newWidth = Math.Max(0, imageWidth - newX);
        }

        if (newY + newHeight > imageHeight)
        {
            newHeight = Math.Max(0, imageHeight - newY);
        }

        return new CropRegion
        {
            X = newX,
            Y = newY,
            Width = newWidth,
            Height = newHeight
        };
    }
}
