namespace SharpMod.Player
{

    public class MixConfig
    {
        public bool Interpolate { get; set; }
        public bool Is16Bits { get; set; }
        public bool NoiseReduction { get; set; }
        public RenderingStyle Style { get; set; }
        public int Rate { get; set; }
        private int _reverb = 0;
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
