using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using SharpMod;
using SharpMod.Song;
using System.IO;
using SharpMod.DSP;
using SharpMod.SilverLight.UI;

namespace SharpMod.SilverLight.Demo
{
    public partial class MainPage : UserControl
    {
        private SharpMod.ModulePlayer _player;

        SongModule myMod;

        public MainPage()
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
            System.Windows.Controls.OpenFileDialog ofd = new OpenFileDialog();

            if (ofd.ShowDialog() == true)
            {
                if (_player != null)
                    _player.Stop();

                myMod = ModuleLoader.Instance.LoadModule(ofd.File.OpenRead());

                _player = new ModulePlayer(myMod);
                SharpMod.SoundRenderer.SilverlightDriver drv = new SharpMod.SoundRenderer.SilverlightDriver(MyMediaElement);
                _player.RegisterRenderer(drv);
                _player.DspAudioProcessor = new AudioProcessor(1024, 50);
                _player.DspAudioProcessor.OnCurrentSampleChanged += new AudioProcessor.CurrentSampleChangedHandler(DspAudioProcessor_OnCurrentSampleChanged);
                _player.OnGetPlayerInfos += new GetPlayerInfosHandler(_player_OnGetPlayerInfos);
                _player.PlayerInstance.SpeedConstant = (float)this.BpmSlide.Value;
                LblTrackNfo0.Value = String.Format("Mod Name: {0}", myMod.SongName);
                LblTrackNfo1.Value = String.Format("Channels: {0:00}", myMod.ChannelsCount);
                LblTrackNfo2.Value = String.Format("Base BPM: {0:000}", myMod.InitialTempo);
                LblTrackNfo3.Value = String.Format("Mod Type: {0}", myMod.ModType);

            }

        }

        void _player_OnGetPlayerInfos(object sender, SharpModEventArgs sme)
        {
            Dispatcher.BeginInvoke(() =>
            {
                LblTrackPos.Value = String.Format("{0:00}/{1:00}", sme.PatternPosition, _player.CurrentModule.Patterns[sme.SongPosition].RowsCount);

                LblBpm.Value = String.Format("{0:000}", _player.PlayerInstance.mp_bpm);
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
