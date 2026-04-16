using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
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
    private const int SPECTRUM_BANDS = 64;

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

    // ═══════════════════════════════════
    // Commands MVVM
    // ═══════════════════════════════════

    public IRelayCommand PlayCommand { get; }
    public IRelayCommand StopCommand { get; }
    public IRelayCommand PauseCommand { get; }
    public IRelayCommand OpenFileCommand { get; }

    public MainViewModel()
    {
        PlayCommand = new RelayCommand(Play, () => _module != null && !IsPlaying);
        StopCommand = new RelayCommand(Stop, () => _module != null && IsPlaying);
        PauseCommand = new RelayCommand(Pause, () => _module != null && IsPlaying);
        OpenFileCommand = new RelayCommand(OpenFile);
    }

    partial void OnIsPlayingChanged(bool value)
    {
        PlayCommand.NotifyCanExecuteChanged();
        StopCommand.NotifyCanExecuteChanged();
        PauseCommand.NotifyCanExecuteChanged();
    }

    private void OpenFile()
    {
        var dlg = new OpenFileDialog
        {
            Filter = "Tracker Modules|*.mod;*.s3m;*.xm|All files|*.*",
            Title = "Open Module"
        };
        if (dlg.ShowDialog() == true)
        {
            LoadModule(dlg.FileName);
            Play();
        }
    }

    // ═══════════════════════════════════
    // Module loading
    // ═══════════════════════════════════

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

            _renderer = new NAudioWaveChannelDriver(
                NAudioWaveChannelDriver.Output.WaveOut);
            _player.RegisterRenderer(_renderer);

            System.Diagnostics.Debug.WriteLine(
                $"[SharpMod] Rate={_player.MixCfg.Rate} " +
                $"WaveFormat={_renderer.TrackerStream?.WaveFormat?.SampleRate} " +
                $"Is16Bits={_player.MixCfg.Is16Bits} " +
                $"Style={_player.MixCfg.Style}");

            _spectrumAnalyzer = new SpectrumAnalyzer(SPECTRUM_BANDS);

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

            // Rafraîchir le CanExecute des commands après chargement
            PlayCommand.NotifyCanExecuteChanged();
            StopCommand.NotifyCanExecuteChanged();
            PauseCommand.NotifyCanExecuteChanged();

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

    // ═══════════════════════════════════
    // Playback
    // ═══════════════════════════════════

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

    // ═══════════════════════════════════
    // Visualization (appelé ~60fps)
    // ═══════════════════════════════════

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

    // ═══════════════════════════════════
    // Events player
    // ═══════════════════════════════════

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
