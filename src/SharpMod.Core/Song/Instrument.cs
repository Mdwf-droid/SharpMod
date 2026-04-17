using SharpMod.Player;
using System.Collections.Generic;

namespace SharpMod.Song;

public class Instrument
{
    public const int MaxNoteMapping = 96;
    public const int MaxEnvelopePoints = 12;

    public short NumSmp { get; set; }
    public short[] SampleNumber { get; set; } = new short[MaxNoteMapping];

    // Volume envelope
    public short VolFlg { get; set; }
    public short VolPts { get; set; }
    public short VolSus { get; set; }
    public short VolBeg { get; set; }
    public short VolEnd { get; set; }
    public EnvPt[] VolEnv { get; set; }

    // Panning envelope
    public short PanFlg { get; set; }
    public short PanPts { get; set; }
    public short PanSus { get; set; }
    public short PanBeg { get; set; }
    public short PanEnd { get; set; }
    public EnvPt[] PanEnv { get; set; }

    // Vibrato
    public short VibType { get; set; }
    public short VibSweep { get; set; }
    public short VibDepth { get; set; }
    public short VibRate { get; set; }
    public int VolFade { get; set; }

    public string InsName { get; set; }
    public List<Sample> Samples { get; set; } = new();

    public Instrument()
    {
        VolEnv = new EnvPt[MaxEnvelopePoints];
        PanEnv = new EnvPt[MaxEnvelopePoints];
        for (int i = 0; i < MaxEnvelopePoints; i++)
        {
            VolEnv[i] = new EnvPt();
            PanEnv[i] = new EnvPt();
        }
    }
}
