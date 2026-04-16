using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.JSInterop;
using SharpMod;
using SharpMod.Song;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SharpMod.Demo.Blazor.Services;

public class PlayerService : IDisposable
{
    private IJSRuntime _js;
    private ModulePlayer _player;
    private SongModule _module;

    public string ModuleName { get; private set; }
    public string ModuleType { get; private set; }
    public int ChannelCount { get; private set; }
    public int Speed { get; private set; }
    public int BPM { get; private set; } = 125;
    public bool IsPlaying { get; private set; }
    public string StatusMessage { get; set; } = "Ready ── Drop a .MOD .S3M .XM file";

    public int SongPosition { get; set; }
    public int PatternNumber { get; set; }
    public int PatternPosition { get; set; }

    public SongModule CurrentModule => _module;
    public ModulePlayer CurrentPlayer => _player;

    public event Action OnStateChanged;

    private WebAudioRenderer _renderer;

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

            _renderer = new WebAudioRenderer(_js);
            _player.RegisterRenderer(_renderer);

            // PAS de renderer.PatternChanged
            // PAS de timer
            // La position est encodée dans FillBuffer

            ModuleName = _module.SongName;
            ModuleType = _module.ModType;
            ChannelCount = _module.ChannelsCount;
            Speed = _module.InitialSpeed;
            BPM = _module.InitialTempo;

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

        try { await _js.InvokeVoidAsync("SharpModAudio.initialize"); }
        catch { }

        if (_renderer != null)
        {
            _renderer.OnPositionChanged += OnRendererPositionChanged;
        }

        _player.Start();
        IsPlaying = true;
        StatusMessage = "Playing...";
        NotifyStateChanged();
    }

    public async Task StopAsync()
    {
        if (_player == null) return;



        _player.Stop();

        if (_renderer != null)
        {
            _renderer.OnPositionChanged -= OnRendererPositionChanged;
        }

        IsPlaying = false;
        SongPosition = 0;
        PatternNumber = 0;
        PatternPosition = 0;
        StatusMessage = "Stopped";
        await (_js?.InvokeVoidAsync("SharpModAudio.stop") ?? ValueTask.CompletedTask);
        NotifyStateChanged();
    }

    public async Task PauseAsync()
    {
        if (_player == null) return;

        _player.Pause();
        IsPlaying = !IsPlaying;

        // Synchroniser l'état avec le JS AudioContext
        await _js.InvokeVoidAsync("SharpModAudio.pause");

        StatusMessage = IsPlaying ? "Playing..." : "Paused";
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnStateChanged?.Invoke();

    private void OnRendererPositionChanged(int songPos, int patNum, int patPos)
    {
        SongPosition = songPos;
        PatternNumber = patNum;
        PatternPosition = patPos;
        NotifyStateChanged();
    }

    public void Dispose() { }
}
