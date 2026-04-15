using System;
using System.Linq;
using SharpMod.Song;
using SharpMod.Player;
using SharpMod.Mixer;
using SharpMod.UniTracker;
using SharpMod.Exceptions;
using SharpMod.DSP;

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
    public class ModulePlayer
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
        /*private DMode _mode;
        public DMode Mode
        {
            get { return _mode; }
            set
            {
                _mode = value;
                
                ChannelsMixer._dMode = _mode;
            }
        }*/
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

        // Buffers pré-alloués pour éviter les allocations
        private int[] _vuVolumes = new int[32];
        private int[] _vuPeaks = new int[32];

        private readonly UniTrk _uniTrk;
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
            _uniTrk = new UniTrk();
            _uniTrk.UniInit();
            CurrentModule = module;
            WaveTableInstance = new WaveTable();
            PlayerInstance = new SharpModPlayer(_uniTrk);
            MixCfg = new MixConfig { Is16Bits = true, Style = RenderingStyle.Stereo, Rate = 48000 };
            ChannelsMixer = new ChannelsMixer(MixCfg /* DMode.DMODE_16BITS | DMode.DMODE_STEREO*/)
            {
                //this.ChannelsMixer.MixFreq = 48000;
                ChannelsCount = module.ChannelsCount
            };
            ChannelsMixer.OnTickHandler += PlayerInstance.MP_HandleTick;
            ChannelsMixer.OnBPMRequest += delegate { return PlayerInstance.mp_bpm; };
            ChannelsMixer.WaveTable = WaveTableInstance;

            ChannelsMixer.InitScopeBuffers();

            PlayerInstance.MP_Init(CurrentModule);
            PlayerInstance._mixer = ChannelsMixer;
            PlayerInstance.SpeedConstant = 1.0f;
            PlayerInstance.mp_volume = 100;
            PlayerInstance.mp_bpm = 125;
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

            var sme = new SharpModEventArgs
                          {
                              PatternNumber = PlayerInstance.CurrentUniMod.Positions[PlayerInstance.mp_sngpos],
                              SongPosition = PlayerInstance.mp_sngpos,
                              PatternPosition = PlayerInstance.mp_patpos
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
                ChannelsMixer.VC_PlayStop();
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
        public byte[] CurrentBytesWindow;

        private sbyte[]? _sbyteBuffer;
        ///<summary>
        ///</summary>
        ///<param name="buffer"></param>
        ///<param name="count"></param>
        ///<returns></returns>
        public int GetBytes(byte[] buffer, int count)
        {
            if (IsPlaying)
            {
                // ══ OPTIM : Éviter le cast (sbyte[])(Array) ══
                // Utiliser un buffer sbyte[] pré-alloué pour éviter l'allocation à chaque appel
                if (_sbyteBuffer == null || _sbyteBuffer.Length < count)
                    _sbyteBuffer = new sbyte[count];

                var c = ChannelsMixer.VC_WriteBytes(_sbyteBuffer, count);

                // Copie sbyte[] → byte[] sans allocation (même layout mémoire)
                Buffer.BlockCopy(_sbyteBuffer, 0, buffer, 0, c);

                CurrentBytesWindow = buffer;
                return c;
            }
            return 0;
        }

        /// <summary>
        /// Récupérer les niveaux VU de tous les canaux.
        /// </summary>
        public void GetChannelLevels(out int[] volumes, out int[] peaks, out int count)
        {
            if (ChannelsMixer != null)
            {
                ChannelsMixer.GetAllChannelLevels(_vuVolumes, _vuPeaks, out count);
                volumes = _vuVolumes;
                peaks = _vuPeaks;
            }
            else
            {
                volumes = _vuVolumes;
                peaks = _vuPeaks;
                count = 0;
            }
        }

        /// <summary>
        /// Récupérer les données d'oscilloscope pour un canal.
        /// </summary>
        public sbyte[] GetScopeData(int channel)
        {
            return ChannelsMixer?.GetScopeData(channel) ?? Array.Empty<sbyte>();
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
    }
}
