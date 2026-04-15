using System;
using System.IO;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using SharpMod.Demo.Wpf.Renderers;
using SharpMod.Song;
using SharpMod.SoundRenderer;

namespace SharpMod.Demo.Wpf.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private ModulePlayer? _player;
    private NAudioWaveChannelDriver? _renderer;

    // ── Observable properties ──
    [ObservableProperty] private string _songName = "No module loaded";
    [ObservableProperty] private string _modType = "";
    [ObservableProperty] private int _channelsCount;
    [ObservableProperty] private int _songPosition;
    [ObservableProperty] private int _patternNumber;
    [ObservableProperty] private int _patternPosition;
    [ObservableProperty] private int _speed;
    [ObservableProperty] private int _tempo;
    [ObservableProperty] private bool _isPlaying;
    [ObservableProperty] private SongModule? _currentModule;

    // ── Scope data par canal (float -1..1, 128 samples) ──
    public float[][] ScopeData { get; private set; } = [];

    // ── VU levels par canal (float 0..1) ──
    public float[] VuLevels { get; private set; } = [];

    // ── Buffers pré-alloués pour GetChannelLevels ──
    private int[] _vuVolumes = new int[32];
    private int[] _vuPeaks = new int[32];

    // ── Spectrum analyzer (FFT sur CurrentBytesWindow) ──
    public SpectrumAnalyzer Spectrum { get; } = new(fftSize: 512, bandCount: 32);

    // ── Commands ──

    [RelayCommand]
    private void OpenFile()
    {
        var dlg = new OpenFileDialog
        {
            Title = "Load Module",
            Filter = "Tracker Modules (*.mod;*.s3m;*.xm)|*.mod;*.s3m;*.xm|All files (*.*)|*.*",
            FilterIndex = 1,
        };
        if (dlg.ShowDialog() == true)
        {
            LoadModule(dlg.FileName);
            Play();
        }
    }

    [RelayCommand]
    private void Play()
    {
        if (_player == null || IsPlaying) return;
        _player.Start();
        IsPlaying = true;
    }

    [RelayCommand]
    private void Stop()
    {
        if (_player == null) return;
        _player.Stop();
        IsPlaying = false;
        SongPosition = 0;
        PatternPosition = 0;
    }

    [RelayCommand]
    private void Pause()
    {
        if (_player == null) return;
        _player.Pause();
        IsPlaying = !IsPlaying;
    }

    // ── Module loading ──

    public void LoadModule(string filePath)
    {
        if (_player != null && IsPlaying)
        {
            _player.Stop();
            IsPlaying = false;
        }

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var module = ModuleLoader.Instance.LoadModule(filePath);
        if (module == null) return;

        CurrentModule = module;
        SongName = module.SongName ?? Path.GetFileName(filePath);
        ModType = module.ModType ?? "Unknown";
        ChannelsCount = module.ChannelsCount;
        Speed = module.InitialSpeed;
        Tempo = module.InitialTempo;

        // Init scope/VU arrays
        ScopeData = new float[module.ChannelsCount][];
        for (int i = 0; i < module.ChannelsCount; i++)
            ScopeData[i] = new float[128]; // SCOPE_BUFFER_SIZE = 128
        VuLevels = new float[module.ChannelsCount];

        // Créer le player (InitScopeBuffers() est appelé dans son constructeur)
        _player = new ModulePlayer(module);

        // Créer + enregistrer le renderer
        _renderer = new NAudioWaveChannelDriver(NAudioWaveChannelDriver.Output.WaveOut);
        _player.RegisterRenderer(_renderer);

        // Player events
        _player.OnGetPlayerInfos += (s, e) =>
        {
            System.Windows.Application.Current?.Dispatcher.BeginInvoke(() =>
            {
                SongPosition = e.SongPosition;
                PatternNumber = e.PatternNumber;
                PatternPosition = e.PatternPosition;
            });
        };

        _player.OnCurrentModulePlayEnd += (s, e) =>
        {
            System.Windows.Application.Current?.Dispatcher.BeginInvoke(() =>
            {
                IsPlaying = false;
            });
        };
    }

    /// <summary>
    /// Appelé à 60fps par ScopeControl.
    /// Lit les oscillos et VU depuis le player via les VRAIES API.
    /// </summary>
    public void UpdateVisualizationData()
    {
        if (_player == null || !IsPlaying) return;

        // ── Scope par canal : player.GetScopeData(ch) → sbyte[] ──
        for (int ch = 0; ch < ChannelsCount && ch < ScopeData.Length; ch++)
        {
            var raw = _player.GetScopeData(ch); // sbyte[], 128 samples
            if (raw != null && raw.Length > 0)
            {
                var dst = ScopeData[ch];
                int len = Math.Min(raw.Length, dst.Length);
                for (int i = 0; i < len; i++)
                {
                    // sbyte (-128..127) → float (-1..1)
                    dst[i] = raw[i] / 128f;
                }
            }
        }

        // ── VU par canal : player.GetChannelLevels() ──
        _player.GetChannelLevels(out var volumes, out var peaks, out int count);
        for (int ch = 0; ch < count && ch < VuLevels.Length; ch++)
        {
            // peaks[] : int 0-128 → float 0-1
            VuLevels[ch] = peaks[ch] / 128f;
        }

        // ── Spectrum : depuis CurrentBytesWindow ──
        if (_player.CurrentBytesWindow != null && _player.CurrentBytesWindow.Length > 0)
        {
            Spectrum.AddStereoBytes(_player.CurrentBytesWindow,
                _player.CurrentBytesWindow.Length);
        }
    }
}
