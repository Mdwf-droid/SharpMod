using System;
using SharpMod.Demo.Wpf.Themes;
using SkiaSharp;

namespace SharpMod.Demo.Wpf.Renderers;

/// <summary>
/// Dessine des VU-meters verticaux par canal tracker.
/// Données : float[] vuLevels (par canal, 0..1).
/// </summary>
public class VuMeterRenderer
{
    private float[]? _smoothPeaks;

    private readonly SKPaint _labelPaint = new()
    {
        Color = Ft2Theme.RowNumberColor,
        TextSize = 9,
        IsAntialias = true,
        TextAlign = SKTextAlign.Center,
    };

    /// <summary>
    /// Dessine les VU-meters.
    /// </summary>
    /// <param name="canvas">SKCanvas SkiaSharp</param>
    /// <param name="size">Taille du contrôle</param>
    /// <param name="vuLevels">float[channels] — peaks 0..1 par canal</param>
    /// <param name="channels">Nombre de canaux tracker</param>
    public void Draw(SKCanvas canvas, SKSize size, float[] vuLevels, int channels)
    {
        canvas.Clear(Ft2Theme.PanelInset);

        if (channels <= 0 || vuLevels == null)
            return;

        // Init / resize smooth peaks
        if (_smoothPeaks == null || _smoothPeaks.Length != channels)
            _smoothPeaks = new float[channels];

        float cellW = size.Width / channels;
        float vuW = Math.Min(cellW * 0.6f, 14f);
        int totalSegs = 12;
        float segH = (size.Height - 14) / totalSegs; // 14 = marge label bas
        float topMargin = 2;

        for (int ch = 0; ch < channels; ch++)
        {
            float raw = ch < vuLevels.Length ? Math.Max(0, Math.Min(1, vuLevels[ch])) : 0;

            // Lissage : montée rapide, descente lente
            if (raw > _smoothPeaks[ch])
                _smoothPeaks[ch] = raw;
            else
                _smoothPeaks[ch] = _smoothPeaks[ch] * 0.92f + raw * 0.08f;

            int litSegs = Math.Min(totalSegs,
                (int)(_smoothPeaks[ch] * totalSegs + 0.5f));

            float vuX = ch * cellW + (cellW - vuW) / 2f;

            for (int s = 0; s < totalSegs; s++)
            {
                SKColor color;
                if (s >= litSegs)
                    color = Ft2Theme.VuOff;
                else if (s < 6)
                    color = Ft2Theme.VuGreen;
                else if (s < 9)
                    color = Ft2Theme.VuYellow;
                else
                    color = Ft2Theme.VuRed;

                using var paint = new SKPaint { Color = color };
                float sy = size.Height - 12 - (s + 1) * segH;
                canvas.DrawRect(vuX, sy, vuW, segH - 1, paint);
            }

            // Label canal
            canvas.DrawText($"{ch + 1}",
                ch * cellW + cellW / 2f,
                size.Height - 1,
                _labelPaint);
        }
    }
}
