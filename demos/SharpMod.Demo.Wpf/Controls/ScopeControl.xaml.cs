using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SharpMod.Demo.Wpf.Renderers;
using SharpMod.Demo.Wpf.ViewModels;
using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace SharpMod.Demo.Wpf.Controls;

public partial class ScopeControl : UserControl
{
    private readonly ScopeRenderer _scopeRenderer = new();
    private MainViewModel? _vm;

    public ScopeControl()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _vm = DataContext as MainViewModel;
        CompositionTarget.Rendering += OnCompositionRendering;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        CompositionTarget.Rendering -= OnCompositionRendering;
    }

    private void OnCompositionRendering(object? sender, EventArgs e)
    {
        _vm?.UpdateVisualizationData();
        SkCanvas.InvalidateVisual();
    }

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        if (_vm == null) return;

        var canvas = e.Surface.Canvas;
        var size = new SKSize(e.Info.Width, e.Info.Height);

        // ScopeRenderer.Draw : 4 args (canvas, size, float[][], int)
        _scopeRenderer.Draw(canvas, size, _vm.ScopeData, _vm.ChannelsCount);
    }
}
