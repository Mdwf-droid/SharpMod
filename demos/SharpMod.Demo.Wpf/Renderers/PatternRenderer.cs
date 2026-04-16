using SharpMod.Demo.Wpf.Themes;
using SharpMod.Song;
using SkiaSharp;
using System;

namespace SharpMod.Demo.Wpf.Renderers;

public class PatternRenderer
{
    private static readonly string[] NoteNames =
        { "C-", "C#", "D-", "D#", "E-", "F-", "F#", "G-", "G#", "A-", "A#", "B-" };

    // Pré-alloués une seule fois
    private readonly SKPaint _bgPaint;
    private readonly SKPaint _activeBgPaint;
    private readonly SKPaint _barBgPaint;
    private readonly SKPaint _rowNumPaint;
    private readonly SKPaint _notePaint;
    private readonly SKPaint _instPaint;
    private readonly SKPaint _fxPaint;
    private readonly SKPaint _dotPaint;
    private readonly SKTypeface? _typeface;

    public PatternRenderer()
    {
        _bgPaint = new SKPaint { Color = Ft2Theme.Background };
        _activeBgPaint = new SKPaint { Color = Ft2Theme.CurrentRowBg };
        _barBgPaint = new SKPaint { Color = new SKColor(0x40, 0x80, 0xD0, 0x18) };

        _typeface = Ft2Theme.LoadFont();
        _rowNumPaint = Ft2Theme.CreateTextPaint(Ft2Theme.RowNumberColor, 11, _typeface);
        _notePaint = Ft2Theme.CreateTextPaint(Ft2Theme.NoteColor, 11, _typeface);
        _instPaint = Ft2Theme.CreateTextPaint(Ft2Theme.InstrumentColor, 11, _typeface);
        _fxPaint = Ft2Theme.CreateTextPaint(Ft2Theme.EffectColor, 11, _typeface);
        _dotPaint = Ft2Theme.CreateTextPaint(Ft2Theme.DotColor, 11, _typeface);
    }

    public void Draw(SKCanvas canvas, SKImageInfo info,
                     Pattern? pattern, int channelCount,
                     int currentRow, float scrollOffset)
    {
        int w = info.Width, h = info.Height;
        canvas.Clear(Ft2Theme.Background);

        if (pattern == null || channelCount <= 0) return;

        int rowCount = pattern.RowsCount;
        float rowH = Ft2Theme.RowHeight;
        float rowNumW = Ft2Theme.RowNumWidth;
        float cellW = Ft2Theme.CellWidth;

        int visibleRows = (int)(h / rowH) + 2;
        int centerRow = (int)(h / rowH) / 2;

        for (int i = -centerRow - 1; i <= visibleRows - centerRow; i++)
        {
            int row = currentRow + i;
            if (row < 0 || row >= rowCount) continue;

            float y = (centerRow + i) * rowH - scrollOffset * rowH;
            if (y < -rowH || y > h + rowH) continue;

            // Highlights
            if (row == currentRow)
                canvas.DrawRect(0, y, w, rowH, _activeBgPaint);
            else if (row % 4 == 0)
                canvas.DrawRect(0, y, w, rowH, _barBgPaint);

            float textY = y + rowH - 3;

            // Row number
            canvas.DrawText($"{row:X2}", 4, textY, _rowNumPaint);

            // Cells
            for (int c = 0; c < channelCount; c++)
            {
                float x = rowNumW + c * cellW;
                DrawCell(canvas, pattern, row, c, x, textY);
            }
        }
    }

    private void DrawCell(SKCanvas canvas, Pattern pattern,
                          int row, int channel, float x, float textY)
    {
        if (channel >= pattern.Tracks.Count)
        {
            canvas.DrawText("··· ·· ···", x, textY, _dotPaint);
            return;
        }
        var track = pattern.Tracks[channel];
        if (track?.Cells == null || row >= track.Cells.Count)
        {
            canvas.DrawText("··· ·· ···", x, textY, _dotPaint);
            return;
        }
        var pc = track.Cells[row];
        if (pc == null)
        {
            canvas.DrawText("··· ·· ···", x, textY, _dotPaint);
            return;
        }

        float cx = x;

        // Note
        bool hasNote = pc.Note.HasValue && pc.Note.Value >= 0 && pc.Note.Value < 12;
        if (hasNote)
        {
            string oct = pc.Octave.HasValue ? (pc.Octave.Value + 1).ToString() : "-";
            canvas.DrawText($"{NoteNames[pc.Note.Value]}{oct}", cx, textY, _notePaint);
        }
        else
            canvas.DrawText("···", cx, textY, _dotPaint);

        cx += Ft2Theme.CellNoteWidth + Ft2Theme.CellSepWidth;

        // Instrument
        if (pc.Instrument != 0)
            canvas.DrawText(pc.Instrument.ToString("X2"), cx, textY, _instPaint);
        else
            canvas.DrawText("··", cx, textY, _dotPaint);

        cx += Ft2Theme.CellInstWidth + Ft2Theme.CellSepWidth;

        // Volume
        canvas.DrawText("··", cx, textY, _dotPaint);
        cx += Ft2Theme.CellVolWidth + Ft2Theme.CellSepWidth;

        // Effect
        if (pc.Effect != 0 || pc.EffectData != 0)
        {
            canvas.DrawText($"{pc.Effect:X1}{pc.EffectData:X2}", cx, textY, _fxPaint);
        }
        else
            canvas.DrawText("···", cx, textY, _dotPaint);
    }
}
