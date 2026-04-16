using Microsoft.Win32;
using SharpMod.Demo.Wpf.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SharpMod.Demo.Wpf.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel _vm;
    private int _lastPatternNumber = -1;
    private TimeSpan _lastRenderTime;

    public MainWindow()
    {
        InitializeComponent();
        _vm = new MainViewModel();
        DataContext = _vm;

        CompositionTarget.Rendering += OnRenderFrame;
        Closed += (_, _) =>
        {
            CompositionTarget.Rendering -= OnRenderFrame;
            _vm.Dispose();
        };
    }

    private void OnRenderFrame(object? sender, EventArgs e)
    {
        if (!_vm.IsPlaying) return;

        var args = (RenderingEventArgs)e;
        if (args.RenderingTime == _lastRenderTime) return;
        _lastRenderTime = args.RenderingTime;

        _vm.UpdateVisualizationData();

        // ── Spectrum ──
        SpectrumVis.Bands = _vm.SpectrumBands;
        SpectrumVis.BandCount = _vm.SpectrumBandCount;
        SpectrumVis.InvalidateVisual();

        // ── Scopes + VU ──
        ScopesVuVis.ChannelCount = _vm.ChannelCount;
        ScopesVuVis.VuLevels = _vm.VuLevels;
        ScopesVuVis.ScopeData = _vm.ScopeData;
        ScopesVuVis.InvalidateVisual();

        // ── Pattern ──
        if (_vm.PatternNumber != _lastPatternNumber && _vm.CurrentModule != null)
        {
            _lastPatternNumber = _vm.PatternNumber;
            PatternEditor.LoadPattern(_vm.CurrentModule, _vm.PatternNumber);
        }
        PatternEditor.UpdateActiveRow(_vm.PatternPosition);
    }

    private void BuildChannelHeaders(int count)
    {
        ChannelHeadersBar.Items.Clear();
        ChannelHeadersBar.Items.Add(new Border { Width = 28 });

        for (int c = 0; c < count; c++)
        {
            var tb = new TextBlock
            {
                Text = $"Ch {c + 1:D2}",
                Foreground = new SolidColorBrush(Color.FromRgb(0x80, 0x90, 0xB0)),
                FontFamily = new FontFamily("Consolas"),
                FontSize = 11,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(4, 0, 4, 0)
            };
            var border = new Border
            {
                Child = tb,
                BorderBrush = new SolidColorBrush(Color.FromRgb(0x2A, 0x35, 0x50)),
                BorderThickness = new Thickness(0, 0, 1, 0),
                MinWidth = 80
            };
            ChannelHeadersBar.Items.Add(border);
        }
    }

    private void OnPlay(object sender, RoutedEventArgs e) => _vm.Play();
    private void OnPause(object sender, RoutedEventArgs e) => _vm.Pause();
    private void OnStop(object sender, RoutedEventArgs e)
    {
        _vm.Stop();
        _lastPatternNumber = -1;
    }

    private void OnOpen(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Filter = "Modules|*.mod;*.s3m;*.xm|All files|*.*",
            Title = "Open Module"
        };
        if (dlg.ShowDialog() == true)
        {
            _vm.LoadModule(dlg.FileName);
            _lastPatternNumber = -1;
            if (_vm.CurrentModule != null)
            {
                PatternEditor.LoadPattern(_vm.CurrentModule, 0);
                BuildChannelHeaders(_vm.ChannelCount);
            }
        }
    }

    private void OnDragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop)
            ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    private void OnFileDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
        {
            _vm.LoadModule(files[0]);
            _lastPatternNumber = -1;
            if (_vm.CurrentModule != null)
            {
                PatternEditor.LoadPattern(_vm.CurrentModule, 0);
                BuildChannelHeaders(_vm.ChannelCount);
                _vm.Play();
            }
        }
    }
}
