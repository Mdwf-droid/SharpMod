<div align="center">

# 🎵 SharpMod

**A 100% managed .NET port of [MikMod](http://mikmod.sourceforge.net/) — the legendary soundtrack player library**

Play Protracker, FastTracker II & ScreamTracker 3 modules directly in your .NET apps.

[![.NET Standard 2.0](https://img.shields.io/badge/.NET_Standard-2.0-5C2D91?logo=dotnet)](https://docs.microsoft.com/dotnet/standard/net-standard)
[![Blazor WASM](https://img.shields.io/badge/Blazor-WebAssembly-512BD4?logo=blazor)](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
[![WPF](https://img.shields.io/badge/WPF-SkiaSharp-0078D4?logo=windows)](https://github.com/mono/SkiaSharp)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![GitHub Pages](https://img.shields.io/badge/Demo-Live-brightgreen?logo=github)](https://mdwf-droid.github.io/SharpMod/)

[Live Demo](https://mdwf-droid.github.io/SharpMod/) · [Getting Started](#-getting-started) · [Architecture](#-architecture) · [Roadmap](#-roadmap)

</div>

---

## ✨ Features

- **100% managed C#** — no native dependencies, no P/Invoke
- **Multi-format** — MOD (Protracker), S3M (ScreamTracker 3), XM (FastTracker II)
- **.NET Standard 2.0** — runs on .NET 8+, .NET Framework 4.6.1+, Mono, Blazor WASM
- **Multiple renderers** — NAudio (desktop), WebAudio AudioWorklet (browser), extensible via `IRenderer`
- **Blazor WASM demo** — FastTracker II-style UI running entirely in the browser
- **WPF demo** — FastTracker II-style desktop app with SkiaSharp rendering
- **Real-time visualization** — per-channel oscilloscopes, VU-meters, scrolling pattern editor
- **AudioWorklet** — glitch-free playback even when the browser tab is in background
- **Latency compensation** — pattern scroll stays synchronized with audible output

---

## 🎮 Live Demo

👉 **[https://mdwf-droid.github.io/SharpMod/](https://mdwf-droid.github.io/SharpMod/)**

The Blazor WebAssembly demo features a **FastTracker II**-inspired interface with:

- 🎹 Scrolling pattern editor with note, instrument, volume & effects columns
- 📊 Per-channel oscilloscopes and VU-meters
- 📋 Instrument list panel
- 🎛️ Transport controls (Play / Stop / Pause)
- 📂 Drag & drop or file picker to load your own modules
- 🔊 AudioWorklet-based audio engine — no glitches in background tabs

> Runs 100% client-side — no server required. Just a modern browser with WebAssembly support.

---

## 🖥️ WPF Demo

A desktop **FastTracker II**-style player built with **SkiaSharp** rendering:

- 🎨 Dark FT2 theme with custom-styled controls
- 📊 Real-time spectrum analyzer (FFT), per-channel scopes & VU-meters
- 🎹 Scrolling pattern editor with synchronized horizontal scroll (scopes + pattern)
- 🔊 NAudio output (WaveOut / WASAPI)
- 📂 Drag & drop support

```bash
cd demos/SharpMod.Demo.Wpf
dotnet run
```

---

## 📦 Project Structure

```
SharpMod.sln
├── src/
│   ├── SharpMod.Core/                    (.NET Standard 2.0)
│   │   ├── DSP/                          Audio processing (FFT)
│   │   ├── Loaders/                      MOD / S3M / XM format parsers
│   │   ├── Mixer/                        Channel mixing engine
│   │   ├── Player/                       Playback engine (partial classes)
│   │   ├── Song/                         Module data structures
│   │   └── UniTracker/                   Universal tracker abstraction
│   └── SharpMod.Renderer.NAudio/         (.NET Standard 2.0)
│       └── NAudioWaveChannelDriver.cs    NAudio-based audio output
├── demos/
│   ├── SharpMod.Demo.Console/            (.NET 8.0)
│   │   └── Program.cs                    CLI player
│   ├── SharpMod.Demo.Blazor/             (.NET 8.0 Blazor WASM)
│   │   ├── Components/                   Razor UI components
│   │   ├── Services/                     PlayerService, WebAudioRenderer
│   │   └── wwwroot/                      Static assets, JS interop
│   └── SharpMod.Demo.Wpf/               (.NET 8.0-windows)
│       ├── Renderers/                    SkiaSharp renderers (Spectrum, Scopes, Pattern)
│       ├── Themes/                       FT2 theme constants
│       ├── ViewModels/                   MVVM view models
│       └── Views/                        WPF windows & controls
└── .github/workflows/
    └── deploy-ghpages.yml                Auto-deploy to GitHub Pages
```

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (or later)

### Console Player

```bash
cd demos/SharpMod.Demo.Console
dotnet run -- path/to/module.mod
```

### Blazor WASM (local dev)

```bash
cd demos/SharpMod.Demo.Blazor
dotnet run
# Open http://localhost:5180
```

### WPF Desktop Player

```bash
cd demos/SharpMod.Demo.Wpf
dotnet run
```

### Use SharpMod.Core in your project

```csharp
using SharpMod;

// Load a module
var module = ModuleLoader.Instance.LoadModule("song.mod");

Console.WriteLine($"Title:    {module.SongName}");
Console.WriteLine($"Type:     {module.ModType}");
Console.WriteLine($"Channels: {module.ChannelsCount}");

// Create player & renderer
var player = new ModulePlayer(module);
var renderer = new NAudioWaveChannelDriver(NAudioWaveChannelDriver.Output.WaveOut);
player.RegisterRenderer(renderer);

// Events
player.OnGetPlayerInfos += (s, e) =>
    Console.Write($"\rPos: {e.SongPosition:D3}  Pat: {e.PatternNumber:D3}  Row: {e.PatternPosition:D2}");

player.OnCurrentModulePlayEnd += (s, e) =>
    Console.WriteLine("\nDone!");

// Play
player.Start();
```

---

## 🏗️ Architecture

```
┌──────────────────────────────────────────────────┐
│                  Your Application                │
├──────────────────────────────────────────────────┤
│              ModulePlayer (playback)             │
│         ┌──────────┴──────────┐                  │
│    SongModule           ChannelsMixer            │
│   (parsed data)      (audio rendering)           │
├──────────────────────────────────────────────────┤
│  IModuleLoader    │         IRenderer            │
│  ┌─────────────┐  │  ┌────────────────────────┐  │
│  │ MOD Loader  │  │  │ NAudioWaveChannelDriver│  │
│  │ S3M Loader  │  │  │ WebAudio AudioWorklet  │  │
│  │ XM Loader   │  │  │ Your custom renderer   │  │
│  └─────────────┘  │  └────────────────────────┘  │
└──────────────────────────────────────────────────┘
          SharpMod.Core              Renderers
       (netstandard2.0)          (netstandard2.0)
```

### Blazor Audio Pipeline

```
┌─ Audio Thread (AudioWorklet — never throttled) ─┐
│  SharpModProcessor: FIFO → process() → speakers │
└──────────────────────┬──────────────────────────┘
                       │ postMessage (Transferable)
┌─ Main Thread ────────┼──────────────────────────┐
│  webaudio-renderer.js                           │
│  ├── setInterval → FillBuffer (C# interop)      │
│  ├── decode header (VU, scopes, positions)      │
│  └── postMessage(Float32Array) → worklet        │
│                                                 │
│  visuals-renderer.js (RAF 60fps)                │
│  ├── drawFFT, drawScopes, updatePattern         │
│  └── reads latency-compensated positions        │
└─────────────────────────────────────────────────┘
```

### Supported Formats

| Format | Extension | Tracker | Channels |
|--------|-----------|---------|----------|
| **Protracker** | `.mod` | Protracker, NoiseTracker | 4–32 |
| **ScreamTracker 3** | `.s3m` | ScreamTracker 3 | 1–32 |
| **FastTracker II** | `.xm` | FastTracker II | 1–32 |

---

## 🔧 Recent Improvements

### Core Engine
- **Track row count fix** — XM patterns with >64 rows now handled correctly
- **PatternCell optimized** — removed `INotifyPropertyChanged` overhead (zero GC pressure)
- **Mixer `Mix32to16` optimized** — `Buffer.BlockCopy` instead of byte-by-byte writes
- **Dead code removed** — legacy `AudioProcessor` (unused DSP processor) cleaned up
- **WaveTable API** — typed accessors (`GetSample`, `IsValidHandle`, `GetSampleLength`)
- **SongModule safety** — collection setters removed (prevents `module.Patterns = null`)
- **IModuleLoader interface** — injectable singleton, mockable for testing

### Blazor Demo
- **AudioWorklet** — audio runs on a separate thread, no glitches in background tabs
- **Latency compensation** — ring buffer of timestamped positions, pattern scroll synced to audible output
- **Scope decay fix** — silent channels fade smoothly to flat line (VU-based detection)

### WPF Demo
- **SkiaSharp rendering** — spectrum, scopes, VU-meters, pattern editor all GPU-accelerated
- **Custom FT2 scrollbar** — dark themed horizontal scrollbar with hover/drag states
- **Synchronized scroll** — scopes + pattern headers + pattern grid scroll together
- **Scope decay fix** — same VU-based silence detection as Blazor

---

## 🌐 Deployment

The Blazor WASM demo auto-deploys to GitHub Pages on every push to `main` via GitHub Actions.

- `base href` is rewritten from `"/"` to `"/SharpMod/"` at build time (no local files to modify)
- `.nojekyll` is added to serve `_framework/` correctly
- `404.html` enables SPA client-side routing

See [`.github/workflows/deploy-ghpages.yml`](.github/workflows/deploy-ghpages.yml) for details.

---

## 📋 Roadmap

### ✅ Done
- [x] Migrate SharpMod.Core to .NET Standard 2.0 (SDK-style)
- [x] NAudio renderer with sub-packages (Core + WinMM + Wasapi)
- [x] Console demo (net8.0)
- [x] Blazor WASM demo with FastTracker II UI
- [x] WPF demo with FastTracker II UI (SkiaSharp)
- [x] Real-time scopes, VU-meters, pattern editor (both demos)
- [x] AudioWorklet audio engine (Blazor)
- [x] Latency-compensated pattern scrolling (Blazor)
- [x] GitHub Pages auto-deployment
- [x] Remove legacy projects (Silverlight, XNA, WinForms)
- [x] Core bug fixes: Track row count, PatternCell INPC removal
- [x] Mixer performance: Mix32to16 optimization, dead code cleanup
- [x] API improvements: IModuleLoader, WaveTable accessors, read-only collections

### 🔧 In Progress
- [ ] Split `Player.cs` into partial class files (Effects.PT, Effects.S3M, Effects.XM, Envelope, etc.)
- [ ] C#-style naming conventions (progressive rename: `mp_sngpos` → `_songPosition`)
- [ ] Pattern editor — display real note/instrument/effect data
- [ ] Improve XM format support (envelope, panning)

### 🔮 Future
- [ ] NuGet packages for Core + renderers
- [ ] Unit tests
- [ ] MIDI export
- [ ] Additional formats (IT, MED)

---

## 🙏 Credits

- **[MikMod](http://mikmod.sourceforge.net/)** — the original C library by Jean-Paul Mikkers (MikMak), Jake Stine, and many contributors
- **[NAudio](https://github.com/naudio/NAudio)** — .NET audio library by Mark Heath
- **[SkiaSharp](https://github.com/mono/SkiaSharp)** — cross-platform 2D graphics library
- **[FastTracker II](https://en.wikipedia.org/wiki/FastTracker_2)** — UI inspiration for both demos

---

## 📜 History

SharpMod was originally created in **2011** as a .NET Framework 3.5 port of MikMod. After years of dormancy, the project was **rebooted in 2025** with:

- Full migration to **.NET Standard 2.0**
- Modern SDK-style projects
- A **Blazor WebAssembly** demo bringing tracker music to the browser
- A **WPF desktop** demo with SkiaSharp-rendered FastTracker II UI
- Core engine improvements (bug fixes, performance, API cleanup)
- Automated CI/CD via GitHub Actions

---

## 📄 License

[MIT](LICENSE) — Use it, fork it, mod it. 🎶
