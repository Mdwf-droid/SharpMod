using System;
using SharpMod;


namespace SharpMod.Mixer
{	
    /// <summary>
    /// Channel Playing Memory
    /// </summary>
	public class ChannelInfo
	{
        private bool _kick;
        /// <summary>
        /// if true -> sample has to be restarted
        /// </summary>
        public bool Kick
        {
            get { return _kick; }
            set { _kick = value; }
        }

        private bool _active;
        /// <summary>
        /// if true -> sample is playing
        /// </summary>
        public bool Active
        {
            get { return _active; }
            set { _active = value; }
        }

        private SampleFormatFlags _flags;
        /// <summary>
        /// 16/8 bits looping/one-shot
        /// </summary>
        public SampleFormatFlags Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        private int _handle;
        /// <summary>
        /// identifies the sample
        /// </summary>
        public int Handle
        {
            get { return _handle; }
            set { _handle = value; }
        }

        private int _start;
        /// <summary>
        /// start index
        /// </summary>
        public int Start
        {
            get { return _start; }
            set { _start = value; }
        }

        private int _size;
        /// <summary>
        /// samplesize
        /// </summary>
        public int Size
        {
            get { return _size; }
            set { _size = value; }
        }

        private int _reppos; 
        /// <summary>
        /// loop start 
        /// </summary>
        public int Reppos
        {
            get { return _reppos; }
            set { _reppos = value; }
        }

        private int _repend;
        /// <summary>
        /// loop end
        /// </summary>
        public int Repend
        {
            get { return _repend; }
            set { _repend = value; }
        }

        private int _frq;
        /// <summary>
        /// current frequency
        /// </summary>
        public int Frq
        {
            get { return _frq; }
            set { _frq = value; }
        }

        private short _vol;
        /// <summary>
        /// current volume
        /// </summary>
        public short Vol
        {
            get { return _vol; }
            set { _vol = value; }
        }

        private short _pan; 
        /// <summary>
        /// current panning position 
        /// </summary>
        public short Pan
        {
            get { return _pan; }
            set { _pan = value; }
        }

        private int _current;
        /// <summary>
        /// current index in the sample
        /// </summary>
        public int Current
        {
            get { return _current; }
            set { _current = value; }
        }

        private int _increment;
        /// <summary>
        /// fixed-point increment value
        /// </summary>
        public int Increment
        {
            get { return _increment; }
            set { _increment = value; }
        }

        private int _leftVolMul;
        /// <summary>
        /// left volume multiply
        /// </summary>
        public int LeftVolMul
        {
            get { return _leftVolMul; }
            set { _leftVolMul = value; }
        }

        private int _rightVolMul;
        /// <summary>
        /// right volume multiply
        /// </summary>
        public int RightVolMul
        {
            get { return _rightVolMul; }
            set { _rightVolMul = value; }
        }

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