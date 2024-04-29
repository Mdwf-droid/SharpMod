
namespace SharpMod.Mixer
{
    /// <summary>
    /// Channel Playing Memory
    /// </summary>
    public class ChannelInfo
    {
        /// <summary>
        /// if true -> sample has to be restarted
        /// </summary>
        public bool Kick { get; set; }

        /// <summary>
        /// if true -> sample is playing
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// 16/8 bits looping/one-shot
        /// </summary>
        public SampleFormats Flags { get; set; }

        /// <summary>
        /// identifies the sample
        /// </summary>
        public int Handle { get; set; }

        /// <summary>
        /// start index
        /// </summary>
        public int Start { get; set; }

        /// <summary>
        /// samplesize
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// loop start 
        /// </summary>
        public int Reppos { get; set; }

        /// <summary>
        /// loop end
        /// </summary>
        public int Repend { get; set; }

        /// <summary>
        /// current frequency
        /// </summary>
        public int Frq { get; set; }

        /// <summary>
        /// current volume
        /// </summary>
        public short Vol { get; set; }

        /// <summary>
        /// current panning position 
        /// </summary>
        public short Pan { get; set; }

        /// <summary>
        /// current index in the sample
        /// </summary>
        public int Current { get; set; }

        /// <summary>
        /// fixed-point increment value
        /// </summary>
        public int Increment { get; set; }

        /// <summary>
        /// left volume multiply
        /// </summary>
        public int LeftVolMul { get; set; }

        /// <summary>
        /// right volume multiply
        /// </summary>
        public int RightVolMul { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int RampVol { get; set; }

        public int OldVol { get; set; }

        public int OldLeftVol { get; set; }

        public int OldRightVol { get; set; }

        public int Click { get; set; }

        public int LastValLeft { get; set; }

        public int LastValRight { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ChannelInfo()
        {
        }
    }
}