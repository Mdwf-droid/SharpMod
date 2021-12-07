using System;
using System.Collections.Generic;
using System.Text;

namespace SharpMod.Player
{
    /*[Flags]
	public enum DMode:int
	{
        DMODE_STEREO = 1,
        DMODE_16BITS = 2,
        DMODE_INTERP = 4,
        DMODE_SURROUND = 8
	}*/

    public class MixConfig
    {
        public bool Interpolate { get; set; }
        public bool Is16Bits { get; set; }
        public bool NoiseReduction { get; set; }
        public RenderingStyle Style { get; set; }
        public int Rate { get; set; }
        private int _reverb =0;        
        public int Reverb
        {
            get { return _reverb; }
            set { _reverb = value < 15 ? value : 15; }
        }

        public MixConfig()
        {
            Is16Bits = true;
            Style = RenderingStyle.Stereo;            
        }
    }

    public enum RenderingStyle
    {
        Mono,
        Stereo,
        Surround
    }
    
}
