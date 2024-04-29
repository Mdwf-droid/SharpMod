using SharpMod.Player;
using System;
using System.IO;
using System.Threading;

namespace SharpMod.SoundRenderer
{
    ///<summary>
    ///</summary>
    public class WaveExporter : IRenderer
    {
        private Stream _exportStream;
        private BinaryWriter _exportWriter;
        private const int BufferLength = 32768;

        private int _dumpSize;
        Thread _threadExporter;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filename">File name with full path to export</param>
        public WaveExporter(string filename)
        {
            _exportStream = File.OpenWrite(filename);

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="destinationStream">Stream to export to</param>
        public WaveExporter(Stream destinationStream)
        {
            _exportStream = destinationStream;

        }

        #region IRenderer Membres
        ///<summary>
        ///</summary>
        public void Init()
        {
            _dumpSize = 0;
            _exportWriter = new BinaryWriter(_exportStream);
            WriteHeader();
            _threadExporter = new Thread(LetsGo);
        }

        ///<summary>
        ///</summary>
        public void PlayStart()
        {

            _threadExporter.Start();
        }

        ///<summary>
        ///</summary>
        public void PlayStop()
        {
            _threadExporter.Join();
            WriteHeader();
        }

        ///<summary>
        ///</summary>
        public ModulePlayer Player
        {
            get;
            set;
        }

        private static readonly char[] RiffHeader = ['R', 'I', 'F', 'F'];
        private static readonly char[] WaveFmtHeader = ['W', 'A', 'V', 'E', 'f', 'm', 't', ' '];
        private static readonly char[] DataHeader = ['d', 'a', 't', 'a'];

        #endregion

        ///<summary>
        ///</summary>
        public void LetsGo()
        {
            var _buffer = new byte[BufferLength];
            int read;
            while (Player.IsPlaying && (read = Player.GetBytes(_buffer, BufferLength)) > 0)
            {
                _dumpSize += read;
                _exportWriter.Write(_buffer, 0, read);
                _buffer = new byte[BufferLength];
            }
        }

        private void WriteHeader()
        {
            _exportStream.Seek(0, SeekOrigin.Begin);
            _exportWriter.Write(RiffHeader);
            _exportWriter.Write(_dumpSize + 44);
            _exportWriter.Write(WaveFmtHeader);
            _exportWriter.Write(16);/* length of this RIFF block crap */
            _exportWriter.Write((short)1);/* microsoft format type */
            _exportWriter.Write((short)(Player.MixCfg.Style == RenderingStyle.Mono ? 1 : 2));
            _exportWriter.Write(Player.MixCfg.Rate);
            _exportWriter.Write(Player.MixCfg.Rate * (Player.MixCfg.Style == RenderingStyle.Mono ? 1 : 2) * (Player.MixCfg.Is16Bits ? 2 : 1));

            /* block alignment (8/16 bit) */
            _exportWriter.Write((short)((Player.MixCfg.Style == RenderingStyle.Mono ? 1 : 2) * (Player.MixCfg.Is16Bits ? 2 : 1)));

            _exportWriter.Write((short)(Player.MixCfg.Is16Bits ? 16 : 8));

            _exportWriter.Write(DataHeader);

            _exportWriter.Write(_dumpSize);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            _exportStream?.Dispose();
            _exportStream = null;
        }
    }
}
