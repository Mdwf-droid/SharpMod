using SharpMod.Demo.Wpf.Themes;
using SharpMod.Song;
using SkiaSharp;
using System;

namespace SharpMod.Demo.Wpf.Renderers;

public class PatternRenderer
{
    private SKBitmap? _patternBitmap;
    private int _cachedPatternNumber = -1;
    private int _cachedChannelCount = -1;

    private const float HEADER_HEIGHT = 18f;

    // Paints pré-créés (réutilisés)
    private static readonly SKPaint BgPaint = new()
    { Color = Ft2Theme.Background, IsAntialias = false };
    private static readonly SKPaint HeaderBgPaint = new()
    { Color = SKColor.Parse("#0F1318"), IsAntialias = false };
    private static readonly SKPaint HeaderBorderPaint = new()
    { Color = Ft2Theme.SeparatorColor, StrokeWidth = 1, IsAntialias = false };
    private static readonly SKPaint HighlightPaint = new()
    { Color = new SKColor(0xFF, 0xD0, 0x40, 0x30), IsAntialias = false };
    private static readonly SKPaint BarBgPaint = new()
    { Color = new SKColor(0x40, 0x80, 0xD0, 0x18), IsAntialias = false };

    public void Draw(SKCanvas canvas, int width, int height,
                     SongModule? module, int patternNumber, int patternPosition,
                     float scrollX = 0)
    {
        canvas.Clear(Ft2Theme.Background);

        if (module == null || patternNumber < 0
            || patternNumber >= module.Patterns.Count)
            return;

        int channels = module.ChannelsCount;

        // Reconstruire le bitmap si le pattern ou le nombre de canaux change
        if (patternNumber != _cachedPatternNumber
            || channels != _cachedChannelCount)
        {
            _cachedPatternNumber = patternNumber;
            _cachedChannelCount = channels;
            RenderPatternBitmap(module, patternNumber);
        }

        if (_patternBitmap == null) return;

        // ═══ Dessiner les headers (fixés en haut, scrollent en X) ═══
        canvas.Save();
        canvas.ClipRect(SKRect.Create(0, 0, width, HEADER_HEIGHT));
        canvas.Translate(-scrollX, 0);
        DrawHeaders(canvas, channels, width + scrollX);
        canvas.Restore();

        // ═══ Dessiner le pattern (sous les headers, scrolle en X et Y) ═══
        float patternTop = HEADER_HEIGHT;
        float patternH = height - patternTop;

        float rowH = Ft2Theme.RowHeight;
        var pattern = module.Patterns[patternNumber];
        int totalRows = pattern.RowsCount;
        float visibleRows = patternH / rowH;
        float centerOffset = visibleRows / 2f;

        float scrollY = (patternPosition - centerOffset) * rowH;
        scrollY = Math.Max(0, Math.Min(scrollY,
            totalRows * rowH - patternH));

        canvas.Save();
        canvas.ClipRect(SKRect.Create(0, patternTop, width, patternH));
        canvas.Translate(-scrollX, patternTop - scrollY);
        canvas.DrawBitmap(_patternBitmap, 0, 0);
        canvas.Restore();

        // ═══ Highlight de la row active ═══
        float activeY = patternTop + (patternPosition * rowH - scrollY);
        if (activeY >= patternTop - rowH && activeY < height)
        {
            canvas.DrawRect(0, activeY, width, rowH, HighlightPaint);
        }
    }

    private void DrawHeaders(SKCanvas canvas, int channels, float totalVisibleW)
    {
        float rowNumW = Ft2Theme.RowNumWidth;
        float cellW = Ft2Theme.CellWidth;
        float totalW = rowNumW + channels * cellW;

        // Fond header
        canvas.DrawRect(0, 0, totalW, HEADER_HEIGHT, HeaderBgPaint);

        // Bordure basse
        canvas.DrawLine(0, HEADER_HEIGHT - 1, totalW, HEADER_HEIGHT - 1,
            HeaderBorderPaint);

        using var headerTextPaint = Ft2Theme.CreateTextPaint(
            new SKColor(0x80, 0x90, 0xB0), 10);
        headerTextPaint.TextAlign = SKTextAlign.Center;

        for (int c = 0; c < channels; c++)
        {
            float cx = rowNumW + c * cellW;

            // Séparateur vertical
            canvas.DrawLine(cx, 0, cx, HEADER_HEIGHT, HeaderBorderPaint);

            // Texte "CH 01"
            canvas.DrawText($"CH {c + 1:D2}",
                cx + cellW * 0.5f,
                HEADER_HEIGHT * 0.72f,
                headerTextPaint);
        }
    }

    // ═══════════════════════════════════
    // Pré-rendu du pattern dans un bitmap
    // ═══════════════════════════════════

    private void RenderPatternBitmap(SongModule module, int patternNumber)
    {
        var pattern = module.Patterns[patternNumber];
        int rowCount = pattern.RowsCount;
        int channels = module.ChannelsCount;
        float rowH = Ft2Theme.RowHeight;
        float rowNumW = Ft2Theme.RowNumWidth;
        float cellW = Ft2Theme.CellWidth;

        // Largeur TOTALE (pas clippée au canvas)
        int bitmapW = (int)(rowNumW + channels * cellW + 2);
        int bitmapH = (int)(rowCount * rowH + 2);

        _patternBitmap?.Dispose();
        _patternBitmap = new SKBitmap(bitmapW, bitmapH);

        using var canvas = new SKCanvas(_patternBitmap);
        canvas.Clear(Ft2Theme.Background);

        using var rowNumPaint = Ft2Theme.CreateTextPaint(Ft2Theme.RowNumberColor, 11);
        using var notePaint = Ft2Theme.CreateTextPaint(Ft2Theme.NoteColor, 11);
        using var instPaint = Ft2Theme.CreateTextPaint(Ft2Theme.InstrumentColor, 11);
        using var fxPaint = Ft2Theme.CreateTextPaint(Ft2Theme.EffectColor, 11);
        using var dotPaint = Ft2Theme.CreateTextPaint(Ft2Theme.DotColor, 11);
        using var sepPaint = new SKPaint
        {
            Color = Ft2Theme.SeparatorColor,
            StrokeWidth = 1,
            IsAntialias = false
        };

        for (int r = 0; r < rowCount; r++)
        {
            float y = r * rowH;
            float textY = y + rowH * 0.75f;

            // Bar highlight every 4 rows
            if (r % 4 == 0)
                canvas.DrawRect(0, y, bitmapW, rowH, BarBgPaint);

            // Row number
            canvas.DrawText($"{r:X2}", 4, textY, rowNumPaint);

            // Channels
            for (int c = 0; c < channels; c++)
            {
                float cx = rowNumW + c * cellW;

                if (c > 0)
                    canvas.DrawLine(cx, y, cx, y + rowH, sepPaint);

                var track = (c < pattern.Tracks.Count)
                    ? pattern.Tracks[c] : null;
                PatternCell? cell = (track?.Cells != null && r < track.Cells.Count)
                    ? track.Cells[r] : null;

                if (cell == null)
                {
                    canvas.DrawText("··· ·· ···", cx + 4, textY, dotPaint);
                    continue;
                }

                // Note
                string noteStr = FormatNote(cell);
                canvas.DrawText(noteStr, cx + 4, textY,
                    noteStr == "···" ? dotPaint : notePaint);

                // Instrument
                string instStr = cell.Instrument != 0
                    ? $"{cell.Instrument:X2}" : "··";
                canvas.DrawText(instStr, cx + Ft2Theme.CellNoteWidth + 4,
                    textY, instStr == "··" ? dotPaint : instPaint);

                // Effect
                string fxStr = (cell.Effect != 0 || cell.EffectData != 0)
                    ? $"{cell.Effect:X1}{cell.EffectData:X2}" : "···";
                canvas.DrawText(fxStr,
                    cx + Ft2Theme.CellNoteWidth + Ft2Theme.CellInstWidth + 4,
                    textY, fxStr == "···" ? dotPaint : fxPaint);
            }
        }
    }

    private static readonly string[] NoteNames =
        { "C-", "C#", "D-", "D#", "E-", "F-", "F#", "G-", "G#", "A-", "A#", "B-" };

    private static string FormatNote(PatternCell cell)
    {
        if (cell.Note.HasValue && cell.Note.Value >= 0 && cell.Note.Value < 12)
        {
            string name = NoteNames[cell.Note.Value];
            string oct = cell.Octave.HasValue
                ? (cell.Octave.Value + 1).ToString() : "-";
            return $"{name}{oct}";
        }
        return "···";
    }
}
