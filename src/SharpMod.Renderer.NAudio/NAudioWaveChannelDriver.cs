using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace SharpMod.SoundRenderer
{
    public class NAudioWaveChannelDriver(NAudioWaveChannelDriver.Output output) : IRenderer
    {
        public enum Output
        {
            WaveOut,
            DirectSound,
            Wasapi,
            Asio
        }

        public NAudioTrackerStream? TrackerStream => _naudioTrackerStream;

        //private ChannelsMixer _virtch;
        public ModulePlayer Player { get; set; }
        private readonly Output _output = output;
        internal sbyte[] buf;

        NAudioTrackerStream _naudioTrackerStream;
        IWavePlayer waveOut;

        readonly WaveChannel32 _volumeStream;

        // NAudio setup variables
        /// <summary>
        /// Asio Driver name, needed to use Asio rendering
        /// </summary>
        public string AsioDriverName { get; set; }
        /// <summary>
        /// Wasapi audio client driver Mode, shared if false, exclusive if true
        /// </summary>
        public bool WasapiExclusiveMode { get; set; }

        /// <summary>
        /// Desired Latency, used for Wavout, DirectSound and Wasapi
        /// by default, value is 250ms
        /// </summary>
        public int Latency { get; set; } = 250;

        public void Init()
        {
            CreateWaveOut();

            _naudioTrackerStream = new NAudioTrackerStream(Player);
            waveOut.Init(_naudioTrackerStream);
            //return 1;
        }

        private void CreateWaveOut()
        {
            //CloseWaveOut();
           
            switch (_output)
            {
                case Output.WaveOut:
                    waveOut = new WaveOutEvent
                    {
                        // DesiredLatency = Latency
                        DesiredLatency = 100,
                        NumberOfBuffers =3
                    };
                    break;
                case Output.DirectSound:
                    waveOut = new DirectSoundOut(Latency);
                    
                    break;
                case Output.Wasapi:
                    waveOut = new WasapiOut(WasapiExclusiveMode?AudioClientShareMode.Exclusive:AudioClientShareMode.Shared,Latency);
                    break;
               /* case Output.Asio:
                    waveOut = new AsioOut(AsioDriverName);                    
                    break;*/
            }
           
        }

        private void CloseWaveOut()
        {
            if (waveOut != null)
            {
               // waveOut.Stop();
            }
            if (_naudioTrackerStream != null)
            {
                // this one really closes the file and ACM conversion
                //volumeStream.Close();
                //volumeStream = null;
                // this one does the metering stream
                _naudioTrackerStream.Close();
                _naudioTrackerStream = null;
            }
            waveOut?.Dispose();
        }


        #region IRenderer Members

        void IRenderer.PlayStart()
        {
            if (waveOut == null)
                CreateWaveOut();

           
            
            waveOut.Play();
        }

        void IRenderer.PlayStop()
        {
            CloseWaveOut();
        }

        #endregion
    }    
}
