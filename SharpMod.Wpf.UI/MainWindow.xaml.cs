using Microsoft.Win32;
using SharpMod.DSP;
using SharpMod.Song;
using SharpMod.SoundRenderer;
using SharpMod.Wpf.UI.UserControls;
using System.Windows;
using System.Windows.Media;

namespace SharpMod.Wpf.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ModulePlayer? _player;
        private SongModule? myMod;


        public MainWindow()
        {
            InitializeComponent();

            CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);
            this.BpmSlide.Value = 1.0d;
        }





        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            VuMeterLeft.Update();
            VuMeterRight.Update();

        }

        void DspAudioProcessor_OnCurrentSampleChanged(int[] leftSample, int[] rightSample)
        {
            VuMeterLeft.Process(leftSample);
            VuMeterRight.Process(rightSample);
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (_player != null)
                _player.Start();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            if (_player != null)
                _player.Stop();
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();

            if (ofd.ShowDialog() == true)
            {
                if (_player != null)
                    _player.Stop();

                myMod = ModuleLoader.Instance.LoadModule(ofd.FileName);

                _player = new ModulePlayer(myMod);
                var drv = new NAudioWaveChannelDriver(NAudioWaveChannelDriver.Output.Wasapi);
                _player.RegisterRenderer(drv);
                _player.DspAudioProcessor = new AudioProcessor(1024, 50);
                _player.DspAudioProcessor.OnCurrentSampleChanged += new AudioProcessor.CurrentSampleChangedHandler(DspAudioProcessor_OnCurrentSampleChanged);
                _player.OnGetPlayerInfos += new GetPlayerInfosHandler(_player_OnGetPlayerInfos);
                _player.PlayerInstance.SpeedConstant = (float)this.BpmSlide.Value;
                LblTrackNfo0.Value = $"{myMod.SongName}";
                LblTrackNfo1.Value = $"{myMod.ChannelsCount:00}";
                LblTrackNfo2.Value = $"{myMod.InitialTempo:000}";
                LblTrackNfo3.Value = $"{myMod.ModType}";

            }

        }

        void _player_OnGetPlayerInfos(object sender, SharpModEventArgs sme)
        {
            Dispatcher.BeginInvoke(() =>
            {

                LblTrackPos.Value = String.Format("{0:000}/{1:000}", sme.PatternPosition, sme.CurrentPatternPositionsCount);
                LblPatPos.Value = $"{sme.PatternNumber:000}";

                LblBpm.Value = String.Format("{0:000}", _player?.PlayerInstance.MpBpm);
            });
        }

        private void VuMeterStyle_Checked(object sender, RoutedEventArgs e)
        {
            if (VuMeterStyle.IsChecked == true)
            {
                VuMeterStyle.Content = "Wave";
                VuMeterLeft.VuMeterStyle = VuStyle.Wave;
                VuMeterRight.VuMeterStyle = VuStyle.Wave;
            }
            else
            {
                VuMeterStyle.Content = "Spectrum";
                VuMeterLeft.VuMeterStyle = VuStyle.SA;
                VuMeterRight.VuMeterStyle = VuStyle.SA;

            }
        }


        private void BpmSlide_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_player != null)
            {
                _player.PlayerInstance.SpeedConstant = (float)e.NewValue;
            }
        }
    }
}