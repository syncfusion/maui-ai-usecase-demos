using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Maui.Graphics;

namespace PatientCompanion.Controls;

public sealed class CircularProgressDrawable : IDrawable
{
    public static CircularProgressDrawable Instance { get; } = new();

    private CircularProgressDrawable()
    {
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        var centerX = dirtyRect.Center.X;
        var centerY = dirtyRect.Center.Y;

        float radius = 28;

        canvas.StrokeSize = 8;
        canvas.StrokeColor = Color.FromArgb("#E5E7EB");

        canvas.DrawCircle(centerX, centerY, radius);

        canvas.StrokeColor = Color.FromArgb("#0F766E");

        canvas.DrawArc(
            centerX - radius,
            centerY - radius,
            radius * 2,
            radius * 2,
            -90,
            331,
            false,
            false);
    }
}
