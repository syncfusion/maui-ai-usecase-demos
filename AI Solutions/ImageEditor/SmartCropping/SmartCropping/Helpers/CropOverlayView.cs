using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace SmartCropping.Helpers;

/// <summary>
/// Draws the AI-suggested crop preview rectangle on top of the image editor.
/// </summary>
/// <remarks>
/// The overlay renders four semi-transparent rectangles that dim the area
/// outside the crop and a stroked border that traces the crop region. The
/// overlay only paints when <see cref="IsOverlayVisible"/> is true and a
/// non-empty rectangle has been supplied. Coordinates are in display space
/// (pixels relative to the GraphicsView itself).
/// </remarks>
public sealed class CropOverlayView : GraphicsView
{
    #region Bindable Properties

    /// <summary>
    /// Bindable property for <see cref="IsOverlayVisible"/>.
    /// </summary>
    public static readonly BindableProperty IsOverlayVisibleProperty = BindableProperty.Create(
        nameof(IsOverlayVisible),
        typeof(bool),
        typeof(CropOverlayView),
        false,
        propertyChanged: OnIsOverlayVisibleChanged);

    /// <summary>
    /// Bindable property for <see cref="OverlayX"/>.
    /// </summary>
    public static readonly BindableProperty OverlayXProperty = BindableProperty.Create(
        nameof(OverlayX),
        typeof(double),
        typeof(CropOverlayView),
        0d,
        propertyChanged: OnOverlayGeometryChanged);

    /// <summary>
    /// Bindable property for <see cref="OverlayY"/>.
    /// </summary>
    public static readonly BindableProperty OverlayYProperty = BindableProperty.Create(
        nameof(OverlayY),
        typeof(double),
        typeof(CropOverlayView),
        0d,
        propertyChanged: OnOverlayGeometryChanged);

    /// <summary>
    /// Bindable property for <see cref="OverlayWidth"/>.
    /// </summary>
    public static readonly BindableProperty OverlayWidthProperty = BindableProperty.Create(
        nameof(OverlayWidth),
        typeof(double),
        typeof(CropOverlayView),
        0d,
        propertyChanged: OnOverlayGeometryChanged);

    /// <summary>
    /// Bindable property for <see cref="OverlayHeight"/>.
    /// </summary>
    public static readonly BindableProperty OverlayHeightProperty = BindableProperty.Create(
        nameof(OverlayHeight),
        typeof(double),
        typeof(CropOverlayView),
        0d,
        propertyChanged: OnOverlayGeometryChanged);

    #endregion

    private readonly CropOverlayDrawable _drawable;

    /// <summary>
    /// Initializes a new instance of the <see cref="CropOverlayView"/> class.
    /// </summary>
    public CropOverlayView()
    {
        _drawable = new CropOverlayDrawable();
        Drawable = _drawable;
        InputTransparent = true;
    }

    #region Properties

    /// <summary>
    /// Gets or sets whether the overlay should be painted.
    /// </summary>
    public bool IsOverlayVisible
    {
        get => (bool)GetValue(IsOverlayVisibleProperty);
        set => SetValue(IsOverlayVisibleProperty, value);
    }

    /// <summary>
    /// Gets or sets the horizontal position of the crop rectangle in
    /// display-space pixels (relative to the overlay view).
    /// </summary>
    public double OverlayX
    {
        get => (double)GetValue(OverlayXProperty);
        set => SetValue(OverlayXProperty, value);
    }

    /// <summary>
    /// Gets or sets the vertical position of the crop rectangle in
    /// display-space pixels (relative to the overlay view).
    /// </summary>
    public double OverlayY
    {
        get => (double)GetValue(OverlayYProperty);
        set => SetValue(OverlayYProperty, value);
    }

    /// <summary>
    /// Gets or sets the width of the crop rectangle in display-space pixels.
    /// </summary>
    public double OverlayWidth
    {
        get => (double)GetValue(OverlayWidthProperty);
        set => SetValue(OverlayWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the height of the crop rectangle in display-space pixels.
    /// </summary>
    public double OverlayHeight
    {
        get => (double)GetValue(OverlayHeightProperty);
        set => SetValue(OverlayHeightProperty, value);
    }

    #endregion

    #region Property Changed Handlers

    private static void OnIsOverlayVisibleChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is CropOverlayView view)
        {
            view._drawable.IsVisible = (bool)newValue;
            view.Invalidate();
        }
    }

    private static void OnOverlayGeometryChanged(BindableObject? bindable, object oldValue, object newValue)
    {
        if (bindable is not CropOverlayView view)
        {
            return;
        }

        view._drawable.OverlayX = (float)view.OverlayX;
        view._drawable.OverlayY = (float)view.OverlayY;
        view._drawable.OverlayWidth = (float)view.OverlayWidth;
        view._drawable.OverlayHeight = (float)view.OverlayHeight;
        view.Invalidate();
    }

    #endregion
}

/// <summary>
/// Drawable that paints the dim mask and blue border for the AI crop preview.
/// </summary>
internal sealed class CropOverlayDrawable : IDrawable
{
    /// <summary>
    /// Mask over everything outside the crop region. Matches the reference
    /// design's <c>rgba(0, 0, 0, 0.42)</c> dim layer.
    /// </summary>
    private static readonly Color DimColor = Color.FromRgba(0f, 0f, 0f, 0.42f);

    /// <summary>
    /// Crop region border. Matches the reference's Syncfusion blue border.
    /// </summary>
    private static readonly Color BorderColor = Color.FromArgb("#0066F5");

    /// <summary>Whether the overlay should be drawn at all.</summary>
    public bool IsVisible { get; set; }

    /// <summary>Display-space X position of the crop rectangle.</summary>
    public float OverlayX { get; set; }

    /// <summary>Display-space Y position of the crop rectangle.</summary>
    public float OverlayY { get; set; }

    /// <summary>Display-space width of the crop rectangle.</summary>
    public float OverlayWidth { get; set; }

    /// <summary>Display-space height of the crop rectangle.</summary>
    public float OverlayHeight { get; set; }

    /// <inheritdoc />
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (!IsVisible)
        {
            return;
        }

        if (OverlayWidth <= 0 || OverlayHeight <= 0)
        {
            return;
        }

        float viewWidth = dirtyRect.Width;
        float viewHeight = dirtyRect.Height;

        float x = Math.Clamp(OverlayX, 0, viewWidth);
        float y = Math.Clamp(OverlayY, 0, viewHeight);
        float width = Math.Min(OverlayWidth, viewWidth - x);
        float height = Math.Min(OverlayHeight, viewHeight - y);

        if (width <= 0 || height <= 0)
        {
            return;
        }

        canvas.FillColor = DimColor;

        // Top strip (above the crop rectangle).
        canvas.FillRectangle(0, 0, viewWidth, y);

        // Bottom strip (below the crop rectangle).
        canvas.FillRectangle(0, y + height, viewWidth, viewHeight - (y + height));

        // Left strip (between top and bottom strips, outside the crop rectangle).
        canvas.FillRectangle(0, y, x, height);

        // Right strip (between top and bottom strips, outside the crop rectangle).
        canvas.FillRectangle(x + width, y, viewWidth - (x + width), height);

        // Blue border tracing the crop region. 2px to match the reference
        // design's 2px border around the suggested crop box.
        canvas.StrokeColor = BorderColor;
        canvas.StrokeSize = 2f;
        canvas.DrawRectangle(x, y, width, height);
    }
}
