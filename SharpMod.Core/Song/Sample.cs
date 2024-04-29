namespace SharpMod.Song
{
    public class Sample
    {
        public int C2Spd { get; set; }

        public sbyte Transpose { get; set; }

        public short Volume { get; set; }

        public short Panning { get; set; }

        public int Length { get; set; }

        public int LoopStart { get; set; }

        public int LoopEnd { get; set; }

        public SampleFormats Flags { get; set; }

        public int SeekPos { get; set; }

        /// <summary>
        /// Name of the sample
        /// </summary>
        public string SampleName { get; set; }

        /// <summary>
        /// Sample handle
        /// </summary>
        public int Handle { get; set; }

        /// <summary>
        /// Byte stream of the sample
        /// </summary>
        public byte[] SampleBytes { get; set; }

        public int SampleRate { get; set; }
    }
}