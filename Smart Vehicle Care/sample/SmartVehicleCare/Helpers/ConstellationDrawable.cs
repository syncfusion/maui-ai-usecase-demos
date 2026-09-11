using Microsoft.Maui.Graphics;

namespace SmartVehicleCare.Helpers;

/// <summary>
/// Draws decorative constellation-style geometric lines for the splash page background.
/// </summary>
public class ConstellationDrawable : IDrawable
{
    public Color LineColor { get; set; } = Color.FromArgb("#30FFFFFF");
    public Color DotColor { get; set; } = Color.FromArgb("#50FFFFFF");

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        float w = dirtyRect.Width;
        float h = dirtyRect.Height;

        // Vertices positioned in the upper-right constellation area
        var pts = new (float x, float y)[]
        {
            (w * 0.68f, h * 0.04f),
            (w * 0.91f, h * 0.09f),
            (w * 0.98f, h * 0.20f),
            (w * 0.82f, h * 0.24f),
            (w * 0.94f, h * 0.34f),
        };

        canvas.StrokeColor = LineColor;
        canvas.StrokeSize = 1f;

        canvas.DrawLine(pts[0].x, pts[0].y, pts[1].x, pts[1].y);
        canvas.DrawLine(pts[1].x, pts[1].y, pts[2].x, pts[2].y);
        canvas.DrawLine(pts[2].x, pts[2].y, pts[3].x, pts[3].y);
        canvas.DrawLine(pts[3].x, pts[3].y, pts[0].x, pts[0].y);
        canvas.DrawLine(pts[1].x, pts[1].y, pts[3].x, pts[3].y);
        canvas.DrawLine(pts[2].x, pts[2].y, pts[4].x, pts[4].y);

        canvas.FillColor = DotColor;
        foreach (var (x, y) in pts)
            canvas.FillCircle(x, y, 3f);
    }
}
