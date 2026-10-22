namespace SmartCropping;

/// <summary>
/// Defines the contract for AI services that suggest a single crop region
/// around the most important content of an image.
/// </summary>
public interface ISmartCropService
{
    /// <summary>
    /// Sends the supplied image to Azure OpenAI and returns a suggested crop
    /// region. Coordinates are returned in the original image pixel space.
    /// </summary>
    /// <param name="imageBytes">The encoded image (PNG/JPEG) to analyze.</param>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    /// <returns>
    /// A <see cref="SmartCropResponse"/> containing the proposed framing and
    /// crop region, or <see langword="null"/> when the service could not
    /// produce a valid suggestion.
    /// </returns>
    Task<SmartCropResponse?> SuggestCropAsync(byte[] imageBytes, CancellationToken cancellationToken = default);
}
