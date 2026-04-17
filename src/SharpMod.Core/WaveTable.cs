using System;
using System.Collections.Generic;
using System.IO;

namespace SharpMod
{
    /// <summary>
    /// Wave Table used by the player
    /// </summary>
    public class WaveTable
    {
        /// <summary>
        /// Raw sample data indexed by handle.
        /// Internal car le mixer y accède directement pour la perf.
        /// </summary>
        internal byte[][] Samples;

        private List<byte[]> sampleList = new List<byte[]>();

        /// <summary>
        /// Nombre de samples chargés.
        /// </summary>
        public int Count => sampleList.Count;

        public WaveTable(int maxHandles = 128)
        {
            Samples = new byte[maxHandles][];
        }

        /// <summary>
        /// Récupérer un sample brut par handle, avec bounds check.
        /// Retourne null si le handle est invalide.
        /// </summary>
        public byte[] GetSample(int handle)
        {
            if (handle < 0 || handle >= Samples.Length)
                return null;
            return Samples[handle];
        }

        /// <summary>
        /// Récupérer un sample brut par handle.
        /// Lève une exception si le handle est invalide.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public byte[] GetSampleChecked(int handle)
        {
            if (handle < 0 || handle >= Samples.Length)
                throw new ArgumentOutOfRangeException(nameof(handle),
                    $"Sample handle {handle} is out of range [0, {Samples.Length - 1}]");
            return Samples[handle]
                ?? throw new InvalidOperationException(
                    $"Sample handle {handle} has no data loaded");
        }

        /// <summary>
        /// Taille d'un sample en bytes. 0 si le handle est invalide.
        /// </summary>
        public int GetSampleLength(int handle)
        {
            var s = GetSample(handle);
            return s?.Length ?? 0;
        }

        /// <summary>
        /// Vérifier qu'un handle est valide et a des données.
        /// </summary>
        public bool IsValidHandle(int handle)
        {
            return handle >= 0
                && handle < Samples.Length
                && Samples[handle] != null
                && Samples[handle].Length > 0;
        }

        // ═══ Méthodes existantes (garder telles quelles) ═══

        public void AddSample(byte[] sampleBytes, int handle)
        {
            sampleList.Add(sampleBytes);
            Samples[handle] = sampleBytes;
        }

        ///<summary>
        ///</summary>
        ///<param name="handle"></param>
        ///<param name="sampleRate"></param>
        ///<param name="bits"></param>
        ///<param name="channels"></param>
        ///<returns></returns>
        public Stream GetSampleWaveStream(int handle, int sampleRate,int bits, int channels)
        {
            var ms = new MemoryStream();

            var blockAlign = (short)(channels * (bits / 8));
            var averageBytesPerSecond = sampleRate * blockAlign;

            var w = new BinaryWriter(ms);
            w.Write(['R','I','F','F']);
            w.Write(0); // placeholder
            w.Write(['W','A','V','E','f','m','t',' ']);

            w.Write(18); // wave format length
            w.Write((short)1);
            w.Write((short)channels);
            w.Write(sampleRate);
            w.Write(averageBytesPerSecond);
            w.Write(blockAlign);
            w.Write((short)bits);
            w.Write((short)0);
            //format.Serialize(w);

            w.Write(['d','a','t','a'] );
            var dataSizePos = ms.Position;
            w.Write(0); // placeholder

            w.Write(Samples[handle], 0, Samples[handle].Length);
            //dataChunkSize += count;

            w.Flush();
            w.Seek(4, SeekOrigin.Begin);
            w.Write((int)(ms.Length - 8));
            w.Seek((int)dataSizePos, SeekOrigin.Begin);
            w.Write(Samples[handle].Length);
            ms=new MemoryStream(ms.GetBuffer());
            w.Close();

            return ms;
        }
    }
}
