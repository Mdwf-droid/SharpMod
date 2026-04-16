using System;
using System.Windows;
using System.Windows.Media;

namespace SharpMod.Demo.Wpf.Controls;

public class SpectrumVisual : FrameworkElement
{
    private static readonly Brush BgBrush;
    private static readonly Pen GridPen;

    // ★ Cache de 256 brushes pré-calculés (gradient vert→jaune→rouge)
    private static readonly Brush[] GradientCache;

    static SpectrumVisual()
    {
        BgBrush = new SolidColorBrush(Color.FromRgb(0x0B, 0x0E, 0x14));
        BgBrush.Freeze();
        GridPen = new Pen(new SolidColorBrush(Color.FromArgb(0x20, 0x40, 0x60, 0x90)), 0.5);
        GridPen.Freeze();

        // Pré-calculer 256 niveaux de couleur
        GradientCache = new Brush[256];
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
            var b = new SolidColorBrush(Color.FromRgb(r, g, 40));
            b.Freeze();
            GradientCache[i] = b;
        }
    }

    public float[]? Bands { get; set; }
    public int BandCount { get; set; }

    protected override void OnRender(DrawingContext dc)
    {
        double w = ActualWidth, h = ActualHeight;
        if (w < 1 || h < 1) return;

        dc.DrawRectangle(BgBrush, null, new Rect(0, 0, w, h));

        // Grille
        for (int g = 1; g <= 4; g++)
            dc.DrawLine(GridPen, new Point(0, h * g / 5.0), new Point(w, h * g / 5.0));

        if (Bands == null || Bands.Length == 0 || BandCount == 0) return;

        int count = Math.Min(Bands.Length, BandCount);
        double barWidth = w / count;
        double gap = Math.Max(1, barWidth * 0.15);

        for (int i = 0; i < count; i++)
        {
            float val = Math.Clamp(Bands[i], 0f, 1f);
            double barH = val * h;
            if (barH < 1) continue;

            double x = i * barWidth + gap * 0.5;

            // ★ ZERO ALLOCATION : lookup dans le cache
            int colorIdx = (int)(val * 255);
            dc.DrawRectangle(GradientCache[colorIdx], null,
                new Rect(x, h - barH, barWidth - gap, barH));
        }
    }
}
