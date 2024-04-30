﻿using System.Collections.Generic;
using System.IO;

namespace SharpMod
{
    /// <summary>
    /// Wave Table used by the player
    /// </summary>
    public class WaveTable
    {
        /// <summary>
        /// Table of samples bytes with handle as key
        /// </summary>
        public byte[][] Samples { get; set; }

        private static readonly char[] RiffHeader = ['R', 'I', 'F', 'F'];
        private static readonly char[] WaveFmtHeader = ['W', 'A', 'V', 'E', 'f', 'm', 't', ' '];
        private static readonly char[] DataHeader = new[] { 'd', 'a', 't', 'a' };

        ///<summary>
        ///</summary>
        public WaveTable()
        {
            Samples = [];
        }

        /// <summary>
        /// Get Sample in the Wave table by his key
        /// </summary>
        /// <param name="handle">Handle of the sample in the table</param>
        /// <returns>array of byte of the sample if found or null </returns>
        public byte[] GetSample(int handle)
        {
            byte[] toReturn = null;

            if (handle < Samples.Length)
                toReturn = Samples[handle];

            return toReturn;
        }

        /// <summary>
        /// Add a sample array of bytes in the wave table
        /// if the handle exist, replace old sample with the new
        /// </summary>
        /// <param name="sampleBytes">Array of byte of the sample to add</param>
        /// <param name="handle">Handle of the sample in the wave table</param>
        public void AddSample(byte[] sampleBytes, int handle)
        {
            var tmp = new List<byte[]>(Samples)
            {
                sampleBytes
            };

            Samples = [.. tmp];
        }

        ///<summary>
        ///</summary>
        ///<param name="handle"></param>
        ///<param name="sampleRate"></param>
        ///<param name="bits"></param>
        ///<param name="channels"></param>
        ///<returns></returns>
        public Stream GetSampleWaveStream(int handle, int sampleRate, int bits, int channels)
        {
            var ms = new MemoryStream();

            var blockAlign = (short)(channels * (bits / 8));
            var averageBytesPerSecond = sampleRate * blockAlign;

            var w = new BinaryWriter(ms);
            w.Write(RiffHeader);
            w.Write(0); // placeholder
            w.Write(WaveFmtHeader);

            w.Write(18); // wave format length
            w.Write((short)1);
            w.Write((short)channels);
            w.Write(sampleRate);
            w.Write(averageBytesPerSecond);
            w.Write(blockAlign);
            w.Write((short)bits);
            w.Write((short)0);


            w.Write(DataHeader);
            var dataSizePos = ms.Position;
            w.Write(0); // placeholder

            w.Write(Samples[handle], 0, Samples[handle].Length);

            w.Flush();
            w.Seek(4, SeekOrigin.Begin);
            w.Write((int)(ms.Length - 8));
            w.Seek((int)dataSizePos, SeekOrigin.Begin);
            w.Write(Samples[handle].Length);
            ms = new MemoryStream(ms.GetBuffer());
            w.Close();

            return ms;
        }
    }
}
