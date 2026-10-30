using Azure;
using Azure.AI.Vision.ImageAnalysis;
using System.Diagnostics;

namespace SmartCropping;

/// <summary>
/// Deterministic Smart Cropping implementation backed by Azure AI Vision
/// (Image Analysis 4.0). Unlike <see cref="SmartCropService"/>, which asks
/// a vision model to interpret a free-form prompt, this service asks the
/// service for the actual bounding boxes it found and then applies
/// intent-specific framing rules locally.
/// </summary>
/// <remarks>
/// <para>
/// <b>Pipeline.</b> image → Azure AI Vision (People + Objects + SmartCrops
/// in one call) → detected bounding boxes → local framing rules →
/// crop coordinates. The framing rules are mathematical, not prompt-based,
/// so the same image and intent always produce the same crop.
/// </para>
/// <para>
/// <b>What Azure AI Vision can detect (and what it cannot).</b>
/// </para>
/// <list type="bullet">
///   <item>
///     <description><b>Coordinates are real.</b> Bounding boxes come back
///     in original-image pixel space and rarely overshoot the border, so
///     crops are reproducible across runs.</description>
///   </item>
///   <item>
///     <description><b>People detection returns a full-person box, not a
///     face box.</b> There are no face landmarks, no head pose, no
///     separate face bounding box. Profile Photo framing therefore
///     <i>estimates</i> the head region from the top portion of the
///     person box (see <see cref="FrameProfilePhoto"/>). True face-aware
///     cropping would require the dedicated Azure Face service.</description>
///   </item>
///   <item>
///     <description><b>Object detection uses an 86-class taxonomy.</b>
///     Pets (dog, cat, bird, horse) and some individual food items
///     (banana, pizza, cake, bottle, bowl, ...) are covered. Generic
///     food like "plate of pasta" or "birthday cake" is not. Food
///     Photography falls back to the largest non-person/pet object when
///     no food tag matches.</description>
///   </item>
///   <item>
///     <description><b>SmartCrops returns ROI suggestions with different
///     aspect ratios, prioritising faces.</b> Useful as a fallback and as
///     the preferred source for Hero Banner framing.</description>
///   </item>
///   <item>
///     <description><b>No semantic understanding.</b> The service cannot
///     tell "this is a LinkedIn photo" vs "this is a group shot", or
///     "this is food" vs "this is a product on a table". Intent
///     interpretation is done locally from the bounding boxes and their
///     tags.</description>
///   </item>
/// </list>
/// <para>
/// <b>Why framing is implemented mathematically rather than via prompts.</b>
/// This is a parallel implementation to the Azure OpenAI path; the goal
/// here is determinism. Same image + same intent = same crop, every time.
/// Prompts produce variance; bounding boxes plus arithmetic do not.
/// </para>
/// <para>
/// <b>Switching engines.</b> This service is parallel to
/// <see cref="SmartCropService"/>. The DI factory in
/// <c>MauiProgram.cs</c> picks one at compile time; flip
/// <c>UseAzureVision</c> to compare.
/// </para>
/// </remarks>
public sealed class AzureVisionSmartCropService : ISmartCropService
{
    /// <summary>
    /// Tags from the Azure AI Vision Objects taxonomy that identify pets.
    /// </summary>
    private static readonly HashSet<string> PetTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "dog", "cat", "bird", "horse"
    };

    /// <summary>
    /// Tags from the Objects taxonomy that identify food items. Coverage
    /// is limited — individual items like banana, pizza, cake are
    /// detected, but "plate of pasta" or "birthday cake" are not. The
    /// Food Photography intent falls back to the largest non-person/pet
    /// object when none of these tags match.
    /// </summary>
    private static readonly HashSet<string> FoodTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "food", "banana", "apple", "orange", "broccoli", "carrot", "hot dog",
        "sandwich", "pizza", "cake", "donut", "bottle", "cup", "bowl",
        "fruit", "vegetable", "bread", "egg"
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
            // One round trip gets us everything we need: people boxes,
            // object boxes (with tags) and ROI / smart-crop suggestions.
            ImageAnalysisResult analysis = await client.AnalyzeAsync(
                BinaryData.FromBytes(imageBytes),
                VisualFeatures.People | VisualFeatures.Objects | VisualFeatures.SmartCrops,
                cancellationToken: cancellationToken);

            DetectedSubject subject = SelectSubject(analysis, intent);

            CropRegion crop = intent switch
            {
                "Profile Photo" => FrameProfilePhoto(subject, imageWidth, imageHeight),
                "Pet Portrait" => FramePetPortrait(subject, imageWidth, imageHeight),
                "Product Image" => FrameProduct(subject, imageWidth, imageHeight),
                "Food Photography" => FrameFood(subject, imageWidth, imageHeight),
                "Hero Banner" => FrameHeroBanner(analysis, subject, imageWidth, imageHeight),
                "Object Focus" => FrameObjectFocus(subject, imageWidth, imageHeight),
                _ => FrameDefault(subject, imageWidth, imageHeight),
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
    /// Picks the bounding box to frame, with the priority driven by the
    /// selected intent. Different intents want different subjects, so
    /// the previous generic "person → pet → food → object" priority is
    /// replaced with intent-specific selection.
    /// </summary>
    private static DetectedSubject SelectSubject(ImageAnalysisResult analysis, string intent)
    {
        IReadOnlyList<DetectedPerson>? people = analysis.People?.Values;
        IReadOnlyList<DetectedObject>? objects = analysis.Objects?.Values;

        return intent switch
        {
            "Profile Photo" => SelectForProfilePhoto(people, objects),
            "Pet Portrait" => SelectForPetPortrait(people, objects),
            "Product Image" => SelectForProduct(objects),
            "Food Photography" => SelectForFood(objects),
            "Hero Banner" => SelectForHeroBanner(people, objects, analysis),
            "Object Focus" => SelectForObjectFocus(objects, people),
            _ => SelectDefault(people, objects, analysis),
        };
    }

    /// <summary>
    /// Profile Photo MUST be a person. If none was detected, fall back
    /// to the largest object (could be a statue, a logo, etc.). Never
    /// return "no subject" for this intent.
    /// </summary>
    private static DetectedSubject SelectForProfilePhoto(
        IReadOnlyList<DetectedPerson>? people,
        IReadOnlyList<DetectedObject>? objects)
    {
        if (people is { Count: > 0 })
        {
            return LargestPerson(people);
        }
        return NoSubject();
    }

    /// <summary>
    /// Pet Portrait prefers a detected pet; falls back to a person (a
    /// human portrait is a reasonable second choice if the user picked
    /// Pet Portrait for a people photo); falls back to the largest
    /// object.
    /// </summary>
    private static DetectedSubject SelectForPetPortrait(
        IReadOnlyList<DetectedPerson>? people,
        IReadOnlyList<DetectedObject>? objects)
    {
        if (objects is { Count: > 0 })
        {
            DetectedSubject? pet = LargestObjectWithTag(objects, PetTags, SubjectKind.Pet);
            if (pet is not null)
            {
                return pet.Value;
            }
        }
        if (people is { Count: > 0 })
        {
            return LargestPerson(people);
        }
        if (objects is { Count: > 0 })
        {
            return LargestObject(objects);
        }
        return NoSubject();
    }

    /// <summary>
    /// Product Image prefers a non-pet object. People are excluded so a
    /// model wearing a product isn't framed as the subject.
    /// </summary>
    private static DetectedSubject SelectForProduct(IReadOnlyList<DetectedObject>? objects)
    {
        if (objects is { Count: > 0 })
        {
            DetectedSubject? product = LargestObjectExcluding(objects, PetTags);
            if (product is not null)
            {
                return product.Value;
            }
            return LargestObject(objects);
        }
        return NoSubject();
    }

    /// <summary>
    /// Food Photography prefers a food-tagged object. If none matches,
    /// fall back to the largest non-pet object (the dish, plate, or
    /// table setting that the Objects taxonomy couldn't classify).
    /// </summary>
    private static DetectedSubject SelectForFood(IReadOnlyList<DetectedObject>? objects)
    {
        if (objects is { Count: > 0 })
        {
            DetectedSubject? food = LargestObjectWithTag(objects, FoodTags, SubjectKind.Food);
            if (food is not null)
            {
                return food.Value;
            }
            DetectedSubject? nonPet = LargestObjectExcluding(objects, PetTags);
            if (nonPet is not null)
            {
                return nonPet.Value;
            }
            return LargestObject(objects);
        }
        return NoSubject();
    }

    /// <summary>
    /// Hero Banner uses the most prominent subject (people first, then
    /// objects) and lets the smart-crop suggestions do the actual
    /// landscape framing. Falls back to a smart-crop ROI if no subject
    /// was detected.
    /// </summary>
    private static DetectedSubject SelectForHeroBanner(
        IReadOnlyList<DetectedPerson>? people,
        IReadOnlyList<DetectedObject>? objects,
        ImageAnalysisResult analysis)
    {
        if (people is { Count: > 0 })
        {
            return LargestPerson(people);
        }
        if (objects is { Count: > 0 })
        {
            return LargestObject(objects);
        }
        DetectedSubject? smart = FirstSmartCrop(analysis);
        if (smart is not null)
        {
            return smart.Value;
        }
        return NoSubject();
    }

    /// <summary>
    /// Object Focus prefers a non-pet object. If only people/pets were
    /// detected, the largest of those is used as a fallback.
    /// </summary>
    private static DetectedSubject SelectForObjectFocus(
        IReadOnlyList<DetectedObject>? objects,
        IReadOnlyList<DetectedPerson>? people)
    {
        if (objects is { Count: > 0 })
        {
            DetectedSubject? obj = LargestObjectExcluding(objects, PetTags);
            if (obj is not null)
            {
                return obj.Value;
            }
            return LargestObject(objects);
        }
        if (people is { Count: > 0 })
        {
            return LargestPerson(people);
        }
        return NoSubject();
    }

    private static DetectedSubject SelectDefault(
        IReadOnlyList<DetectedPerson>? people,
        IReadOnlyList<DetectedObject>? objects,
        ImageAnalysisResult analysis)
    {
        if (people is { Count: > 0 })
        {
            return LargestPerson(people);
        }
        if (objects is { Count: > 0 })
        {
            DetectedSubject? pet = LargestObjectWithTag(objects, PetTags, SubjectKind.Pet);
            if (pet is not null)
            {
                return pet.Value;
            }
            return LargestObject(objects);
        }
        DetectedSubject? smart = FirstSmartCrop(analysis);
        if (smart is not null)
        {
            return smart.Value;
        }
        return NoSubject();
    }

    // ---- Selection helpers ----

    private static DetectedSubject LargestPerson(IReadOnlyList<DetectedPerson> people)
    {
        DetectedPerson largest = people
            .OrderByDescending(p => p.BoundingBox.Width * p.BoundingBox.Height)
            .First();
        return new DetectedSubject(
            ToCropRegion(largest.BoundingBox),
            SubjectKind.Person,
            "person",
            largest.Confidence);
    }

    private static DetectedSubject LargestObject(IReadOnlyList<DetectedObject> objects)
    {
        DetectedObject largest = objects
            .OrderByDescending(o => o.BoundingBox.Width * o.BoundingBox.Height)
            .First();
        string primaryTag = largest.Tags.Count > 0 ? largest.Tags[0].Name : "object";
        return new DetectedSubject(
            ToCropRegion(largest.BoundingBox),
            SubjectKind.Object,
            primaryTag,
            1.0);
    }

    private static DetectedSubject? LargestObjectWithTag(
        IReadOnlyList<DetectedObject> objects,
        HashSet<string> tags,
        SubjectKind kind)
    {
        DetectedObject? match = objects
            .SelectMany(o => o.Tags.Select(t => (Object: o, Tag: t)))
            .Where(x => tags.Contains(x.Tag.Name))
            .OrderByDescending(x => x.Object.BoundingBox.Width * x.Object.BoundingBox.Height)
            .Select(x => x.Object)
            .FirstOrDefault();

        if (match is null)
        {
            return null;
        }

        string primaryTag = match.Tags[0].Name;
        return new DetectedSubject(
            ToCropRegion(match.BoundingBox),
            kind,
            primaryTag,
            1.0);
    }

    private static DetectedSubject? LargestObjectExcluding(
        IReadOnlyList<DetectedObject> objects,
        HashSet<string> excludeTags)
    {
        DetectedObject? match = objects
            .Where(o => !o.Tags.Any(t => excludeTags.Contains(t.Name)))
            .OrderByDescending(o => o.BoundingBox.Width * o.BoundingBox.Height)
            .FirstOrDefault();

        if (match is null)
        {
            return null;
        }

        string primaryTag = match.Tags.Count > 0 ? match.Tags[0].Name : "object";
        return new DetectedSubject(
            ToCropRegion(match.BoundingBox),
            SubjectKind.Object,
            primaryTag,
            1.0);
    }

    private static DetectedSubject? FirstSmartCrop(ImageAnalysisResult analysis)
    {
        var smart = analysis.SmartCrops?.Values;
        if (smart is { Count: > 0 })
        {
            return new DetectedSubject(
                ToCropRegion(smart[0].BoundingBox),
                SubjectKind.SmartCrop,
                "smart-crop",
                1.0);
        }
        return null;
    }

    private static DetectedSubject NoSubject() =>
        new(new CropRegion { X = 0, Y = 0, Width = 1, Height = 1 },
            SubjectKind.None, "none", 0.0);

    private static CropRegion ToCropRegion(ImageBoundingBox box) =>
        new() { X = box.X, Y = box.Y, Width = box.Width, Height = box.Height };

    #endregion

    #region Intent-specific framing

    /// <summary>
    /// Professional head-and-shoulders profile photo.
    /// </summary>
    /// <remarks>
    /// <b>Limitation.</b> Azure AI Vision's People detection returns a
    /// bounding box around the entire person, not the face. We do not
    /// have face landmarks. To approximate a head-and-shoulders crop, we
    /// estimate the head region from the top portion of the person box:
    /// <list type="bullet">
    ///   <item><description>Head height ≈ 25% of person height (typical
    ///   adult head-to-body ratio).</description></item>
    ///   <item><description>Head width ≈ 50% of person width (head is
    ///   narrower than shoulders).</description></item>
    /// </list>
    /// The crop is then built around the estimated head, centred
    /// horizontally and positioned so the head sits in the upper third.
    /// This is a mathematical approximation. For accurate face-aware
    /// cropping, the dedicated Azure Face service would be required.
    /// </remarks>
    private static CropRegion FrameProfilePhoto(
    DetectedSubject subject,
    int imageWidth,
    int imageHeight)
    {
        if (subject.Kind == SubjectKind.None)
        {
            return CentredSquare(imageWidth, imageHeight);
        }

        CropRegion box = subject.Box;

        // Use the upper half of whatever Azure detected.
        // This works for:
        // - full-body photos
        // - half-body photos
        // - portrait photos
        // much better than guessing head size.

        double visibleUpperBodyHeight = box.Height * 0.25;

        double centerX = box.X + box.Width / 2.0;

        // Portrait crop.
        double cropWidth = box.Width * 1.15;
        double cropHeight = visibleUpperBodyHeight * 1.25;

        // Keep square avatars looking good.
        cropWidth = Math.Max(cropWidth, cropHeight * 0.90);

        double x = centerX - cropWidth / 2.0;

        // Focus on upper body.
        double y = box.Y - (visibleUpperBodyHeight * 0.10);

        return Clamp(
            imageWidth,
            imageHeight,
            x,
            y,
            cropWidth,
            cropHeight);
    }

    /// <summary>
    /// Pet Portrait: the full pet is visible with comfortable margins.
    /// Pet detection may not capture extremities (ears, tail, paws) that
    /// extend beyond the bounding box, so the minimum margin is larger
    /// than for other intents.
    /// </summary>
    private static CropRegion FramePetPortrait(
        DetectedSubject subject,
        int imageWidth,
        int imageHeight)
    {
        // 15% of the subject's smaller dimension as minimum margin,
        // guarding against ears, tail, paws that extend outside the box.
        // Target prominence 0.60 keeps the pet at roughly 55–65% of the
        // crop area as required.
        return FrameWithProminence(
            subject, imageWidth, imageHeight,
            subjectMarginFraction: 0.15,
            targetProminence: 0.60);
    }

    /// <summary>
    /// Product Image: the full product is visible and dominates the
    /// crop. Products are typically well-bounded by the Objects
    /// taxonomy, so a moderate margin is enough.
    /// </summary>
    private static CropRegion FrameProduct(
        DetectedSubject subject,
        int imageWidth,
        int imageHeight)
    {
        return FrameWithProminence(
            subject, imageWidth, imageHeight,
            subjectMarginFraction: 0.08,
            targetProminence: 0.70);
    }

    /// <summary>
    /// Food Photography: the dish is the focal point with presentation
    /// margins. Azure AI Vision's food detection is limited, so we rely
    /// on the largest non-pet object as a fallback. The framing is
    /// slightly looser than Product Image so surrounding plate / table
    /// context is preserved.
    /// </summary>
    private static CropRegion FrameFood(
        DetectedSubject subject,
        int imageWidth,
        int imageHeight)
    {
        return FrameWithProminence(
            subject, imageWidth, imageHeight,
            subjectMarginFraction: 0.10,
            targetProminence: 0.65);
    }

    /// <summary>
    /// Hero Banner: wide landscape crop that preserves environmental
    /// context. Prefers a smart-crop suggestion with a wide aspect
    /// ratio that still contains the detected subject; falls back to a
    /// forced 16:9 landscape crop around the subject.
    /// </summary>
    private static CropRegion FrameHeroBanner(
        ImageAnalysisResult analysis,
        DetectedSubject subject,
        int imageWidth,
        int imageHeight)
    {
        var smart = analysis.SmartCrops?.Values;
        if (smart is { Count: > 0 } && subject.Kind != SubjectKind.None)
        {
            CropRegion subjectBox = subject.Box;
            CropRegion? best = smart
                .Select(c => ToCropRegion(c.BoundingBox))
                .Where(c => ContainsBox(c, subjectBox))
                .Where(c => c.Width >= c.Height)
                .OrderByDescending(c => c.Width / (double)Math.Max(1, c.Height))
                .FirstOrDefault();

            if (best is { Width: > 0 })
            {
                return Clamp(imageWidth, imageHeight, best.X, best.Y, best.Width, best.Height);
            }
        }

        // Fallback: forced 16:9 landscape crop with low prominence so
        // the subject sits inside a generous amount of context.
        return FrameWithProminence(
            subject, imageWidth, imageHeight,
            subjectMarginFraction: 0.05,
            targetProminence: 0.30,
            forceAspectRatio: 16.0 / 9.0);
    }

    /// <summary>
    /// Object Focus: tightest framing. The subject occupies most of the
    /// crop while a small margin guarantees no edge clipping. This is
    /// the most aggressive of the six intents.
    /// </summary>
    private static CropRegion FrameObjectFocus(
        DetectedSubject subject,
        int imageWidth,
        int imageHeight)
    {
        // Minimum margin is 4% of the subject's smaller dimension —
        // enough to keep the subject fully inside the crop even when
        // detection undershoots by a few pixels, while still letting
        // the subject dominate the frame.
        return FrameWithProminence(
            subject, imageWidth, imageHeight,
            subjectMarginFraction: 0.04,
            targetProminence: 0.82);
    }

    /// <summary>Default fallback: balanced crop around the detected subject.</summary>
    private static CropRegion FrameDefault(
        DetectedSubject subject,
        int imageWidth,
        int imageHeight)
    {
        return FrameWithProminence(
            subject, imageWidth, imageHeight,
            subjectMarginFraction: 0.08,
            targetProminence: 0.60);
    }

    #endregion

    #region Framing helpers

    /// <summary>
    /// Sizes a crop so the subject occupies roughly
    /// <paramref name="targetProminence"/> of the crop area, centres it
    /// on the subject, and enforces a minimum margin around the subject
    /// so it is never clipped.
    /// </summary>
    /// <param name="subjectMarginFraction">
    /// Minimum margin around the subject, as a fraction of the subject's
    /// smaller dimension. Used as a floor so the crop can never be
    /// smaller than <c>subject + 2 * margin</c> on either axis.
    /// </param>
    /// <param name="targetProminence">
    /// Target fraction of the crop area occupied by the subject (0–1).
    /// Higher = tighter crop. The actual prominence may be lower when
    /// the resulting crop exceeds the image dimensions or when the
    /// aspect ratio forces the crop wider than the subject.
    /// </param>
    /// <param name="forceAspectRatio">
    /// If set, the crop uses this width/height ratio instead of the
    /// subject's natural ratio. Used by Hero Banner to force landscape.
    /// </param>
    private static CropRegion FrameWithProminence(
        DetectedSubject subject,
        int imageWidth,
        int imageHeight,
        double subjectMarginFraction,
        double targetProminence,
        double? forceAspectRatio = null)
    {
        if (subject.Kind == SubjectKind.None)
        {
            return CentredSquare(imageWidth, imageHeight);
        }

        CropRegion box = subject.Box;

        // Step 1 — crop area from target prominence.
        // subject_area / crop_area = targetProminence
        // =>           crop_area = subject_area / targetProminence
        double subjectArea = (double)box.Width * box.Height;
        double prominence = Math.Clamp(targetProminence, 0.10, 0.95);
        double cropArea = subjectArea / prominence;

        // Step 2 — distribute the area across width and height.
        double cropWidth;
        double cropHeight;
        if (forceAspectRatio.HasValue)
        {
            double aspect = forceAspectRatio.Value;
            cropHeight = Math.Sqrt(cropArea / aspect);
            cropWidth = cropHeight * aspect;
        }
        else
        {
            // Preserve the subject's natural aspect ratio so the crop
            // looks proportional to what was detected.
            double subjectAspect = box.Width / Math.Max(1.0, (double)box.Height);
            cropHeight = Math.Sqrt(cropArea / subjectAspect);
            cropWidth = cropHeight * subjectAspect;
        }

        // Step 3 — enforce a minimum margin around the subject on both
        // axes. The margin is computed from the subject's own size so a
        // small subject in a large image gets a proportional margin,
        // not an enormous one.
        double minMargin = Math.Min((double)box.Width, box.Height) * subjectMarginFraction;
        double requiredWidth = box.Width + 2 * minMargin;
        double requiredHeight = box.Height + 2 * minMargin;
        if (cropWidth < requiredWidth)
        {
            cropWidth = requiredWidth;
        }
        if (cropHeight < requiredHeight)
        {
            cropHeight = requiredHeight;
        }

        // Step 4 — centre the crop on the subject's centre.
        double subjectCenterX = box.X + box.Width / 2.0;
        double subjectCenterY = box.Y + box.Height / 2.0;
        double x = subjectCenterX - cropWidth / 2.0;
        double y = subjectCenterY - cropHeight / 2.0;

        // Step 5 — clamp to image bounds (Clamp shrinks oversized
        // crops proportionally and recentres them).
        return Clamp(imageWidth, imageHeight, x, y, cropWidth, cropHeight);
    }

    /// <summary>
    /// Centres a square rectangle inside the image. Used when no
    /// subject was detected.
    /// </summary>
    private static CropRegion CentredSquare(int imageWidth, int imageHeight)
    {
        double side = Math.Min(imageWidth, imageHeight);
        double x = (imageWidth - side) / 2.0;
        double y = (imageHeight - side) / 2.0;
        return Clamp(imageWidth, imageHeight, x, y, side, side);
    }

    /// <summary>
    /// Clamps the rectangle so it stays inside the image. If the
    /// requested rectangle is larger than the image, it is shrunk
    /// proportionally and recentred so the subject remains visible.
    /// </summary>
    private static CropRegion Clamp(
        int imageWidth,
        int imageHeight,
        double x,
        double y,
        double width,
        double height)
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

        if (x < 0)
        {
            x = 0;
        }
        if (y < 0)
        {
            y = 0;
        }
        if (x + width > imageWidth)
        {
            x = imageWidth - width;
        }
        if (y + height > imageHeight)
        {
            y = imageHeight - height;
        }

        return new CropRegion
        {
            X = Math.Round(x),
            Y = Math.Round(y),
            Width = Math.Round(width),
            Height = Math.Round(height)
        };
    }

    /// <summary>Returns true when <paramref name="outer"/> fully contains <paramref name="inner"/>.</summary>
    private static bool ContainsBox(CropRegion outer, CropRegion inner) =>
        outer.X <= inner.X &&
        outer.Y <= inner.Y &&
        outer.X + outer.Width >= inner.X + inner.Width &&
        outer.Y + outer.Height >= inner.Y + inner.Height;

    /// <summary>Short human-readable description of how the crop was framed.</summary>
    private static string DescribeFraming(
        string intent,
        DetectedSubject subject,
        CropRegion crop,
        int imageWidth,
        int imageHeight)
    {
        double prominence = (crop.Width * crop.Height) /
            Math.Max(1.0, (double)imageWidth * imageHeight);

        string subjectDesc = subject.Kind switch
        {
            SubjectKind.Person => "detected person",
            SubjectKind.Pet => $"detected {subject.Label}",
            SubjectKind.Food => "detected food item",
            SubjectKind.Object => $"detected {subject.Label}",
            SubjectKind.SmartCrop => "smart-crop suggestion",
            _ => "image centre",
        };

        return intent switch
        {
            "Profile Photo" => $"Estimated the head region from the {subjectDesc} and framed head, neck, and shoulders with the head centred horizontally (subject occupies {prominence:P0} of the crop). Head position is approximated because Azure AI Vision People detection does not provide face landmarks.",
            "Pet Portrait" => $"Kept the full {subjectDesc} visible with generous margins so ears, tail, and paws are not clipped (subject occupies {prominence:P0} of the crop).",
            "Product Image" => $"Framed the {subjectDesc} with balanced margins so the product dominates without edge clipping (subject occupies {prominence:P0} of the crop).",
            "Food Photography" => $"Highlighted the {subjectDesc} with presentation-focused margins (subject occupies {prominence:P0} of the crop).",
            "Hero Banner" => $"Picked a wide landscape crop that includes the {subjectDesc} so surrounding context is preserved (subject occupies {prominence:P0} of the crop).",
            "Object Focus" => $"Tightened the crop around the {subjectDesc} with minimal margins so the subject dominates the frame (subject occupies {prominence:P0} of the crop).",
            _ => $"Balanced crop around the {subjectDesc} (subject occupies {prominence:P0} of the crop).",
        };
    }

    #endregion

    private enum SubjectKind { None, Person, Pet, Food, Object, SmartCrop }

    /// <summary>Bundle of the picked detection: its bounding box, kind, label and confidence.</summary>
    private readonly record struct DetectedSubject(CropRegion Box, SubjectKind Kind, string Label, double Confidence);
}