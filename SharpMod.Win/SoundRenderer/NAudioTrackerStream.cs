using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Wave;
using SharpMod.Mixer;

namespace SharpMod.SoundRenderer
{
    class NAudioTrackerStream : NAudio.Wave.WaveStream
    {        
        private WaveFormat waveFormat;
        internal ModulePlayer Player { get; set; }

        public NAudioTrackerStream(ModulePlayer player)
        {
            Player = player;
            waveFormat = new WaveFormat(Player.MixCfg.Rate, Player.MixCfg.Is16Bits?16:8,(Player.MixCfg.Style == SharpMod.Player.RenderingStyle.Mono)?1:2);
        }

        public override long Position
        {
            get { return 0; }
            set { ;}
            /*{
                return _mixer.idxlpos;
            }
            set;
            {
                _mixer.idxlpos = (int)value;
            }*/
        }

        public override long Length
        {
            get { return 0; }// { return _mixer.idxsize; }
        }

        public override WaveFormat WaveFormat
        {
            get { return waveFormat; }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int readed = 0;
            //byte[] tmpBuffer = new sbyte[count];
            readed = Player.GetBytes(buffer, count);
            //Buffer.BlockCopy(tmpBuffer, 0, buffer, 0, readed);
            return readed;
        }
    }

}
