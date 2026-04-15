using System;
using SharpMod.Demo.Wpf.Themes;
using SkiaSharp;

namespace SharpMod.Demo.Wpf.Renderers;

/// <summary>
/// Dessine le spectrum analyzer (barres verticales, style FT2).
/// Données : float[] bands (0..1), int bandCount.
/// </summary>
public class SpectrumRenderer
{
    private readonly SKPaint _gridPaint = new()
    {
        Color = Ft2Theme.ScopeLine,
        StrokeWidth = 0.5f,
    };

    /// <summary>
    /// Dessine le spectrum.
    /// </summary>
    /// <param name="canvas">SKCanvas SkiaSharp</param>
    /// <param name="size">Taille du contrôle</param>
    /// <param name="bands">float[bandCount] — niveaux 0..1</param>
    /// <param name="bandCount">Nombre de bandes</param>
    public void Draw(SKCanvas canvas, SKSize size, float[] bands, int bandCount)
    {
        canvas.Clear(Ft2Theme.PanelInset);

        if (bands == null || bandCount <= 0)
            return;

        float barW = size.Width / bandCount;
        float gap = Math.Max(1f, barW * 0.15f);
        float effectiveW = barW - gap;

        // Grille horizontale
        for (int g = 1; g <= 4; g++)
        {
            float gy = size.Height * g / 5f;
            canvas.DrawLine(0, gy, size.Width, gy, _gridPaint);
        }

        for (int b = 0; b < bandCount && b < bands.Length; b++)
        {
            float level = Math.Max(0, Math.Min(1, bands[b]));
            float barH = level * (size.Height - 2);
            float x = b * barW + gap / 2f;

            if (barH < 1) continue;

            // Gradient vert → jaune → rouge
            float greenZone = size.Height * 0.5f;
            float yellowZone = size.Height * 0.3f;

            float greenH = Math.Min(barH, greenZone);
            float yellowH = Math.Min(Math.Max(0, barH - greenH), yellowZone);
            float redH = Math.Max(0, barH - greenH - yellowH);

            // Vert (bas)
            using (var p = new SKPaint { Color = Ft2Theme.VuGreen })
                canvas.DrawRect(x, size.Height - greenH, effectiveW, greenH, p);

            // Jaune (milieu)
            if (yellowH > 0)
                using (var p = new SKPaint { Color = Ft2Theme.VuYellow })
                    canvas.DrawRect(x, size.Height - greenH - yellowH,
                        effectiveW, yellowH, p);

            // Rouge (haut)
            if (redH > 0)
                using (var p = new SKPaint { Color = Ft2Theme.VuRed })
                    canvas.DrawRect(x, size.Height - barH,
                        effectiveW, redH, p);

            // Peak cap
            using (var p = new SKPaint { Color = new SKColor(0xFFD0D8E8) })
                canvas.DrawRect(x, size.Height - barH - 2, effectiveW, 2, p);
        }
    }
}
