using CommunityToolkit.Mvvm.ComponentModel;
using SharpMod.Demo.Wpf.Renderers;
using SharpMod.SoundRenderer;
using SharpMod.Song;
using System;
using System.IO;

namespace SharpMod.Demo.Wpf.ViewModels;

public partial class MainViewModel : ObservableObject, IDisposable
{
    private ModulePlayer? _player;
    private SongModule? _module;
    private NAudioWaveChannelDriver? _renderer;
    private SpectrumAnalyzer? _spectrumAnalyzer;

    private const int SCOPE_SIZE = 128;
    private const int SPECTRUM_BANDS = 32;

    [ObservableProperty] private string _moduleTitle = "SharpMod ── Drop a .MOD .S3M .XM file";
    [ObservableProperty] private string _statusMessage = "Ready";
    [ObservableProperty] private int _speed = 6;
    [ObservableProperty] private int _bpm = 125;
    [ObservableProperty] private int _channelCount = 4;
    [ObservableProperty] private bool _isPlaying;
    [ObservableProperty] private int _songPosition;
    [ObservableProperty] private int _patternNumber;
    [ObservableProperty] private int _patternPosition;

    public string SongPositionDisplay => $"{SongPosition:D3}";
    public string PatternNumberDisplay => $"{PatternNumber:D3}";
    public string PatternPositionDisplay => $"{PatternPosition:X2}";

    public float[] VuLevels { get; private set; } = new float[32];
    public float[][] ScopeData { get; private set; } = Array.Empty<float[]>();
    public float[] SpectrumBands => _spectrumAnalyzer?.Bands ?? Array.Empty<float>();
    public int SpectrumBandCount => SPECTRUM_BANDS;

    public SongModule? CurrentModule => _module;

    public void LoadModule(string filePath)
    {
        Stop();

        try
        {
            _module = ModuleLoader.Instance.LoadModule(filePath);
            if (_module == null)
            {
                StatusMessage = $"Error: Cannot load {Path.GetFileName(filePath)}";
                return;
            }

            _player = new ModulePlayer(_module);

            // ★ FIX BPM : forcer le rate à 44100 (identique au Blazor)
            // Le MixCfg.Rate par défaut est 48000 dans le constructeur de ModulePlayer
            // mais les timings BPM sont calibrés pour 44100
            _player.MixCfg.Rate = 44100;

            _renderer = new NAudioWaveChannelDriver(
                NAudioWaveChannelDriver.Output.WaveOut);
            _player.RegisterRenderer(_renderer);

            _spectrumAnalyzer = new SpectrumAnalyzer(SPECTRUM_BANDS);

            // Brancher le spectrum sur les samples audio
            if (_renderer.TrackerStream != null)
                _renderer.TrackerStream.OnSamplesGenerated += OnAudioSamplesGenerated;

            ModuleTitle = $"{_module.SongName} ── {_module.ModType}";
            ChannelCount = _module.ChannelsCount;
            Speed = _module.InitialSpeed;
            Bpm = _module.InitialTempo;
            StatusMessage = $"Loaded: {Path.GetFileName(filePath)}";

            VuLevels = new float[ChannelCount];
            ScopeData = new float[ChannelCount][];
            for (int i = 0; i < ChannelCount; i++)
                ScopeData[i] = new float[SCOPE_SIZE];

            _player.OnGetPlayerInfos += OnPlayerInfos;
            _player.OnCurrentModulePlayEnd += OnPlayEnd;
            NotifyPositionDisplays();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    private void OnAudioSamplesGenerated(byte[] buffer, int bytesRead)
    {
        _spectrumAnalyzer?.AddStereoBytes(buffer, bytesRead);
    }

    public void Play()
    {
        if (_player == null) return;
        _player.Start();
        IsPlaying = true;
        StatusMessage = "Playing...";
    }

    public void Pause()
    {
        if (_player == null) return;
        _player.Pause();
        IsPlaying = !IsPlaying;
        StatusMessage = IsPlaying ? "Playing..." : "Paused";
    }

    public void Stop()
    {
        if (_renderer?.TrackerStream != null)
            _renderer.TrackerStream.OnSamplesGenerated -= OnAudioSamplesGenerated;

        if (_player != null)
        {
            _player.OnGetPlayerInfos -= OnPlayerInfos;
            _player.OnCurrentModulePlayEnd -= OnPlayEnd;
            _player.Stop();
        }

        IsPlaying = false;
        SongPosition = 0;
        PatternNumber = 0;
        PatternPosition = 0;
        StatusMessage = _module != null ? $"Stopped ── {_module.SongName}" : "Stopped";
        NotifyPositionDisplays();
    }

    /// <summary>
    /// Appelé ~60fps par CompositionTarget.Rendering.
    /// Ne fait que copier les données volatiles (léger).
    /// </summary>
    public void UpdateVisualizationData()
    {
        if (_player == null || !IsPlaying) return;

        _player.GetChannelLevels(out int[] _, out int[] peaks, out int count);
        for (int ch = 0; ch < ChannelCount && ch < count; ch++)
        {
            float peak = peaks[ch] / 128f;
            VuLevels[ch] = Math.Min(1f, (float)Math.Sqrt(peak));
        }

        for (int ch = 0; ch < ChannelCount; ch++)
        {
            sbyte[]? raw = _player.GetScopeData(ch);
            if (raw != null && ch < ScopeData.Length)
            {
                int len = Math.Min(SCOPE_SIZE, raw.Length);
                for (int i = 0; i < len; i++)
                    ScopeData[ch][i] = raw[i] / 128f;
            }
        }
    }

    private void OnPlayerInfos(object sender, SharpModEventArgs e)
    {
        SongPosition = e.SongPosition;
        PatternNumber = e.PatternNumber;
        PatternPosition = e.PatternPosition;
        App.Current?.Dispatcher?.BeginInvoke(NotifyPositionDisplays);
    }

    private void OnPlayEnd(object sender, EventArgs e)
    {
        App.Current?.Dispatcher?.BeginInvoke(() => Stop());
    }

    private void NotifyPositionDisplays()
    {
        OnPropertyChanged(nameof(SongPositionDisplay));
        OnPropertyChanged(nameof(PatternNumberDisplay));
        OnPropertyChanged(nameof(PatternPositionDisplay));
    }

    public void Dispose() => Stop();
}
