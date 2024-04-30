using SharpMod.DSP;
using SharpMod.Exceptions;
using SharpMod.Mixer;
using SharpMod.Player;
using SharpMod.Song;
using SharpMod.UniTracker;
using System;
using System.Linq;

namespace SharpMod
{
    ///<summary>
    ///</summary>
    ///<param name="sender"></param>
    ///<param name="sme"></param>
    public delegate void GetPlayerInfosHandler(object sender, SharpModEventArgs sme);

    public delegate void CurrentModulePlayEndHandler(object sender, EventArgs e);

    ///<summary>
    ///</summary>
    public class ModulePlayer : IDisposable
    {
        ///<summary>
        ///</summary>
        public event GetPlayerInfosHandler OnGetPlayerInfos;

        /// <summary>
        /// Current Module have finish to play
        /// </summary>
        public event CurrentModulePlayEndHandler OnCurrentModulePlayEnd;

        ///<summary>
        ///</summary>
        public SongModule CurrentModule { get; set; }
        ///<summary>
        ///</summary>
        public SharpModPlayer PlayerInstance { get; set; }
        ///<summary>
        ///</summary>
        public ChannelsMixer ChannelsMixer { get; set; }
        ///<summary>
        ///</summary>
        public WaveTable WaveTableInstance { get; set; }
        ///<summary>
        ///</summary>
        public IRenderer SoundRenderer { get; set; }

        private AudioProcessor _dspAudioProcessor;
        ///<summary>
        ///</summary>
        public AudioProcessor DspAudioProcessor
        {
            get { return _dspAudioProcessor; }
            set
            {
                _dspAudioProcessor = value;
                if (_dspAudioProcessor != null)
                {
                    _dspAudioProcessor.initializeProcessor(ChannelsMixer);
                    ChannelsMixer._audioProcessor = _dspAudioProcessor;

                }
            }
        }

        private MixConfig _mixCfg;
        ///<summary>
        ///</summary>
        public MixConfig MixCfg
        {
            get { return _mixCfg; }
            set
            {
                _mixCfg = value;
                if (ChannelsMixer != null)
                    ChannelsMixer.MixCfg = _mixCfg;
            }
        }

        ///<summary>
        ///</summary>
        public bool IsPlaying
        {
            get;
            private set;
        }

        ///<summary>
        ///</summary>
        ///<param name="module"></param>
        public ModulePlayer(SongModule module)
        {
            var _uniTrk = new UniTrk();
            _uniTrk.UniInit();
            CurrentModule = module;
            WaveTableInstance = new WaveTable();
            PlayerInstance = new SharpModPlayer(_uniTrk);
            MixCfg = new MixConfig { Is16Bits = true, Style = RenderingStyle.Stereo, Rate = 48000 };
            ChannelsMixer = new ChannelsMixer(MixCfg)
            {
                ChannelsCount = module.ChannelsCount
            };
            ChannelsMixer.OnTickHandler += PlayerInstance.MP_HandleTick;
            ChannelsMixer.OnBPMRequest += delegate { return PlayerInstance.MpBpm; };
            ChannelsMixer.WaveTable = WaveTableInstance;

            PlayerInstance.MP_Init(CurrentModule);
            PlayerInstance.Mixer = ChannelsMixer;
            PlayerInstance.SpeedConstant = 1.0f;
            PlayerInstance.MpVolume = 100;
            PlayerInstance.MpBpm = 125;
            PlayerInstance.OnUpdateUI += PlayerInstance_OnUpdateUI;
            PlayerInstance.OnCurrentModEnd += new CurrentModEndHandler(PlayerInstance_OnCurrentModEnd);

            InitWaveTable();
        }

        void PlayerInstance_OnCurrentModEnd()
        {
            OnCurrentModulePlayEnd?.Invoke(this, new EventArgs());
        }

        void PlayerInstance_OnUpdateUI()
        {
            if (OnGetPlayerInfos == null)
                return;

            var currentPatternNumber = PlayerInstance.CurrentUniMod.Positions[PlayerInstance.MpSngPos];
            var sme = new SharpModEventArgs
            {
                PatternNumber = currentPatternNumber,
                SongPosition = PlayerInstance.MpSngPos,
                PatternPosition = PlayerInstance.MpPatpos,
                CurrentPatternPositionsCount = PlayerInstance.CurrentUniMod.Patterns[currentPatternNumber].RowsCount
            };

            OnGetPlayerInfos?.Invoke(this, sme);
        }

        /// <summary>
        /// Initialize the Wave Table with samples in the module
        /// </summary>
        private void InitWaveTable()
        {
            foreach (var smp in
                CurrentModule.Instruments.SelectMany(ins => ins.Samples.Where(smp => smp.SampleBytes != null)))
            {
                WaveTableInstance.AddSample(smp.SampleBytes, smp.Handle);
            }
        }

        ///<summary>
        /// Start playing the loaded song module
        ///</summary>
        ///<exception cref="SharpModException"></exception>
        public void Start()
        {
            if (SoundRenderer == null)
                throw new SharpModException("No renderer");

            if (!IsPlaying)
            {
                IsPlaying = true;
                ChannelsMixer.VC_PlayStart();
                SoundRenderer.PlayStart();
            }

        }

        ///<summary>
        /// Stop the currently playing song module
        ///</summary>
        ///<exception cref="SharpModException"></exception>
        public void Stop()
        {
            if (SoundRenderer == null)
                throw new SharpModException("No renderer");

            if (IsPlaying)
            {
                IsPlaying = false;
                Pause();

                SoundRenderer.PlayStop();
            }
        }

        ///<summary>
        /// Pause the currently playing song module
        ///</summary>
        public void Pause()
        {
            if (IsPlaying)
                IsPlaying = false;
        }

        ///<summary>
        ///</summary>
        public byte[] CurrentBytesWindow { get; private set; }
        ///<summary>
        ///</summary>
        ///<param name="buffer"></param>
        ///<param name="count"></param>
        ///<returns></returns>
        public int GetBytes(byte[] buffer, int count)
        {
            if (IsPlaying)
            {
                var c = ChannelsMixer.VC_WriteBytes((sbyte[])(Array)buffer, count);
                CurrentBytesWindow = buffer;
                return c;
            }
            return 0;
        }

        ///<summary>
        /// Registers and initialises the Sound Renderer
        ///</summary>
        ///<param name="renderer"></param>
        public void RegisterRenderer(IRenderer renderer)
        {
            SoundRenderer = renderer;
            SoundRenderer.Player = this;
            SoundRenderer.Init();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            SoundRenderer?.Dispose();
            SoundRenderer = null;
            IsDisposed = true;
        }

        public bool IsDisposed { get; private set; }
    }
}
