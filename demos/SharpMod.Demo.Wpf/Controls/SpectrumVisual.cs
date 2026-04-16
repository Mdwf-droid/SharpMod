using System;
using System.Windows;
using System.Windows.Media;

namespace SharpMod.Demo.Wpf.Controls;

/// <summary>
/// Spectrum FFT — rendu GPU via DrawingContext (OnRender).
/// Identique visuellement au Blazor (vert → jaune → rouge).
/// </summary>
public class SpectrumVisual : FrameworkElement
{
    private static readonly Brush BgBrush;
    private static readonly Pen GridPen;

    // Cache de brushes par bande (évite new à chaque frame)
    private Brush[] _barBrushes = Array.Empty<Brush>();

    static SpectrumVisual()
    {
        BgBrush = new SolidColorBrush(Color.FromRgb(0x0B, 0x0E, 0x14));
        BgBrush.Freeze();
        GridPen = new Pen(new SolidColorBrush(Color.FromArgb(0x20, 0x40, 0x60, 0x90)), 0.5);
        GridPen.Freeze();
    }

    public float[]? Bands { get; set; }
    public int BandCount { get; set; }

    protected override void OnRender(DrawingContext dc)
    {
        double w = ActualWidth, h = ActualHeight;
        if (w < 1 || h < 1) return;

        // Background
        dc.DrawRectangle(BgBrush, null, new Rect(0, 0, w, h));

        // Grille
        for (int g = 1; g <= 4; g++)
        {
            double gy = h * g / 5.0;
            dc.DrawLine(GridPen, new Point(0, gy), new Point(w, gy));
        }

        if (Bands == null || Bands.Length == 0 || BandCount == 0) return;

        int count = Math.Min(Bands.Length, BandCount);
        double barWidth = w / count;
        double gap = Math.Max(1, barWidth * 0.15);

        // Init brush cache
        if (_barBrushes.Length != count)
            _barBrushes = new Brush[count];

        for (int i = 0; i < count; i++)
        {
            float val = Math.Clamp(Bands[i], 0f, 1f);
            double barH = val * h;
            if (barH < 1) continue;

            double x = i * barWidth + gap * 0.5;

            // Couleur identique Blazor : vert → jaune → rouge
            byte r, g2;
            if (val < 0.5f)
            {
                r = (byte)Math.Min(255, val * 384);
                g2 = 192;
            }
            else
            {
                r = 192;
                g2 = (byte)Math.Max(0, (1f - (val - 0.5f) * 2f) * 192);
            }

            // Réutiliser ou créer le brush
            var brush = new SolidColorBrush(Color.FromRgb(r, g2, 40));
            brush.Freeze();
            _barBrushes[i] = brush;

            dc.DrawRectangle(brush, null,
                new Rect(x, h - barH, barWidth - gap, barH));
        }
    }
}
