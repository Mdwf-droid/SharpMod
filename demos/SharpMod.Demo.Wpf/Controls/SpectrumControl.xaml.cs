using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SharpMod.Demo.Wpf.Renderers;
using SharpMod.Demo.Wpf.ViewModels;
using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace SharpMod.Demo.Wpf.Controls;

public partial class SpectrumControl : UserControl
{
    private readonly SpectrumRenderer _renderer = new();
    private MainViewModel? _vm;

    public SpectrumControl()
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
        SkCanvas.InvalidateVisual();
    }

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        if (_vm?.Spectrum == null) return;

        var canvas = e.Surface.Canvas;
        var size = new SKSize(e.Info.Width, e.Info.Height);

        _renderer.Draw(canvas, size, _vm.Spectrum.Bands, _vm.Spectrum.BandCount);
    }
}