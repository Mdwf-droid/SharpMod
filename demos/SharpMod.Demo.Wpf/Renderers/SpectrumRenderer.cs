using SkiaSharp;
using System;

public class SpectrumRenderer
{
    private static readonly SKPaint BgPaint = new()
    {
        Color = SKColor.Parse("#0B0E14"),
        IsAntialias = false
    };
    private static readonly SKPaint GridPaint = new()
    {
        Color = new SKColor(0x40, 0x60, 0x90, 0x20),
        StrokeWidth = 0.5f,
        IsAntialias = false
    };
    private static readonly SKPaint[] BarPaints;

    static SpectrumRenderer()
    {
        BarPaints = new SKPaint[256];
        for (int i = 0; i < 256; i++)
        {
            float val = i / 255f;
            byte r, g;
            if (val < 0.5f)
            {
                r = (byte)Math.Min(255, val * 384);
                g = 192;
            }
            else
            {
                r = 192;
                g = (byte)Math.Max(0, (1f - (val - 0.5f) * 2f) * 192);
            }
            BarPaints[i] = new SKPaint
            {
                Color = new SKColor(r, g, 40),
                IsAntialias = false
            };
        }
    }

    public void Draw(SKCanvas canvas, int width, int height,
                     float[]? bands, int bandCount)
    {
        canvas.DrawRect(0, 0, width, height, BgPaint);

        // Grille
        for (int g = 1; g <= 4; g++)
        {
            float gy = height * g / 5f;
            canvas.DrawLine(0, gy, width, gy, GridPaint);
        }

        if (bands == null || bandCount == 0) return;

        int count = Math.Min(bands.Length, bandCount);
        float barWidth = (float)width / count;
        float gap = Math.Max(1, barWidth * 0.15f);

        for (int i = 0; i < count; i++)
        {
            float val = Math.Clamp(bands[i], 0f, 1f);
            float barH = val * height;
            if (barH < 1) continue;

            float x = i * barWidth + gap * 0.5f;
            int colorIdx = (int)(val * 255);

            canvas.DrawRect(x, height - barH, barWidth - gap, barH,
                BarPaints[colorIdx]);
        }
    }
}
