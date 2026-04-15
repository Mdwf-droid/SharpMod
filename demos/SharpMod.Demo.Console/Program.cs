using System;
using System.Text;
using SharpMod;
using SharpMod.SoundRenderer;

// Encodage DOS pour les noms de samples
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

if (args.Length == 0)
{
    Console.WriteLine("Usage: SharpMod.Demo.Console <module_file>");
    Console.WriteLine("Supported formats: .MOD .S3M .XM .M15");
    return;
}

var filePath = args[0];
if (!System.IO.File.Exists(filePath))
{
    Console.WriteLine($"File not found: {filePath}");
    return;
}

Console.WriteLine("🎵 SharpMod Console Player v2.0");
Console.WriteLine($"Loading: {filePath}");

// Chargement du module
var module = ModuleLoader.Instance.LoadModule(filePath);

if (module == null)
{
    Console.WriteLine("Error: Unable to load module.");
    return;
}

Console.WriteLine($"Title:    {module.SongName}");
Console.WriteLine($"Type:     {module.ModType}");
Console.WriteLine($"Channels: {module.ChannelsCount}");
Console.WriteLine($"Patterns: {module.Patterns.Count}");
Console.WriteLine();

// Initialisation du player (constructeur requiert le SongModule)
var player = new ModulePlayer(module);

// Configuration du renderer NAudio
var renderer = new NAudioWaveChannelDriver(NAudioWaveChannelDriver.Output.WaveOut);

player.RegisterRenderer(renderer);

// Event de fin de module
player.OnCurrentModulePlayEnd += (sender, e) =>
{
    Console.WriteLine("\n✅ Playback finished.");
    Environment.Exit(0);
};

// Event d'infos de lecture
player.OnGetPlayerInfos += (sender, e) =>
{
    Console.Write($"\r  SngPos: {e.SongPosition:D3}  " +
                  $"Pattern: {e.PatternNumber:D3}  " +
                  $"Row: {e.PatternPosition:D2}  ");
};

// Lecture
Console.WriteLine("▶ Playing... (Press Q to quit)");
Console.WriteLine();
player.Start();

// Boucle d'interaction
while (true)
{
    if (Console.KeyAvailable)
    {
        var key = Console.ReadKey(true).Key;
        if (key == ConsoleKey.Q)
        {
            player.Stop();
            Console.WriteLine("\n⏹ Stopped.");
            return;
        }
    }
    System.Threading.Thread.Sleep(50);
}
