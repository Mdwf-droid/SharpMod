using Microsoft.JSInterop;
using System;

namespace SharpMod.Demo.Blazor.Services;

public class WebAudioRenderer : IRenderer
{
    private readonly IJSRuntime _js;
    private DotNetObjectReference<WebAudioRenderer> _dotNetRef;
    private byte[] _outputBuffer;
    private byte[] _resultBuffer;
    private byte[]? _audioOnlyBuffer;
    private byte[]? _visualsOnlyBuffer;

    private const int SCOPE_SIZE = 128; // Doit correspondre à SCOPE_BUFFER_SIZE du mixer

    // Position cachée depuis OnGetPlayerInfos
    private int _songPosition;
    private int _patternNumber;
    private int _patternPosition;

    // VU buffers pré-alloués
    private int[] _vuVolumes = new int[32];
    private int[] _vuPeaks = new int[32];

    public ModulePlayer Player { get; set; }
    public event Action<int, int> PatternChanged;

    // Event déclenché par le JS à chaque changement de position (throttlé 10fps)
    public event Action<int, int, int>? OnPositionChanged;

    public WebAudioRenderer(IJSRuntime js)
    {
        _js = js;
    }

    public void Init()
    {
        _dotNetRef = DotNetObjectReference.Create(this);
        _ = _js.InvokeVoidAsync("SharpModAudio.setDotNetReference", _dotNetRef);

        if (Player != null)
        {
            Player.OnGetPlayerInfos += OnPlayerInfos;
        }
    }

    private void OnPlayerInfos(object sender, SharpModEventArgs e)
    {
        _songPosition = e.SongPosition;
        _patternNumber = e.PatternNumber;
        _patternPosition = e.PatternPosition;
    }

    public void PlayStart()
    {
        _ = _js.InvokeVoidAsync("SharpModAudio.play");
    }

    public void PlayStop()
    {
        if (Player != null)
            Player.OnGetPlayerInfos -= OnPlayerInfos;
        _ = _js.InvokeVoidAsync("SharpModAudio.stop");
    }

    [JSInvokable]
    public void OnPatternChanged(int songPosition, int patternNumber)
    {
        _songPosition = songPosition;
        _patternNumber = patternNumber;
        PatternChanged?.Invoke(songPosition, patternNumber);
    }

    // Appelé par le JS depuis _fetchVisuals()
    [JSInvokable]
    public void NotifyPosition(int songPosition, int patternNumber, int patternPosition)
    {
        _songPosition = songPosition;
        _patternNumber = patternNumber;
        _patternPosition = patternPosition;
        OnPositionChanged?.Invoke(songPosition, patternNumber, patternPosition);
    }

    [JSInvokable]
    public byte[] FillBuffer(int byteCount)
    {
        if (Player == null) return Array.Empty<byte>();

        int channelCount = Player.CurrentModule?.ChannelsCount ?? 0;
        if (channelCount > 32) channelCount = 32;

        // ── Calculer la taille du header ──
        // 16 bytes fixe + channelCount VU bytes + channelCount × SCOPE_SIZE scope bytes
        int headerSize = 16 + channelCount + (channelCount * SCOPE_SIZE);

        // ── Allouer les buffers ──
        if (_outputBuffer == null || _outputBuffer.Length != byteCount)
            _outputBuffer = new byte[byteCount];

        int totalSize = headerSize + byteCount;
        if (_resultBuffer == null || _resultBuffer.Length != totalSize)
            _resultBuffer = new byte[totalSize];

        // ── Remplir l'audio ──
        Player.GetBytes(_outputBuffer, byteCount);

        // ── Header : position (12 bytes) ──
        WriteInt32(_resultBuffer, 0, _songPosition);
        WriteInt32(_resultBuffer, 4, _patternNumber);
        WriteInt32(_resultBuffer, 8, _patternPosition);
        WriteInt32(_resultBuffer, 12, channelCount);

        // ── Header : VU levels par canal ──
        int vuCount;
        Player.GetChannelLevels(out _vuVolumes, out _vuPeaks, out vuCount);
        int vuOffset = 16;
        for (int ch = 0; ch < channelCount; ch++)
        {
            // Peak level 0-128, on clamp à 0-255 pour un byte
            int peak = ch < vuCount ? _vuPeaks[ch] : 0;
            // peak est 0-128 dans le mixer
            // On veut que peak=64 (mi-volume) donne déjà ~200/255
            int val = Math.Min(255, (int)(Math.Sqrt(peak / 128.0) * 255));
            _resultBuffer[vuOffset + ch] = (byte)val;
        }

        // ── Header : scope data par canal (128 sbytes chacun) ──
        int scopeOffset = vuOffset + channelCount;
        for (int ch = 0; ch < channelCount; ch++)
        {
            sbyte[] scopeData = Player.GetScopeData(ch);
            int destOff = scopeOffset + ch * SCOPE_SIZE;

            if (scopeData != null && scopeData.Length >= SCOPE_SIZE)
            {
                // sbyte[] → byte[] : même layout mémoire
                Buffer.BlockCopy(scopeData, 0, _resultBuffer, destOff, SCOPE_SIZE);
            }
            else
            {
                // Canal inactif → silence
                Array.Clear(_resultBuffer, destOff, SCOPE_SIZE);
            }
        }

        // ── Audio PCM après le header ──
        Buffer.BlockCopy(_outputBuffer, 0, _resultBuffer, headerSize, byteCount);

        return _resultBuffer;
    }

    [JSInvokable]
    public byte[]? FillAudio(int byteCount)
    {
        if (Player == null || !Player.IsPlaying)
            return null;

        if (_audioOnlyBuffer == null || _audioOnlyBuffer.Length < byteCount)
            _audioOnlyBuffer = new byte[byteCount];

        int read = Player.GetBytes(_audioOnlyBuffer, byteCount);
        if (read <= 0)
            return null;

        if (read == byteCount)
            return _audioOnlyBuffer;

        // Rare : retourner un slice exact
        var result = new byte[read];
        Buffer.BlockCopy(_audioOnlyBuffer, 0, result, 0, read);
        return result;
    }

    [JSInvokable]
    public byte[]? FillVisuals()
    {
        if (Player == null || !Player.IsPlaying)
            return null;

        var module = Player.CurrentModule;
        if (module == null)
            return null;

        int channels = module.ChannelsCount;
        int headerSize = 16 + channels + (channels * 128); // même calcul que FillBuffer

        if (_visualsOnlyBuffer == null || _visualsOnlyBuffer.Length < headerSize)
            _visualsOnlyBuffer = new byte[headerSize];

        int offset = 0;

        // ── Header fixe (16 bytes) — même code que FillBuffer ──
        WriteInt32LE(_visualsOnlyBuffer, offset, _songPosition); offset += 4;
        WriteInt32LE(_visualsOnlyBuffer, offset, _patternNumber); offset += 4;
        WriteInt32LE(_visualsOnlyBuffer, offset, _patternPosition); offset += 4;
        WriteInt32LE(_visualsOnlyBuffer, offset, channels); offset += 4;

        // ── VU peaks — même code que FillBuffer ──
        Player.GetChannelLevels(out int[] volumes, out int[] peaks, out int count);
        for (int ch = 0; ch < channels; ch++)
        {
            int peak = (ch < count) ? peaks[ch] : 0;
            _visualsOnlyBuffer[offset++] = (byte)Math.Min(255, Math.Max(0, peak));
        }

        // ── Scope data — même code que FillBuffer ──
        for (int ch = 0; ch < channels; ch++)
        {
            sbyte[]? scopeData = Player.GetScopeData(ch);
            if (scopeData != null)
            {
                int len = Math.Min(scopeData.Length, 128);
                for (int i = 0; i < len; i++)
                    _visualsOnlyBuffer[offset + i] = (byte)scopeData[i];
                for (int i = len; i < 128; i++)
                    _visualsOnlyBuffer[offset + i] = 0;
            }
            else
            {
                for (int i = 0; i < 128; i++)
                    _visualsOnlyBuffer[offset + i] = 0;
            }
            offset += 128;
        }

        return _visualsOnlyBuffer;
    }

    // Helper — si tu n'as pas déjà cette méthode dans la classe
    private static void WriteInt32LE(byte[] buf, int offset, int value)
    {
        buf[offset] = (byte)(value & 0xFF);
        buf[offset + 1] = (byte)((value >> 8) & 0xFF);
        buf[offset + 2] = (byte)((value >> 16) & 0xFF);
        buf[offset + 3] = (byte)((value >> 24) & 0xFF);
    }

    private static void WriteInt32(byte[] buf, int offset, int value)
    {
        buf[offset] = (byte)value;
        buf[offset + 1] = (byte)(value >> 8);
        buf[offset + 2] = (byte)(value >> 16);
        buf[offset + 3] = (byte)(value >> 24);
    }
}
