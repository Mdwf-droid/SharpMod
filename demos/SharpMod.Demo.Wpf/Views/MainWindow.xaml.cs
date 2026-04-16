using SharpMod.Demo.Wpf.Renderers;
using SharpMod.Demo.Wpf.Themes;
using SharpMod.Demo.Wpf.ViewModels;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace SharpMod.Demo.Wpf.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel _vm;
    private readonly SpectrumRenderer _spectrumRenderer = new();
    private readonly ScopesVuRenderer _scopesRenderer = new();
    private readonly PatternRenderer _patternRenderer = new();
    private TimeSpan _lastRenderTime;

    // ═══ Scroll horizontal partagé ═══
    private float _scrollX = 0;
    private float _totalContentWidth = 0;
    private int _lastChannelCount = -1;

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

    // ═══════════════════════════════════
    // Custom Title Bar
    // ═══════════════════════════════════

    private void OnMinimizeClick(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void OnMaximizeRestoreClick(object sender, RoutedEventArgs e)
    {
        if (WindowState == WindowState.Maximized)
        {
            WindowState = WindowState.Normal;
            MaxRestoreBtn.Content = "□";
        }
        else
        {
            WindowState = WindowState.Maximized;
            MaxRestoreBtn.Content = "❐";
        }
    }

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    // ═══════════════════════════════════
    // Render loop
    // ═══════════════════════════════════

    private void OnRenderFrame(object? sender, EventArgs e)
    {
        var args = (RenderingEventArgs)e;
        if (args.RenderingTime == _lastRenderTime) return;
        _lastRenderTime = args.RenderingTime;

        if (!_vm.IsPlaying) return;

        _vm.UpdateVisualizationData();

        // Mettre à jour la scrollbar si le nombre de canaux change
        if (_vm.ChannelCount != _lastChannelCount)
        {
            _lastChannelCount = _vm.ChannelCount;
            UpdateScrollBar();
        }

        SpectrumCanvas.InvalidateVisual();
        ScopesCanvas.InvalidateVisual();
        PatternCanvas.InvalidateVisual();
    }

    // ═══════════════════════════════════
    // ScrollBar H
    // ═══════════════════════════════════

    private void UpdateScrollBar()
    {
        _totalContentWidth = Ft2Theme.RowNumWidth
            + _vm.ChannelCount * Ft2Theme.CellWidth;

        float visibleWidth = (float)PatternCanvas.ActualWidth;

        if (_totalContentWidth <= visibleWidth)
        {
            // Tout rentre → cacher la scrollbar
            HScrollBar.Visibility = Visibility.Collapsed;
            _scrollX = 0;
        }
        else
        {
            HScrollBar.Visibility = Visibility.Visible;
            HScrollBar.Maximum = _totalContentWidth - visibleWidth;
            HScrollBar.ViewportSize = visibleWidth;
            HScrollBar.LargeChange = visibleWidth * 0.8;
            HScrollBar.SmallChange = Ft2Theme.CellWidth;
        }
    }

    private void OnHScrollChanged(object sender,
        RoutedPropertyChangedEventArgs<double> e)
    {
        _scrollX = (float)e.NewValue;
        // Forcer un repaint immédiat
        ScopesCanvas.InvalidateVisual();
        PatternCanvas.InvalidateVisual();
    }

    private void OnPatternMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
        {
            // Shift + Wheel = scroll horizontal
            float delta = -e.Delta * 0.5f;
            float newVal = (float)HScrollBar.Value + delta;
            HScrollBar.Value = Math.Clamp(newVal,
                HScrollBar.Minimum, HScrollBar.Maximum);
            e.Handled = true;
        }
    }

    // ═══════════════════════════════════
    // PaintSurface handlers
    // ═══════════════════════════════════

    private void OnSpectrumPaint(object? sender, SKPaintSurfaceEventArgs e)
    {
        _spectrumRenderer.Draw(
            e.Surface.Canvas, e.Info.Width, e.Info.Height,
            _vm.SpectrumBands, _vm.SpectrumBandCount);
    }

    private void OnScopesPaint(object? sender, SKPaintSurfaceEventArgs e)
    {
        _scopesRenderer.Draw(
            e.Surface.Canvas, e.Info.Width, e.Info.Height,
            _vm.ChannelCount, _vm.VuLevels, _vm.ScopeData,
            _scrollX);  // ★ scroll X passé au renderer
    }

    private void OnPatternPaint(object? sender, SKPaintSurfaceEventArgs e)
    {
        _patternRenderer.Draw(
            e.Surface.Canvas, e.Info.Width, e.Info.Height,
            _vm.CurrentModule, _vm.PatternNumber, _vm.PatternPosition,
            _scrollX);  // ★ scroll X passé au renderer
    }

    // ═══════════════════════════════════
    // Drag & Drop
    // ═══════════════════════════════════

    private void OnDragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop)
            ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    private void OnDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetData(DataFormats.FileDrop) is string[] files
            && files.Length > 0)
        {
            _vm.LoadModule(files[0]);
            _scrollX = 0;
            HScrollBar.Value = 0;
            UpdateScrollBar();
            _vm.PlayCommand.Execute(null);
        }
    }
}
