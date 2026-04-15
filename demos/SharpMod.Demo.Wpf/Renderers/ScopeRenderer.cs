using System;
using SharpMod.Demo.Wpf.Themes;
using SkiaSharp;

namespace SharpMod.Demo.Wpf.Renderers;

/// <summary>
/// Dessine un oscilloscope par canal tracker.
/// Données : float[][] scopeData (par canal, 128 samples, -1..1).
/// </summary>
public class ScopeRenderer
{
    private readonly SKPaint _borderPaint = new()
    {
        Color = Ft2Theme.ScopeBorder,
        StrokeWidth = 1,
        Style = SKPaintStyle.Stroke,
    };

    private readonly SKPaint _centerLinePaint = new()
    {
        Color = Ft2Theme.ScopeLine,
        StrokeWidth = 0.5f,
        Style = SKPaintStyle.Stroke,
    };

    private readonly SKPaint _labelPaint = new()
    {
        Color = Ft2Theme.RowNumberColor,
        TextSize = 9,
        IsAntialias = true,
    };

    private readonly SKPaint[] _wavePaints;

    public ScopeRenderer()
    {
        _wavePaints = new SKPaint[Ft2Theme.ScopeColors.Length];
        for (int i = 0; i < Ft2Theme.ScopeColors.Length; i++)
        {
            _wavePaints[i] = new SKPaint
            {
                Color = Ft2Theme.ScopeColors[i],
                StrokeWidth = 1.5f,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true,
            };
        }
    }

    /// <summary>
    /// Dessine les oscilloscopes.
    /// </summary>
    /// <param name="canvas">SKCanvas SkiaSharp</param>
    /// <param name="size">Taille du contrôle</param>
    /// <param name="scopeData">float[channels][128] — samples -1..1 par canal</param>
    /// <param name="channels">Nombre de canaux tracker</param>
    public void Draw(SKCanvas canvas, SKSize size, float[][] scopeData, int channels)
    {
        canvas.Clear(Ft2Theme.PanelInset);

        if (channels <= 0 || scopeData == null || scopeData.Length == 0)
            return;

        float cellW = size.Width / channels;
        float halfH = size.Height * 0.5f;
        float ampH = halfH * 0.85f;

        for (int ch = 0; ch < channels; ch++)
        {
            float x0 = ch * cellW;

            // Bordure verticale entre canaux
            if (ch > 0)
                canvas.DrawLine(x0, 0, x0, size.Height, _borderPaint);

            // Label "01", "02", ...
            canvas.DrawText($"{ch + 1:D2}", x0 + 2, 10, _labelPaint);

            // Ligne centrale
            canvas.DrawLine(x0 + 1, halfH, x0 + cellW - 1, halfH, _centerLinePaint);

            // Waveform
            if (ch >= scopeData.Length || scopeData[ch] == null || scopeData[ch].Length == 0)
                continue;

            var data = scopeData[ch];
            var paint = _wavePaints[ch % _wavePaints.Length];
            float scopeW = cellW - 2;
            float step = (float)data.Length / scopeW;

            using var path = new SKPath();
            bool started = false;

            for (int px = 0; px < (int)scopeW; px++)
            {
                int idx = Math.Min((int)(px * step), data.Length - 1);
                float val = Math.Max(-1f, Math.Min(1f, data[idx]));
                float py = halfH - val * ampH;

                if (!started)
                {
                    path.MoveTo(x0 + 1 + px, py);
                    started = true;
                }
                else
                {
                    path.LineTo(x0 + 1 + px, py);
                }
            }

            canvas.DrawPath(path, paint);
        }
    }
}
