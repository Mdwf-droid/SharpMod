using SharpMod.Song;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SharpMod.Demo.Wpf.Controls;

public partial class PatternEditorControl : UserControl
{
    private static readonly string[] NoteNames =
        { "C-", "C#", "D-", "D#", "E-", "F-", "F#", "G-", "G#", "A-", "A#", "B-" };

    // Couleurs
    private static readonly Brush BgBrush = new SolidColorBrush(Color.FromRgb(0x0B, 0x0E, 0x14));
    private static readonly Brush RowNumBrush = new SolidColorBrush(Color.FromRgb(0x50, 0x60, 0x80));
    private static readonly Brush NoteBrush = new SolidColorBrush(Color.FromRgb(0xD0, 0xD8, 0xE8));
    private static readonly Brush InstBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0xD0, 0x40));
    private static readonly Brush FxBrush = new SolidColorBrush(Color.FromRgb(0x40, 0xB0, 0x40));
    private static readonly Brush DotBrush = new SolidColorBrush(Color.FromRgb(0x30, 0x38, 0x48));
    private static readonly Brush ActiveBg = new SolidColorBrush(Color.FromArgb(0x40, 0xFF, 0xD0, 0x40));
    private static readonly Brush BarBg = new SolidColorBrush(Color.FromArgb(0x18, 0x40, 0x80, 0xD0));
    private static readonly Brush TransparentBg = Brushes.Transparent;
    private static readonly FontFamily MonoFont = new("Consolas");

    private readonly List<Border> _rowBorders = new();
    private int _activeRow = -1;
    private int _rowCount;
    private double _rowHeight = 16;

    static PatternEditorControl()
    {
        // Freeze brushes pour perf
        BgBrush.Freeze(); RowNumBrush.Freeze(); NoteBrush.Freeze();
        InstBrush.Freeze(); FxBrush.Freeze(); DotBrush.Freeze();
        ActiveBg.Freeze(); BarBg.Freeze();
    }

    public PatternEditorControl()
    {
        InitializeComponent();
    }

    public void LoadPattern(SongModule? module, int patternNumber)
    {
        if (module == null || patternNumber < 0 ||
            patternNumber >= module.Patterns.Count) return;

        var pattern = module.Patterns[patternNumber];
        int chCount = module.ChannelsCount;
        _rowCount = pattern.RowsCount;
        _activeRow = -1;
        _rowBorders.Clear();
        RowsPanel.Children.Clear();

        for (int r = 0; r < _rowCount; r++)
        {
            var sp = new StackPanel { Orientation = Orientation.Horizontal };

            // Row number
            sp.Children.Add(MakeText($"{r:X2} ", RowNumBrush, 28));

            // Cells
            for (int c = 0; c < chCount; c++)
            {
                var (note, inst, fx) = GetCellText(pattern, r, c);
                sp.Children.Add(MakeText(note + " ", note == "···" ? DotBrush : NoteBrush, 0));
                sp.Children.Add(MakeText(inst + " ", inst == "··" ? DotBrush : InstBrush, 0));
                sp.Children.Add(MakeText(fx, fx == "···" ? DotBrush : FxBrush, 0));

                if (c < chCount - 1)
                    sp.Children.Add(MakeText(" │ ", DotBrush, 0));
            }

            var bg = (r % 4 == 0) ? BarBg : TransparentBg;
            var border = new Border
            {
                Child = sp,
                Background = bg,
                Padding = new Thickness(2, 0, 2, 0),
                Height = _rowHeight
            };
            _rowBorders.Add(border);
            RowsPanel.Children.Add(border);
        }
    }

    public void UpdateActiveRow(int row)
    {
        if (row == _activeRow || row < 0 || row >= _rowBorders.Count) return;

        // Retirer le highlight précédent
        if (_activeRow >= 0 && _activeRow < _rowBorders.Count)
        {
            _rowBorders[_activeRow].Background =
                (_activeRow % 4 == 0) ? BarBg : TransparentBg;
        }

        // Appliquer le highlight
        _activeRow = row;
        _rowBorders[row].Background = ActiveBg;

        // Smooth scroll centré
        double viewH = PatternScroll.ViewportHeight;
        double target = row * _rowHeight - viewH / 2 + _rowHeight / 2;
        target = Math.Max(0, Math.Min(target,
            _rowCount * _rowHeight - viewH));

        double current = PatternScroll.VerticalOffset;
        double next = current + (target - current) * 0.3;
        PatternScroll.ScrollToVerticalOffset(next);
    }

    private TextBlock MakeText(string text, Brush fg, double minWidth)
    {
        var tb = new TextBlock
        {
            Text = text,
            Foreground = fg,
            FontFamily = MonoFont,
            FontSize = 11,
            VerticalAlignment = VerticalAlignment.Center,
        };
        if (minWidth > 0) tb.MinWidth = minWidth;
        return tb;
    }

    private static (string Note, string Inst, string Fx) GetCellText(
        Pattern pattern, int row, int channel)
    {
        if (channel >= pattern.Tracks.Count) return ("···", "··", "···");
        var track = pattern.Tracks[channel];
        if (track?.Cells == null || row >= track.Cells.Count) return ("···", "··", "···");
        var pc = track.Cells[row];
        if (pc == null) return ("···", "··", "···");

        string note = (pc.Note.HasValue && pc.Note.Value >= 0 && pc.Note.Value < 12)
            ? $"{NoteNames[pc.Note.Value]}{(pc.Octave.HasValue ? (pc.Octave.Value + 1).ToString() : "-")}"
            : "···";

        string inst = pc.Instrument != 0 ? pc.Instrument.ToString("X2") : "··";

        string fx = (pc.Effect != 0 || pc.EffectData != 0)
            ? $"{pc.Effect:X1}{pc.EffectData:X2}" : "···";

        return (note, inst, fx);
    }
}
