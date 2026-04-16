using SharpMod.Demo.Wpf.Themes;
using SkiaSharp;
using System;

namespace SharpMod.Demo.Wpf.Renderers;

public class SpectrumRenderer
{
    private readonly SKPaint _barPaint = new()
    {
        Style = SKPaintStyle.Fill,
        IsAntialias = false
    };

    public void Draw(SKCanvas canvas, SKSize size,
                     float[] bands, int bandCount)
    {
        float w = size.Width, h = size.Height;
        canvas.Clear(Ft2Theme.Background);

        if (bands == null || bands.Length == 0 || bandCount == 0) return;

        int count = Math.Min(bands.Length, bandCount);
        float barWidth = w / count;
        float gap = Math.Max(1, barWidth * 0.15f);

        for (int i = 0; i < count; i++)
        {
            float val = Math.Clamp(bands[i], 0f, 1f);
            float barH = val * h;
            float x = i * barWidth;

            if (barH < 1) continue;

            // Couleur identique au Blazor : vert→jaune→rouge
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

            _barPaint.Color = new SKColor(r, g, 40);
            canvas.DrawRect(x + gap * 0.5f, h - barH,
                            barWidth - gap, barH, _barPaint);
        }
    }
}
