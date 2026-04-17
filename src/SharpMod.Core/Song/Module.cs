using SharpMod.UniTracker;
using System.Collections.Generic;

namespace SharpMod.Song;

public class SongModule
{
    // ★ Plus de setter public — les listes sont créées une seule fois
    public List<int> Positions { get; } = [];
    public List<Pattern> Patterns { get; } = [];
    public List<Instrument> Instruments { get; } = [];

    // Garder tout le reste tel quel
    public short InitialSpeed { get; set; }
    public short InitialTempo { get; set; }
    public int ChannelsCount { get; set; }
    public short[] Panning { get; set; }
    public UniModFlags Flags { get; set; }
    public string SongName { get; set; }
    public string ModType { get; set; }
    public string Comment { get; set; }
    public short RepPos { get; set; }

    public SongModule()
    {
        Panning = new short[32];
    }
}