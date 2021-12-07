using System.Threading;
using System.IO;
using SharpMod.Player;

namespace SharpMod.SoundRenderer
{
    ///<summary>
    ///</summary>
    public class WaveExporter: IRenderer
    {
        private Stream _exportStream;
        private BinaryWriter _exportWriter;
        private const int BufferLength = 32768;
        private byte[] _buffer;
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

        #endregion

        ///<summary>
        ///</summary>
        public void LetsGo()
        {
            _buffer = new byte[BufferLength];
            int read;
            while(Player.IsPlaying && (read = Player.GetBytes(_buffer,BufferLength))>0)
            {
                _dumpSize += read;
                _exportWriter.Write(_buffer,0,read);                
                _buffer = new byte[BufferLength];
            }
        }

        private void WriteHeader()
        {
            _exportStream.Seek(0, SeekOrigin.Begin);
            _exportWriter.Write(new[]{'R','I','F','F'});//_mm_write_string("RIFF", wavout);
            _exportWriter.Write(_dumpSize+44);//_mm_write_I_ULONG(dumpsize + 44, wavout);
            _exportWriter.Write(new[] { 'W', 'A', 'V', 'E', 'f', 'm', 't', ' ' });//_mm_write_string("WAVEfmt ", wavout);
            _exportWriter.Write(16);//_mm_write_I_ULONG(16, wavout);	/* length of this RIFF block crap */
            _exportWriter.Write((short)1);//_mm_write_I_UWORD(1, wavout);	/* microsoft format type */
            _exportWriter.Write((short)(Player.MixCfg.Style == RenderingStyle.Mono ? 1 : 2));//_mm_write_I_UWORD((md_mode & DMODE_STEREO) ? 2 : 1, wavout);
            _exportWriter.Write(Player.MixCfg.Rate);//_mm_write_I_ULONG(md_mixfreq, wavout);
            _exportWriter.Write(Player.MixCfg.Rate * (Player.MixCfg.Style == RenderingStyle.Mono ? 1 : 2) * (Player.MixCfg.Is16Bits ? 2 : 1));
            //_mm_write_I_ULONG(md_mixfreq * ((md_mode & DMODE_STEREO) ? 2 : 1) *((md_mode & DMODE_16BITS) ? 2 : 1), wavout);
            /* block alignment (8/16 bit) */
            _exportWriter.Write((short)((Player.MixCfg.Style == RenderingStyle.Mono ? 1 : 2) * (Player.MixCfg.Is16Bits ? 2 : 1)));
            //_mm_write_I_UWORD(((md_mode & DMODE_16BITS) ? 2 : 1) *((md_mode & DMODE_STEREO) ? 2 : 1), wavout);
            _exportWriter.Write((short)(Player.MixCfg.Is16Bits ? 16 : 8));
            //_mm_write_I_UWORD((md_mode & DMODE_16BITS) ? 16 : 8, wavout);
            _exportWriter.Write(new[] { 'd', 'a', 't', 'a' });
            //_mm_write_string("data", wavout);
            _exportWriter.Write(_dumpSize);
            //_mm_write_I_ULONG(dumpsize, wavout);
        }
    }
}
