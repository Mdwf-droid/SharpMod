using Microsoft.JSInterop;
using System;

namespace SharpMod.Demo.Blazor.Services;

public class WebAudioRenderer : IRenderer
{
    private readonly IJSRuntime _js;
    private DotNetObjectReference<WebAudioRenderer> _dotNetRef;
    private byte[] _outputBuffer;
    private byte[] _resultBuffer;

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
        PatternChanged?.Invoke(songPosition, patternNumber);
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

    private static void WriteInt32(byte[] buf, int offset, int value)
    {
        buf[offset] = (byte)value;
        buf[offset + 1] = (byte)(value >> 8);
        buf[offset + 2] = (byte)(value >> 16);
        buf[offset + 3] = (byte)(value >> 24);
    }
}
