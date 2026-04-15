using SharpMod.Demo.Wpf.Themes;
using SharpMod.Song;
using SkiaSharp;

namespace SharpMod.Demo.Wpf.Renderers;

public class PatternRenderer
{
    private readonly SKTypeface _typeface;
    private readonly SKPaint _bgPaint;
    private readonly SKPaint _currentRowPaint;
    private readonly SKPaint _rowNumPaint;
    private readonly SKPaint _notePaint;
    private readonly SKPaint _instPaint;
    private readonly SKPaint _volPaint;
    private readonly SKPaint _fxPaint;
    private readonly SKPaint _dotPaint;
    private readonly SKPaint _sepPaint;
    private readonly SKPaint _headerBgPaint;
    private readonly SKPaint _headerTextPaint;

    private static readonly string[] NoteNames =
        ["C-", "C#", "D-", "D#", "E-", "F-", "F#", "G-", "G#", "A-", "A#", "B-"];

    public PatternRenderer()
    {
        _typeface = Ft2Theme.LoadFont();
        _bgPaint = new SKPaint { Color = Ft2Theme.PanelInset };
        _currentRowPaint = new SKPaint { Color = Ft2Theme.CurrentRowBg };
        _rowNumPaint = Ft2Theme.CreateTextPaint(Ft2Theme.RowNumberColor, 11, _typeface);
        _notePaint = Ft2Theme.CreateTextPaint(Ft2Theme.NoteColor, 11, _typeface);
        _instPaint = Ft2Theme.CreateTextPaint(Ft2Theme.InstrumentColor, 11, _typeface);
        _volPaint = Ft2Theme.CreateTextPaint(Ft2Theme.VolumeColor, 11, _typeface);
        _fxPaint = Ft2Theme.CreateTextPaint(Ft2Theme.EffectColor, 11, _typeface);
        _dotPaint = Ft2Theme.CreateTextPaint(Ft2Theme.DotColor, 11, _typeface);
        _sepPaint = new SKPaint { Color = Ft2Theme.SeparatorColor, StrokeWidth = 1 };
        _headerBgPaint = new SKPaint { Color = Ft2Theme.PanelBg };
        _headerTextPaint = Ft2Theme.CreateTextPaint(Ft2Theme.TitleColor, 10, _typeface);
    }

    public void Draw(SKCanvas canvas, SKSize size, SongModule? module,
        int currentRow, int patternIndex)
    {
        canvas.Clear(Ft2Theme.PanelInset);

        if (module == null) return;
        if (patternIndex < 0 || patternIndex >= module.Patterns.Count) return;

        var pattern = module.Patterns[patternIndex];
        int channels = module.ChannelsCount;
        float rowH = Ft2Theme.RowHeight;
        float cellW = Ft2Theme.CellWidth;
        float headerH = 18f;

        // ── Channel headers ──
        canvas.DrawRect(0, 0, size.Width, headerH, _headerBgPaint);
        for (int ch = 0; ch < channels; ch++)
        {
            float hx = Ft2Theme.RowNumWidth + ch * cellW;
            canvas.DrawText($"CH {(ch + 1):D2}", hx + 4, headerH - 4, _headerTextPaint);
            canvas.DrawLine(hx, 0, hx, headerH, _sepPaint);
        }

        // ── Pattern rows ──
        float contentH = size.Height - headerH;
        int visibleRows = (int)(contentH / rowH) + 2;
        int halfVisible = visibleRows / 2;
        int startRow = currentRow - halfVisible;

        for (int vi = 0; vi < visibleRows; vi++)
        {
            int row = startRow + vi;
            float y = headerH + vi * rowH;

            if (y + rowH < headerH) continue;
            if (y > size.Height) break;

            if (row == currentRow)
                canvas.DrawRect(0, y, size.Width, rowH, _currentRowPaint);

            bool inRange = row >= 0 && row < pattern.RowsCount;

            if (inRange)
                canvas.DrawText(row.ToString("X2"), 4, y + rowH - 3, _rowNumPaint);

            float x = Ft2Theme.RowNumWidth;
            for (int ch = 0; ch < channels; ch++)
            {
                canvas.DrawLine(x, y, x, y + rowH, _sepPaint);

                if (inRange && ch < pattern.Tracks.Count)
                {
                    var track = pattern.Tracks[ch];
                    if (row < track.Cells.Count)
                        DrawCell(canvas, x, y + rowH - 3, track.Cells[row]);
                    else
                        DrawEmptyCell(canvas, x, y + rowH - 3);
                }
                else
                {
                    DrawEmptyCell(canvas, x, y + rowH - 3);
                }

                x += cellW;
            }
        }
    }

    private void DrawCell(SKCanvas canvas, float x, float textY, PatternCell cell)
    {
        float cx = x + 2;

        // ── Note (int?) ──
        int note = cell.Note ?? 0;
        int octave = cell.Octave ?? 0;
        if (note > 0 && note <= 12)
        {
            string noteStr = $"{NoteNames[note - 1]}{octave}";
            canvas.DrawText(noteStr, cx, textY, _notePaint);
        }
        else
        {
            canvas.DrawText("···", cx, textY, _dotPaint);
        }
        cx += Ft2Theme.CellNoteWidth;

        // ── Instrument (int?) ──
        int inst = cell.Instrument;
        if (inst > 0)
        {
            canvas.DrawText(inst.ToString("D2"), cx, textY, _instPaint);
        }
        else
        {
            canvas.DrawText("··", cx, textY, _dotPaint);
        }
        cx += Ft2Theme.CellInstWidth;

        // ── Volume ──
        canvas.DrawText("··", cx, textY, _dotPaint);
        cx += Ft2Theme.CellVolWidth;

        // ── Effect (int?) ──
        int fx = cell.Effect;
        int fxData = cell.EffectData;
        if (fx > 0 || fxData > 0)
        {
            canvas.DrawText(fx.ToString("X1"), cx, textY, _fxPaint);
            cx += Ft2Theme.CellFxWidth;
            canvas.DrawText(fxData.ToString("X2"), cx, textY, _fxPaint);
        }
        else
        {
            canvas.DrawText("·", cx, textY, _dotPaint);
            cx += Ft2Theme.CellFxWidth;
            canvas.DrawText("··", cx, textY, _dotPaint);
        }
    }

    private void DrawEmptyCell(SKCanvas canvas, float x, float textY)
    {
        float cx = x + 2;
        canvas.DrawText("···", cx, textY, _dotPaint);
        cx += Ft2Theme.CellNoteWidth;
        canvas.DrawText("··", cx, textY, _dotPaint);
        cx += Ft2Theme.CellInstWidth;
        canvas.DrawText("··", cx, textY, _dotPaint);
        cx += Ft2Theme.CellVolWidth;
        canvas.DrawText("·", cx, textY, _dotPaint);
        cx += Ft2Theme.CellFxWidth;
        canvas.DrawText("··", cx, textY, _dotPaint);
    }
}
