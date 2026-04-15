using System;
using SharpMod;

namespace SharpMod.Song
{	
	public class Sample
    {
        private int _c2Spd; /* finetune frequency */

        public int C2Spd
        {
            get { return _c2Spd; }
            set { _c2Spd = value; }
        }
        private sbyte _transpose; /* transpose value */

        public sbyte Transpose
        {
            get { return _transpose; }
            set { _transpose = value; }
        }
        private short _volume; /* volume 0-64 */

        public short Volume
        {
            get { return _volume; }
            set { _volume = value; }
        }
        private short _panning; /* panning */

        public short Panning
        {
            get { return _panning; }
            set { _panning = value; }
        }
        private int _length; /* length of sample (in samples!) */

        public int Length
        {
            get { return _length; }
            set { _length = value; }
        }
        private int _loopStart; /* repeat position (relative to start, in samples) */

        public int LoopStart
        {
            get { return _loopStart; }
            set { _loopStart = value; }
        }
        private int _loopEnd; /* repeat end */

        public int LoopEnd
        {
            get { return _loopEnd; }
            set { _loopEnd = value; }
        }
        private SampleFormatFlags _flags; /* sample format */

        public SampleFormatFlags Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }
        private int _seekPos; /* seek position in file */

        public int SeekPos
        {
            get { return _seekPos; }
            set { _seekPos = value; }
        }

        /// <summary>
        /// Name of the sample
        /// </summary>
        public String SampleName { get; set; }
        
        /// <summary>
        /// Sample handle
        /// </summary>
        public int Handle { get; set; }

        /// <summary>
        /// Byte stream of the sample
        /// </summary>
        public byte[] SampleBytes
        {
            get;
            set;
        }

        public int SampleRate
        {
            get;
            set;
        }
	}
}