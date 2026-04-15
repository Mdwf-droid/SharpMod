using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Wave;
using SharpMod.Mixer;

namespace SharpMod.SoundRenderer
{
    public class NAudioTrackerStream : NAudio.Wave.WaveStream
    {        
        private readonly WaveFormat waveFormat;
        internal ModulePlayer Player { get; set; }

        public event Action<byte[], int>? OnSamplesGenerated;

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
            int readed;
            
            readed = Player.GetBytes(buffer, count);

            OnSamplesGenerated?.Invoke(buffer, readed);

            return readed;
        }
    }

}
