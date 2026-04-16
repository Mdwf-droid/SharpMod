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

    public void Draw(SKCanvas canvas, int width, int height,
                     int channelCount, float[]? vuLevels, float[][]? scopeData,
                     float scrollX = 0)
    {
        canvas.DrawRect(0, 0, width, height, _bgPaint);

        if (channelCount <= 0 || vuLevels == null || scopeData == null) return;

        if (_vuSmooth == null || _vuSmooth.Length != channelCount)
        {
            _vuSmooth = new float[channelCount];
            _scopeSmooth = new float[channelCount][];
            for (int i = 0; i < channelCount; i++)
                _scopeSmooth[i] = new float[SCOPE_SIZE];
        }

        float cellW = Ft2Theme.CellWidth;
        float rowNumW = Ft2Theme.RowNumWidth;
        int vuW = 6;
        float halfH = height * 0.5f;
        float ampH = halfH * 0.8f;
        int segH = Math.Max(1, (height - 2) / 8);

        // ── Smooth VU ──
        for (int ch = 0; ch < channelCount; ch++)
        {
            float raw = ch < vuLevels.Length ? vuLevels[ch] : 0f;
            if (raw > _vuSmooth[ch]) _vuSmooth[ch] = raw;
            else
            {
                _vuSmooth[ch] *= 0.92f;
                if (_vuSmooth[ch] < 0.01f) _vuSmooth[ch] = 0;
            }
        }

        // ── ★ Smooth scope — FIX : utiliser le VU pour détecter le silence ── 
        for (int ch = 0; ch < channelCount; ch++)
        {
            var src = ch < scopeData.Length ? scopeData[ch] : null;
            var dst = _scopeSmooth![ch];
            float vu = ch < vuLevels.Length ? vuLevels[ch] : 0f;

            if (src != null && vu > 0.01f)
            {
                // Canal actif (VU > seuil) → copie directe
                Array.Copy(src, dst, Math.Min(src.Length, SCOPE_SIZE));
            }
            else
            {
                // Canal silencieux → decay rapide vers zéro
                bool allDead = true;
                for (int i = 0; i < SCOPE_SIZE; i++)
                {
                    dst[i] *= 0.7f;
                    if (Math.Abs(dst[i]) < 0.003f)
                        dst[i] = 0f;
                    else
                        allDead = false;
                }
                if (allDead)
                    Array.Clear(dst, 0, SCOPE_SIZE);
            }
        }

        // Clip au canvas
        canvas.Save();
        canvas.ClipRect(SKRect.Create(0, 0, width, height));

        // ── Draw channels ──
        for (int ch = 0; ch < channelCount; ch++)
        {
            float cellX = rowNumW + ch * cellW - scrollX;

            // Skip si hors écran
            if (cellX + cellW < 0 || cellX > width) continue;

            float scopeW = cellW - vuW - 3;
            if (scopeW < 4) scopeW = 4;
            float x0 = cellX + 1;

            // Bordure entre canaux
            if (ch > 0)
                canvas.DrawLine(cellX, 0, cellX, height, _borderPaint);

            // Ligne centrale
            canvas.DrawLine(x0, halfH, x0 + scopeW, halfH, _centerPaint);

            // Waveform
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

            // VU segments allumés
            int segs = Math.Min(8, (int)(_vuSmooth[ch] * 8 + 0.5f));
            float vuX = cellX + scopeW + 2;

            int[][] vuRanges = { new[] { 0, 4 }, new[] { 4, 6 }, new[] { 6, 8 } };
            for (int pass = 0; pass < 3; pass++)
            {
                _vuPaint.Color = VuOnColors[pass];
                int sMin = vuRanges[pass][0], sMax = vuRanges[pass][1];
                for (int s = sMin; s < sMax && s < segs; s++)
                    canvas.DrawRect(vuX, height - 1 - (s + 1) * segH,
                        vuW, segH - 1, _vuPaint);
            }

            // VU segments éteints
            for (int s = segs; s < 8; s++)
                canvas.DrawRect(vuX, height - 1 - (s + 1) * segH,
                    vuW, segH - 1, _vuOffPaint);
        }

        canvas.Restore();
    }
}
