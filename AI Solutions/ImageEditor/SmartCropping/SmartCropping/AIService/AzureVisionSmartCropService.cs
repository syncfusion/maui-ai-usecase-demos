using Azure;
using Azure.AI.Vision.ImageAnalysis;
using System.Diagnostics;

namespace SmartCropping;

/// <summary>
/// Deterministic Smart Cropping implementation backed by Azure AI Vision
/// (Image Analysis). Unlike <see cref="SmartCropService"/>, which asks a
/// vision model to interpret a free-form prompt, this service asks the
/// service for the actual bounding boxes it found and then applies
/// intent-specific framing rules locally.
/// </summary>
/// <remarks>
/// This is a parallel implementation; the existing Azure OpenAI path
/// (<see cref="SmartCropService"/>) is unchanged. Switch implementations
/// via DI (see <c>MauiProgram.cs</c>).
/// </remarks>
public sealed class AzureVisionSmartCropService : ISmartCropService
{
    /// <summary>Converts an SDK bounding box into our own <see cref="CropRegion"/>.</summary>
    private static CropRegion ToCropRegion(ImageBoundingBox box) =>
        new() { X = box.X, Y = box.Y, Width = box.Width, Height = box.Height };

    /// <summary>Tags from the Azure AI Vision Objects taxonomy that identify pets.</summary>
    private static readonly HashSet<string> PetTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "dog", "cat", "bird", "horse"
    };

    /// <summary>Tags from the Objects taxonomy that identify food items (best-effort).</summary>
    private static readonly HashSet<string> FoodTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "food", "banana", "apple", "orange", "broccoli", "carrot", "hot dog",
        "sandwich", "pizza", "cake", "donut", "bottle", "cup", "bowl"
    };

    private readonly ImageAnalysisClient client;

    public AzureVisionSmartCropService(string endpoint, string apiKey)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            throw new ArgumentException(
                "Azure AI Vision endpoint is required (set AZURE_VISION_ENDPOINT).",
                nameof(endpoint));
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new ArgumentException(
                "Azure AI Vision API key is required (set AZURE_VISION_KEY).",
                nameof(apiKey));
        }

        client = new ImageAnalysisClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
    }

    /// <inheritdoc />
    public async Task<SmartCropResponse?> SuggestCropAsync(
        byte[] imageBytes,
        string intent,
        int imageWidth,
        int imageHeight,
        CancellationToken cancellationToken = default)
    {
        if (imageBytes is null || imageBytes.Length == 0)
        {
            return null;
        }

        if (imageWidth <= 0 || imageHeight <= 0)
        {
            return null;
        }

        try
        {
            // Request people, object, and smart-crop suggestions in a single
            // call so we have the full detection picture in one round trip.
            ImageAnalysisResult analysis = await client.AnalyzeAsync(
                BinaryData.FromBytes(imageBytes),
                VisualFeatures.People | VisualFeatures.Objects | VisualFeatures.SmartCrops,
                cancellationToken: cancellationToken);

            DetectedSubject subject = SelectSubject(analysis, intent);

            CropRegion crop = intent switch
            {
                "Profile Photo"      => FrameProfilePhoto(subject, imageWidth, imageHeight),
                "Pet Portrait"       => FramePetPortrait(subject, imageWidth, imageHeight),
                "Product Image"      => FrameProduct(subject, imageWidth, imageHeight),
                "Food Photography"   => FrameFood(subject, imageWidth, imageHeight),
                "Hero Banner"        => FrameHeroBanner(analysis, subject, imageWidth, imageHeight),
                "Object Focus"       => FrameObjectFocus(subject, imageWidth, imageHeight),
                _                    => FrameDefault(subject, imageWidth, imageHeight),
            };

            crop = crop.ClampedTo(imageWidth, imageHeight);
            if (!crop.IsValid(imageWidth, imageHeight))
            {
                Debug.WriteLine("AzureVisionSmartCropService: produced an invalid crop after framing.");
                return null;
            }

            return new SmartCropResponse
            {
                Framing = DescribeFraming(intent, subject, crop, imageWidth, imageHeight),
                CropRegion = crop
            };
        }
        catch (RequestFailedException ex)
        {
            Debug.WriteLine($"Azure Vision request failed: {ex.Status} | {ex.ErrorCode} | {ex.Message}");
            return null;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"AzureVisionSmartCropService: {ex.Message}");
            return null;
        }
    }

    #region Subject selection

    /// <summary>
    /// Picks the bounding box to frame, in this priority order: a detected
    /// person, a detected pet, the largest detected object, or the first
    /// smart-crop suggestion if nothing else was detected.
    /// </summary>
    private static DetectedSubject SelectSubject(ImageAnalysisResult analysis, string intent)
    {
        var people = analysis.People?.Values;
        if (people is { Count: > 0 })
        {
            DetectedPerson largest = people
                .OrderByDescending(p => p.BoundingBox.Width * p.BoundingBox.Height)
                .First();
            return new DetectedSubject(ToCropRegion(largest.BoundingBox), SubjectKind.Person, "person", largest.Confidence);
        }

        var objects = analysis.Objects?.Values;
        if (objects is { Count: > 0 })
        {
            DetectedObject? pet = objects
                .SelectMany(o => o.Tags.Select(t => (Object: o, Tag: t)))
                .Where(x => PetTags.Contains(x.Tag.Name))
                .OrderByDescending(x => x.Object.BoundingBox.Width * x.Object.BoundingBox.Height)
                .Select(x => x.Object)
                .FirstOrDefault();

            if (pet is not null)
            {
                return new DetectedSubject(ToCropRegion(pet.BoundingBox), SubjectKind.Pet, "pet", 1.0);
            }

            // For food, prefer a food-tagged object if any.
            if (string.Equals(intent, "Food Photography", StringComparison.OrdinalIgnoreCase))
            {
                DetectedObject? food = objects
                    .SelectMany(o => o.Tags.Select(t => (Object: o, Tag: t)))
                    .Where(x => FoodTags.Contains(x.Tag.Name))
                    .OrderByDescending(x => x.Object.BoundingBox.Width * x.Object.BoundingBox.Height)
                    .Select(x => x.Object)
                    .FirstOrDefault();

                if (food is not null)
                {
                    return new DetectedSubject(ToCropRegion(food.BoundingBox), SubjectKind.Food, "food", 1.0);
                }
            }

            DetectedObject largestObj = objects
                .OrderByDescending(o => o.BoundingBox.Width * o.BoundingBox.Height)
                .First();

            string primaryTag = largestObj.Tags.Count > 0 ? largestObj.Tags[0].Name : "object";
            return new DetectedSubject(ToCropRegion(largestObj.BoundingBox), SubjectKind.Object, primaryTag, 1.0);
        }

        var smart = analysis.SmartCrops?.Values;
        if (smart is { Count: > 0 })
        {
            CropRegion first = ToCropRegion(smart[0].BoundingBox);
            return new DetectedSubject(first, SubjectKind.SmartCrop, "smart-crop", 1.0);
        }

        // No detection returned at all: leave the subject empty so the
        // framing rules fall back to a centred full-image crop.
        return new DetectedSubject(new CropRegion
        {
            X = 0, Y = 0, Width = 1, Height = 1
        }, SubjectKind.None, "none", 0.0);
    }

    #endregion

    #region Intent-specific framing

    /// <summary>
    /// Professional head-and-shoulders profile photo. The detected person
    /// box (or, if People wasn't found, the largest object box) is treated
    /// as the head/torso region. The crop is recentred so the head sits at
    /// the horizontal middle, and the vertical extent is sized to head plus
    /// shoulders only.
    /// </summary>
    private static CropRegion FrameProfilePhoto(DetectedSubject subject, int imageWidth, int imageHeight)
    {
        if (subject.Kind == SubjectKind.None)
        {
            return CentredSquare(imageWidth, imageHeight);
        }

        var box = subject.Box;
        double headHeight = Math.Max(box.Height, box.Width * 1.2);
        double headCenterX = box.X + box.Width / 2.0;

        // Target crop: tall enough to hold the head plus shoulders, square
        // or slightly portrait so it works as an avatar. The crop is always
        // centred horizontally on the head, which fixes the asymmetric
        // shoulder framing the OpenAI implementation produced.
        double desiredHeight = headHeight * 2.4;
        double desiredWidth = desiredHeight * 0.95; // slight portrait

        if (desiredWidth > imageWidth)
        {
            desiredWidth = imageWidth;
            desiredHeight = desiredWidth / 0.95;
        }

        double x = headCenterX - desiredWidth / 2.0;
        double y = box.Y - headHeight * 0.15; // start a touch above the head

        return Clamp(imageWidth, imageHeight, x, y, desiredWidth, desiredHeight);
    }

    /// <summary>
    /// Pet Portrait: full pet visible with natural margins. Uses the
    /// detected pet box and adds ~15% padding on every side so ears, tail
    /// and paws aren't clipped, while keeping the pet prominent.
    /// </summary>
    private static CropRegion FramePetPortrait(DetectedSubject subject, int imageWidth, int imageHeight)
    {
        return PadAround(subject, imageWidth, imageHeight, fraction: 0.18, targetProminence: 0.60);
    }

    /// <summary>Product Image: full product with professional margins.</summary>
    private static CropRegion FrameProduct(DetectedSubject subject, int imageWidth, int imageHeight)
    {
        return PadAround(subject, imageWidth, imageHeight, fraction: 0.12, targetProminence: 0.70);
    }

    /// <summary>Food Photography: food fills most of the crop with comfortable margins.</summary>
    private static CropRegion FrameFood(DetectedSubject subject, int imageWidth, int imageHeight)
    {
        return PadAround(subject, imageWidth, imageHeight, fraction: 0.10, targetProminence: 0.70);
    }

    /// <summary>
    /// Hero Banner: the widest crop that still keeps the subject visible.
    /// Prefers a smart-crop suggestion that already represents a landscape
    /// framing; falls back to a generous landscape rectangle around the
    /// subject.
    /// </summary>
    private static CropRegion FrameHeroBanner(ImageAnalysisResult analysis, DetectedSubject subject, int imageWidth, int imageHeight)
    {
        var smart = analysis.SmartCrops?.Values;
        if (smart is { Count: > 0 })
        {
            // Pick the smart crop with the widest aspect ratio (W >= H).
            CropRegion best = smart
                .Select(c => ToCropRegion(c.BoundingBox))
                .Where(c => c.Width >= c.Height)
                .OrderByDescending(c => c.Width / (double)Math.Max(1, c.Height))
                .FirstOrDefault();

            if (best.Width > 0)
            {
                return Clamp(imageWidth, imageHeight, best.X, best.Y, best.Width, best.Height);
            }
        }

        return PadAround(subject, imageWidth, imageHeight, fraction: 0.30, targetProminence: 0.35);
    }

    /// <summary>
    /// Object Focus: tightest framing. Keeps the subject fully visible but
    /// adds only a small safety margin so the subject visually dominates.
    /// </summary>
    private static CropRegion FrameObjectFocus(DetectedSubject subject, int imageWidth, int imageHeight)
    {
        return PadAround(subject, imageWidth, imageHeight, fraction: 0.04, targetProminence: 0.82);
    }

    /// <summary>Default fallback: balanced crop around the detected subject.</summary>
    private static CropRegion FrameDefault(DetectedSubject subject, int imageWidth, int imageHeight)
    {
        return PadAround(subject, imageWidth, imageHeight, fraction: 0.10, targetProminence: 0.65);
    }

    #endregion

    #region Framing helpers

    /// <summary>
    /// Expands the subject box by a fraction of the image dimensions, then
    /// rescales the crop so the subject occupies roughly <paramref name="targetProminence"/>
    /// of the crop area. The result is recentred and clamped to the image.
    /// </summary>
    private static CropRegion PadAround(DetectedSubject subject, int imageWidth, int imageHeight, double fraction, double targetProminence)
    {
        if (subject.Kind == SubjectKind.None)
        {
            return CentredSquare(imageWidth, imageHeight);
        }

        var box = subject.Box;

        // Step 1: add a comfortable margin so we never clip the subject.
        double padX = imageWidth * fraction;
        double padY = imageHeight * fraction;
        double paddedX = box.X - padX;
        double paddedY = box.Y - padY;
        double paddedW = box.Width + 2 * padX;
        double paddedH = box.Height + 2 * padY;

        // Step 2: enlarge so the subject occupies targetProminence of the
        // crop. Square the prominence so area-based, not edge-based.
        double desiredArea = (paddedW * paddedH) / Math.Max(0.05, targetProminence);
        double currentArea = paddedW * paddedH;
        if (desiredArea > currentArea)
        {
            double scale = Math.Sqrt(desiredArea / currentArea);
            paddedW *= scale;
            paddedH *= scale;
        }

        // Step 3: keep the subject centred inside the crop after expansion.
        double subjectCenterX = box.X + box.Width / 2.0;
        double subjectCenterY = box.Y + box.Height / 2.0;
        double finalX = subjectCenterX - paddedW / 2.0;
        double finalY = subjectCenterY - paddedH / 2.0;

        return Clamp(imageWidth, imageHeight, finalX, finalY, paddedW, paddedH);
    }

    /// <summary>
    /// Centres a rectangle of the given size inside the image, preferring
    /// a square aspect ratio. Used when no subject was detected at all.
    /// </summary>
    private static CropRegion CentredSquare(int imageWidth, int imageHeight)
    {
        double side = Math.Min(imageWidth, imageHeight);
        double x = (imageWidth - side) / 2.0;
        double y = (imageHeight - side) / 2.0;
        return Clamp(imageWidth, imageHeight, x, y, side, side);
    }

    /// <summary>
    /// Clamps the rectangle so it stays inside the image. If the requested
    /// rectangle is larger than the image, it is shrunk proportionally and
    /// recentred so the subject remains visible.
    /// </summary>
    private static CropRegion Clamp(int imageWidth, int imageHeight, double x, double y, double width, double height)
    {
        if (width > imageWidth)
        {
            double s = imageWidth / width;
            width *= s;
            height *= s;
            x = (imageWidth - width) / 2.0;
        }

        if (height > imageHeight)
        {
            double s = imageHeight / height;
            width *= s;
            height *= s;
            x = (imageWidth - width) / 2.0;
            y = (imageHeight - height) / 2.0;
        }

        if (x < 0) x = 0;
        if (y < 0) y = 0;
        if (x + width > imageWidth) x = imageWidth - width;
        if (y + height > imageHeight) y = imageHeight - height;

        return new CropRegion
        {
            X = Math.Round(x),
            Y = Math.Round(y),
            Width = Math.Round(width),
            Height = Math.Round(height)
        };
    }

    /// <summary>Short human-readable description of how the crop was framed.</summary>
    private static string DescribeFraming(string intent, DetectedSubject subject, CropRegion crop, int imageWidth, int imageHeight)
    {
        double prominence = (crop.Width * crop.Height) / Math.Max(1.0, (double)imageWidth * imageHeight);
        return intent switch
        {
            "Profile Photo"    => $"Centered the detected head horizontally and framed head, neck, and shoulders with a square composition (subject occupies {prominence:P0} of the crop).",
            "Pet Portrait"     => $"Kept the full detected pet visible with comfortable margins so ears, tail and paws are not clipped (subject occupies {prominence:P0} of the crop).",
            "Product Image"    => $"Framed the detected product with professional margins suitable for e-commerce and catalogues (subject occupies {prominence:P0} of the crop).",
            "Food Photography" => $"Highlighted the detected food item with presentation-focused margins (subject occupies {prominence:P0} of the crop).",
            "Hero Banner"      => $"Picked the widest landscape crop that still includes the subject so surrounding context is preserved (subject occupies {prominence:P0} of the crop).",
            "Object Focus"     => $"Tightened the crop around the detected subject with minimal margins so the subject dominates the frame (subject occupies {prominence:P0} of the crop).",
            _                  => $"Balanced crop around the detected subject (subject occupies {prominence:P0} of the crop)."
        };
    }

    #endregion

    private enum SubjectKind { None, Person, Pet, Food, Object, SmartCrop }

    /// <summary>Bundle of the picked detection: its bounding box, kind, label and confidence.</summary>
    private readonly record struct DetectedSubject(CropRegion Box, SubjectKind Kind, string Label, double Confidence);
}
