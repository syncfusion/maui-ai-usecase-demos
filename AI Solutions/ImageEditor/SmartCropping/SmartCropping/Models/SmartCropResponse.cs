using System.Text.Json.Serialization;

namespace SmartCropping;

/// <summary>
/// Represents the AI-generated crop suggestion returned by the Azure AI service.
/// </summary>
/// <remarks>
/// The response contains a short human-readable description of the proposed
/// framing together with the rectangular region to crop. The ViewModel is
/// responsible for validating the region against the loaded image before it is
/// applied to the editor.
/// </remarks>
public sealed class SmartCropResponse
{
    /// <summary>
    /// Gets or sets a short description of how the AI chose to frame the crop.
    /// </summary>
    [JsonPropertyName("framing")]
    public string Framing { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the proposed crop region in original image pixels.
    /// </summary>
    [JsonPropertyName("cropRegion")]
    public CropRegion CropRegion { get; set; } = new();
}
