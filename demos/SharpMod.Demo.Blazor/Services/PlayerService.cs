using Microsoft.JSInterop;
using SharpMod.Song;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SharpMod.Demo.Blazor.Services;

public class PlayerService : IDisposable
{
    private IJSRuntime? _js;
    private ModulePlayer? _player;
    private SongModule? _module;
    private WebAudioRenderer? _renderer;
    private Timer? _positionTimer;

    public string? ModuleName { get; private set; }
    public string? ModuleType { get; private set; }
    public int ChannelCount { get; private set; }
    public int Speed { get; private set; }
    public int BPM { get; private set; } = 125;
    public bool IsPlaying { get; private set; }
    public string StatusMessage { get; set; } = "Ready ── Drop a .MOD .S3M .XM file";

    public int SongPosition { get; private set; }
    public int PatternNumber { get; private set; }
    public int PatternPosition { get; private set; }

    public SongModule? CurrentModule => _module;
    public ModulePlayer? CurrentPlayer => _player;

    public event Action? OnStateChanged;
    public event Action<int, int>? OnPatternChanged;

    public async Task InitializeAsync(IJSRuntime js)
    {
        _js = js;
    }

    public async Task LoadModuleAsync(byte[] fileData, string fileName)
    {
        try
        {
            StatusMessage = $"Loading {fileName}...";
            NotifyStateChanged();

            using var ms = new MemoryStream(fileData);
            _module = ModuleLoader.Instance.LoadModule(ms);

            if (_module == null)
            {
                StatusMessage = $"Error: Cannot load {fileName}";
                NotifyStateChanged();
                return;
            }

            _player = new ModulePlayer(_module);
            _renderer = new WebAudioRenderer(_js!);
            _player.RegisterRenderer(_renderer);

            ModuleName = _module.SongName;
            ModuleType = _module.ModType;
            ChannelCount = _module.ChannelsCount;
            Speed = _module.InitialSpeed;
            BPM = _module.InitialTempo;

            SongPosition = 0;
            PatternNumber = 0;
            PatternPosition = 0;

            StatusMessage = $"Loaded: {fileName} ── {_module.SongName}";
            NotifyStateChanged();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            NotifyStateChanged();
        }
    }

    public async Task PlayAsync()
    {
        if (_player == null) return;

        try { await _js!.InvokeVoidAsync("SharpModAudio.initialize"); }
        catch { }

        _player.Start();
        IsPlaying = true;
        StatusMessage = "Playing...";

        // Timer ~12fps pour poll les positions retardées
        _positionTimer = new Timer(
            PollPositions, null, 0, 80);

        NotifyStateChanged();
    }

    public async Task StopAsync()
    {
        _positionTimer?.Dispose();
        _positionTimer = null;

        if (_player == null) return;

        _player.Stop();
        IsPlaying = false;
        SongPosition = 0;
        PatternNumber = 0;
        PatternPosition = 0;
        StatusMessage = "Stopped";

        try { await _js!.InvokeVoidAsync("SharpModAudio.stop"); }
        catch { }

        NotifyStateChanged();
    }

    public async Task PauseAsync()
    {
        if (_player == null) return;

        _player.Pause();
        IsPlaying = !IsPlaying;

        try { await _js!.InvokeVoidAsync("SharpModAudio.pause"); }
        catch { }

        StatusMessage = IsPlaying ? "Playing..." : "Paused";
        NotifyStateChanged();
    }

    // ★ Poll les positions RETARDÉES depuis le ring buffer JS
    private async void PollPositions(object? state)
    {
        if (_renderer == null || !IsPlaying || _js == null) return;

        try
        {
            // ★ Lire les positions compensées depuis le JS
            var positions = await _js.InvokeAsync<int[]>(
                "SharpModAudio.getDisplayPositions");

            if (positions == null || positions.Length < 3) return;

            int songPos = positions[0];
            int patNum = positions[1];
            int patPos = positions[2];

            bool changed = songPos != SongPosition
                        || patNum != PatternNumber
                        || patPos != PatternPosition;

            if (!changed) return;

            bool patternChanged = patNum != PatternNumber;

            SongPosition = songPos;
            PatternNumber = patNum;
            PatternPosition = patPos;

            if (patternChanged)
                OnPatternChanged?.Invoke(songPos, patNum);

            NotifyStateChanged();
        }
        catch
        {
            // Fallback : utiliser les positions brutes du renderer
            var songPos = _renderer.SongPosition;
            var patNum = _renderer.PatternNumber;
            var patPos = _renderer.PatternPosition;

            bool changed = songPos != SongPosition
                        || patNum != PatternNumber
                        || patPos != PatternPosition;

            if (!changed) return;

            bool patternChanged = patNum != PatternNumber;

            SongPosition = songPos;
            PatternNumber = patNum;
            PatternPosition = patPos;

            if (patternChanged)
                OnPatternChanged?.Invoke(songPos, patNum);

            NotifyStateChanged();
        }
    }

    private void NotifyStateChanged() => OnStateChanged?.Invoke();

    public void Dispose()
    {
        _positionTimer?.Dispose();
    }
}
