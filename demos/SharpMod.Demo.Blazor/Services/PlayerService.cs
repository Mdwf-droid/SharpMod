using Microsoft.JSInterop;
using SharpMod;
using SharpMod.Song;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SharpMod.Demo.Blazor.Services;

public class PlayerService : IDisposable
{
    private IJSRuntime? _js;
    private ModulePlayer? _player;
    private SongModule? _module;
    private System.Threading.Timer? _uiTimer;

    // ── Propriétés UI ──
    public string? ModuleName { get; private set; }
    public string? ModuleType { get; private set; }
    public int ChannelCount { get; private set; }
    public int Speed { get; private set; }
    public int BPM { get; private set; } = 125;
    public int SongPosition { get; private set; }
    public int PatternNumber { get; private set; }
    public int PatternPosition { get; private set; }
    public bool IsPlaying { get; private set; }
    public string StatusMessage { get; set; } = "Ready ── Drop a .MOD .S3M .XM file";

    public SongModule? CurrentModule => _module;
    public ModulePlayer? CurrentPlayer => _player;

    // ── Events ──
    public event Action? OnStateChanged;

    public async Task InitializeAsync(IJSRuntime js)
    {
        _js = js;
        try
        {
            await js.InvokeVoidAsync("SharpModAudio.initialize");
        }
        catch (JSException ex)
        {
            Console.WriteLine($"WebAudio init error: {ex.Message}");
        }
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

            var renderer = new WebAudioRenderer(_js!);
            _player.RegisterRenderer(renderer);

            _player.OnGetPlayerInfos += OnPlayerInfos;
            _player.OnCurrentModulePlayEnd += OnPlayEnd;

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

        // Initialiser l'AudioContext au moment du clic utilisateur
        // (politique autoplay des navigateurs)
        try
        {
            await _js!.InvokeVoidAsync("SharpModAudio.initialize");
        }
        catch { /* déjà initialisé */ }

        _player.Start();
        IsPlaying = true;
        StatusMessage = "Playing...";

        _uiTimer = new System.Threading.Timer(_ =>
        {
            NotifyStateChanged();
        }, null, 0, 100);

        NotifyStateChanged();
    }

    public async Task StopAsync()
    {
        if (_player == null) return;
        _player.Stop();
        IsPlaying = false;
        _uiTimer?.Dispose();
        StatusMessage = "Stopped";
        await (_js?.InvokeVoidAsync("SharpModAudio.stop") ?? ValueTask.CompletedTask);
        NotifyStateChanged();
    }

    public async Task PauseAsync()
    {
        if (_player == null) return;
        _player.Pause();
        IsPlaying = !IsPlaying;
        StatusMessage = IsPlaying ? "Playing..." : "Paused";
        NotifyStateChanged();
    }

    private void OnPlayerInfos(object sender, SharpModEventArgs e)
    {
        SongPosition = e.SongPosition;
        PatternNumber = e.PatternNumber;
        PatternPosition = e.PatternPosition;
    }

    private void OnPlayEnd(object sender, EventArgs e)
    {
        IsPlaying = false;
        _uiTimer?.Dispose();
        StatusMessage = "Playback finished";
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnStateChanged?.Invoke();

    public void Dispose()
    {
        _uiTimer?.Dispose();
    }
}
