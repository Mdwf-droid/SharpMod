using Microsoft.JSInterop;
using System;

namespace SharpMod.Demo.Blazor.Services;

public class WebAudioRenderer : IRenderer
{
    private readonly IJSRuntime _js;
    private DotNetObjectReference<WebAudioRenderer>? _dotNetRef;

    public ModulePlayer? Player { get; set; }

    public WebAudioRenderer(IJSRuntime js)
    {
        _js = js;
    }

    public void Init()
    {
        // Créer la référence .NET et l'envoyer au JS
        _dotNetRef = DotNetObjectReference.Create(this);
        _ = _js.InvokeVoidAsync("SharpModAudio.setDotNetReference", _dotNetRef);
    }

    public void PlayStart()
    {
        _ = _js.InvokeVoidAsync("SharpModAudio.play");
    }

    public void PlayStop()
    {
        _ = _js.InvokeVoidAsync("SharpModAudio.stop");
    }

    [JSInvokable]
    public byte[] FillBuffer(int byteCount)
    {
        if (Player == null) return Array.Empty<byte>();

        var buffer = new byte[byteCount];
        Player.GetBytes(buffer, byteCount);
        return buffer;
    }
}