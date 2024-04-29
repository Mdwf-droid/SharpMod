using NAudio.Wave;

namespace SharpMod.SoundRenderer
{
    class NAudioTrackerStream : NAudio.Wave.WaveStream
    {
        private readonly WaveFormat waveFormat;
        internal ModulePlayer Player { get; set; }

        public NAudioTrackerStream(ModulePlayer player)
        {
            Player = player;
            waveFormat = new WaveFormat(Player.MixCfg.Rate, Player.MixCfg.Is16Bits ? 16 : 8, (Player.MixCfg.Style == SharpMod.Player.RenderingStyle.Mono) ? 1 : 2);
        }

        public override long Position
        {
            get;
            set;

        }

        public override long Length
        {
            get { return 0; }
        }

        public override WaveFormat WaveFormat
        {
            get { return waveFormat; }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var readed = Player.GetBytes(buffer, count);
            Position += readed;
            return readed;
        }
    }

}
