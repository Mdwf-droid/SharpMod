using System;
using System.Windows;
using System.Windows.Media;

namespace SharpMod.Demo.Wpf.Controls;

public class ScopesVuVisual : FrameworkElement
{
    private const int SCOPE_SIZE = 128;

    private static readonly Color[] ScopeColors =
    {
        Color.FromRgb(0x40, 0xB0, 0x40), Color.FromRgb(0x40, 0xA0, 0xD0),
        Color.FromRgb(0xD0, 0xA0, 0x40), Color.FromRgb(0xD0, 0x40, 0x80),
        Color.FromRgb(0x80, 0x40, 0xD0), Color.FromRgb(0x40, 0xD0, 0xA0),
        Color.FromRgb(0xD0, 0x60, 0x40), Color.FromRgb(0x40, 0x80, 0xD0)
    };

    private static readonly Brush BgBrush;
    private static readonly Pen BorderPen;
    private static readonly Pen CenterPen;
    private static readonly Brush VuGreen, VuYellow, VuRed, VuOff;
    private static readonly Pen[] ScopePens;

    static ScopesVuVisual()
    {
        BgBrush = Freeze(new SolidColorBrush(Color.FromRgb(0x0B, 0x0E, 0x14)));
        BorderPen = FreezePen(new Pen(new SolidColorBrush(Color.FromRgb(0x1A, 0x1F, 0x2E)), 1));
        CenterPen = FreezePen(new Pen(new SolidColorBrush(Color.FromArgb(0x4D, 0x30, 0x40, 0x60)), 0.5));
        VuGreen = Freeze(new SolidColorBrush(Color.FromRgb(0x40, 0xC0, 0x40)));
        VuYellow = Freeze(new SolidColorBrush(Color.FromRgb(0xC0, 0xC0, 0x40)));
        VuRed = Freeze(new SolidColorBrush(Color.FromRgb(0xC0, 0x40, 0x40)));
        VuOff = Freeze(new SolidColorBrush(Color.FromRgb(0x0E, 0x12, 0x18)));

        ScopePens = new Pen[ScopeColors.Length];
        for (int i = 0; i < ScopeColors.Length; i++)
            ScopePens[i] = FreezePen(new Pen(new SolidColorBrush(ScopeColors[i]), 1));
    }

    private static SolidColorBrush Freeze(SolidColorBrush b) { b.Freeze(); return b; }
    private static Pen FreezePen(Pen p) { p.Freeze(); return p; }

    private static readonly Brush[] VuOnBrushes = { VuGreen, VuYellow, VuRed };

    private float[]? _vuSmooth;
    private float[][]? _scopeSmooth;

    // ★ Géométries pré-allouées, réutilisées chaque frame
    private StreamGeometry[]? _scopeGeometries;

    public int ChannelCount { get; set; }
    public float[]? VuLevels { get; set; }
    public float[][]? ScopeData { get; set; }

    protected override void OnRender(DrawingContext dc)
    {
        double w = ActualWidth, h = ActualHeight;
        if (w < 1 || h < 1) return;

        dc.DrawRectangle(BgBrush, null, new Rect(0, 0, w, h));

        int count = ChannelCount;
        if (count <= 0 || VuLevels == null || ScopeData == null) return;

        // Init smooth buffers
        if (_vuSmooth == null || _vuSmooth.Length != count)
        {
            _vuSmooth = new float[count];
            _scopeSmooth = new float[count][];
            _scopeGeometries = new StreamGeometry[count];
            for (int i = 0; i < count; i++)
            {
                _scopeSmooth[i] = new float[SCOPE_SIZE];
                _scopeGeometries[i] = new StreamGeometry();
            }
        }

        double cellW = w / count;
        int vuW = 6;
        double halfH = h * 0.5;
        double ampH = halfH * 0.8;
        int segH = Math.Max(1, (int)((h - 2) / 8));

        // ── Smooth VU ──
        for (int ch = 0; ch < count; ch++)
        {
            float raw = ch < VuLevels.Length ? VuLevels[ch] : 0f;
            if (raw > _vuSmooth[ch]) _vuSmooth[ch] = raw;
            else
            {
                _vuSmooth[ch] *= 0.96f;  // ★ était 0.88f — le Blazor utilise 0.96
                if (_vuSmooth[ch] < 0.01f) _vuSmooth[ch] = 0;
            }
        }

        // ── Smooth scope ──
        for (int ch = 0; ch < count; ch++)
        {
            var src = ch < ScopeData.Length ? ScopeData[ch] : null;
            var dst = _scopeSmooth![ch];
            if (src == null) continue;

            float maxAbs = 0;
            for (int i = 0; i < SCOPE_SIZE && i < src.Length; i++)
            {
                float a = Math.Abs(src[i]);
                if (a > maxAbs) maxAbs = a;
            }
            if (maxAbs > 0.001f)
                Array.Copy(src, dst, Math.Min(src.Length, SCOPE_SIZE));
            else
                for (int i = 0; i < SCOPE_SIZE; i++) dst[i] *= 0.85f;
        }

        // ── Borders ──
        for (int ch = 1; ch < count; ch++)
            dc.DrawLine(BorderPen, new Point(ch * cellW, 0), new Point(ch * cellW, h));

        // ── Center lines + Waveforms ──
        for (int ch = 0; ch < count; ch++)
        {
            double x0 = ch * cellW + 1;
            double scopeW = cellW - vuW - 3;
            if (scopeW < 4) scopeW = 4;

            dc.DrawLine(CenterPen, new Point(x0, halfH), new Point(x0 + scopeW, halfH));

            var data = _scopeSmooth![ch];
            double step = (double)SCOPE_SIZE / scopeW;

            // ★ RÉUTILISER la géométrie au lieu d'en créer une nouvelle
            var geom = _scopeGeometries![ch];
            // StreamGeometry ne peut pas être réutilisée une fois Frozen
            // → on doit quand même en créer une nouvelle MAIS sans Freeze
            // Alternative : utiliser PathGeometry avec PathFigure recyclée
            // → Plus simple : PathGeometry + segments réutilisés

            // Approche la plus rapide : dessiner avec DrawLine directement
            var pen = ScopePens[ch % ScopePens.Length];
            double prevY = halfH - data[0] * ampH;
            int pixelW = (int)scopeW;
            for (int px = 1; px < pixelW; px++)
            {
                int idx = Math.Min((int)(px * step), SCOPE_SIZE - 1);
                double y = halfH - data[idx] * ampH;
                dc.DrawLine(pen, new Point(x0 + px - 1, prevY), new Point(x0 + px, y));
                prevY = y;
            }
        }

        // ── VU on ──
        int[][] vuRanges = { new[] { 0, 4 }, new[] { 4, 6 }, new[] { 6, 8 } };
        for (int pass = 0; pass < 3; pass++)
        {
            var brush = VuOnBrushes[pass];
            int sMin = vuRanges[pass][0], sMax = vuRanges[pass][1];
            for (int ch = 0; ch < count; ch++)
            {
                int segs = Math.Min(8, (int)(_vuSmooth[ch] * 8 + 0.5f));
                double scopeW = cellW - vuW - 3;
                if (scopeW < 4) scopeW = 4;
                double vuX = ch * cellW + scopeW + 2;
                for (int s = sMin; s < sMax && s < segs; s++)
                    dc.DrawRectangle(brush, null,
                        new Rect(vuX, h - 1 - (s + 1) * segH, vuW, segH - 1));
            }
        }

        // ── VU off ──
        for (int ch = 0; ch < count; ch++)
        {
            int segs = Math.Min(8, (int)(_vuSmooth[ch] * 8 + 0.5f));
            double scopeW = cellW - vuW - 3;
            if (scopeW < 4) scopeW = 4;
            double vuX = ch * cellW + scopeW + 2;
            for (int s = segs; s < 8; s++)
                dc.DrawRectangle(VuOff, null,
                    new Rect(vuX, h - 1 - (s + 1) * segH, vuW, segH - 1));
        }
    }
}
