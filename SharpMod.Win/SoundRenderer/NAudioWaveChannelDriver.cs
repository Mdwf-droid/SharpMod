using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;


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

        public ModulePlayer Player { get; set; }
        private readonly Output _output = output;        

        NAudioTrackerStream _naudioTrackerStream;
        IWavePlayer waveOut;

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
        }

        private void CreateWaveOut()
        {

            switch (_output)
            {
                case Output.WaveOut:

                    /* var callbackInfo = WaveCallbackInfo.FunctionCallback()
                     var outputDevice= new WaveOut(callbackInfo) {DesiredLatency = Latency}
                     waveOut = outputDevice                    */
                    throw new NotImplementedException();
                    
                case Output.DirectSound:
                    waveOut = new DirectSoundOut(Latency);
                    break;
                case Output.Wasapi:
                    waveOut = new WasapiOut(WasapiExclusiveMode ? AudioClientShareMode.Exclusive : AudioClientShareMode.Shared, Latency);
                    break;
                case Output.Asio:
                    waveOut = new AsioOut(AsioDriverName);
                    break;
            }

        }

        private void CloseWaveOut()
        {
            waveOut.PlaybackStopped += WaveOut_PlaybackStopped;


            waveOut.Stop();


        }

        private void WaveOut_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            CleanUp();
        }

        private void CleanUp()
        {
            _naudioTrackerStream?.Dispose();
            _naudioTrackerStream = null;

            waveOut?.Dispose();
            waveOut = null;
        }

        #region IRenderer Members

        void IRenderer.PlayStart()
        {
            if (waveOut == null)
                CreateWaveOut();

            _naudioTrackerStream = new NAudioTrackerStream(Player);
            waveOut.Init(_naudioTrackerStream);

            waveOut.Play();
        }

        void IRenderer.PlayStop()
        {
            CloseWaveOut();
        }

        public void Dispose()
        {          
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            _naudioTrackerStream?.Dispose();
            _naudioTrackerStream = null;
        }

        #endregion
    }
}
