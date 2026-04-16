using SharpMod.Demo.Wpf.Themes;
using SkiaSharp;
using System;

namespace SharpMod.Demo.Wpf.Renderers;

public class ScopesVuRenderer
{
    private const int SCOPE_SIZE = 128;

    private static readonly SKColor[] VuOnColors =
        { Ft2Theme.VuGreen, Ft2Theme.VuYellow, Ft2Theme.VuRed };

    private readonly SKPaint _bgPaint = new() { Color = Ft2Theme.Background };
    private readonly SKPaint _borderPaint = new()
    {
        Color = Ft2Theme.ScopeBorder,
        StrokeWidth = 1,
        Style = SKPaintStyle.Stroke
    };
    private readonly SKPaint _centerPaint = new()
    {
        Color = new SKColor(0x30, 0x40, 0x60, 0x4D),
        StrokeWidth = 0.5f,
        Style = SKPaintStyle.Stroke
    };
    private readonly SKPaint _scopePaint = new()
    {
        StrokeWidth = 1,
        Style = SKPaintStyle.Stroke,
        IsAntialias = true
    };
    private readonly SKPaint _vuPaint = new() { Style = SKPaintStyle.Fill };
    private readonly SKPaint _vuOffPaint = new()
    {
        Color = Ft2Theme.VuOff,
        Style = SKPaintStyle.Fill
    };

    private float[]? _vuSmooth;
    private float[][]? _scopeSmooth;

    public void Draw(SKCanvas canvas, SKImageInfo info,
                     int channelCount, float[] vuLevels, float[][] scopeData)
    {
        int w = info.Width, h = info.Height;
        canvas.DrawRect(0, 0, w, h, _bgPaint);

        if (channelCount <= 0 || vuLevels == null || scopeData == null) return;

        if (_vuSmooth == null || _vuSmooth.Length != channelCount)
        {
            _vuSmooth = new float[channelCount];
            _scopeSmooth = new float[channelCount][];
            for (int i = 0; i < channelCount; i++)
                _scopeSmooth[i] = new float[SCOPE_SIZE];
        }

        float cellW = (float)w / channelCount;
        int vuW = 6;
        float halfH = h * 0.5f;
        float ampH = halfH * 0.8f;
        int segH = Math.Max(1, (h - 2) / 8);

        // Smooth VU (0.88 decay)
        for (int ch = 0; ch < channelCount; ch++)
        {
            float raw = ch < vuLevels.Length ? vuLevels[ch] : 0f;
            if (raw > _vuSmooth[ch]) _vuSmooth[ch] = raw;
            else
            {
                _vuSmooth[ch] *= 0.88f;
                if (_vuSmooth[ch] < 0.01f) _vuSmooth[ch] = 0;
            }
        }

        // Smooth scope
        for (int ch = 0; ch < channelCount; ch++)
        {
            var src = ch < scopeData.Length ? scopeData[ch] : null;
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

        // Borders
        for (int ch = 1; ch < channelCount; ch++)
            canvas.DrawLine(ch * cellW, 0, ch * cellW, h, _borderPaint);

        // Center lines + Waveforms
        for (int ch = 0; ch < channelCount; ch++)
        {
            float x0 = ch * cellW + 1;
            float scopeW = cellW - vuW - 3;
            if (scopeW < 4) scopeW = 4;

            canvas.DrawLine(x0, halfH, x0 + scopeW, halfH, _centerPaint);

            var data = _scopeSmooth![ch];
            _scopePaint.Color = Ft2Theme.ScopeColors[ch % Ft2Theme.ScopeColors.Length];
            float step = (float)SCOPE_SIZE / scopeW;

            using var path = new SKPath();
            for (int px = 0; px < (int)scopeW; px++)
            {
                int idx = Math.Min((int)(px * step), SCOPE_SIZE - 1);
                float y = halfH - data[idx] * ampH;
                if (px == 0) path.MoveTo(x0 + px, y);
                else path.LineTo(x0 + px, y);
            }
            canvas.DrawPath(path, _scopePaint);
        }

        // VU segments allumés
        int[][] vuRanges = { new[] { 0, 4 }, new[] { 4, 6 }, new[] { 6, 8 } };
        for (int pass = 0; pass < 3; pass++)
        {
            _vuPaint.Color = VuOnColors[pass];
            int sMin = vuRanges[pass][0], sMax = vuRanges[pass][1];
            for (int ch = 0; ch < channelCount; ch++)
            {
                int segs = Math.Min(8, (int)(_vuSmooth[ch] * 8 + 0.5f));
                float scopeW = cellW - vuW - 3;
                if (scopeW < 4) scopeW = 4;
                float vuX = ch * cellW + scopeW + 2;
                for (int s = sMin; s < sMax && s < segs; s++)
                    canvas.DrawRect(vuX, h - 1 - (s + 1) * segH, vuW, segH - 1, _vuPaint);
            }
        }

        // VU segments éteints
        for (int ch = 0; ch < channelCount; ch++)
        {
            int segs = Math.Min(8, (int)(_vuSmooth[ch] * 8 + 0.5f));
            float scopeW = cellW - vuW - 3;
            if (scopeW < 4) scopeW = 4;
            float vuX = ch * cellW + scopeW + 2;
            for (int s = segs; s < 8; s++)
                canvas.DrawRect(vuX, h - 1 - (s + 1) * segH, vuW, segH - 1, _vuOffPaint);
        }
    }
}
