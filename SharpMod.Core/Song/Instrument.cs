using SharpMod.Player;
using System;
using System.Collections.Generic;

namespace SharpMod.Song
{
    public class Instrument
    {
        public short NumSmp { get; set; }

        public short[] SampleNumber { get; set; }

        /* bit 0: on 1: sustain 2: loop */
        public short VolFlg { get; set; }

        public short VolPts { get; set; }

        public short VolSus { get; set; }

        public short VolBeg { get; set; }

        public short VolEnd { get; set; }

        public EnvPt[] VolEnv { get; set; }

        /* bit 0: on 1: sustain 2: loop */
        public short PanFlg { get; set; }

        public short PanPts { get; set; }

        public short PanSus { get; set; }

        public short PanBeg { get; set; }

        public short PanEnd { get; set; }

        public EnvPt[] PanEnv { get; set; }

        public short VibType { get; set; }

        public short VibSweep { get; set; }

        public short VibDepth { get; set; }

        public short VibRate { get; set; }

        public int VolFade { get; set; }

        public String InsName { get; set; }

        public List<Sample> Samples { get; set; }

        public Instrument()
        {
            SampleNumber = new short[96];
            VolEnv = new EnvPt[12];
            PanEnv = new EnvPt[12];
            int i;
            for (i = 0; i < 12; i++)
            {
                VolEnv[i] = new EnvPt();
                PanEnv[i] = new EnvPt();
            }
        }
    }
}