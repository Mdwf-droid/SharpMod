using System;
using System.Text;
using System.Windows.Forms;
using AdvancedNetModTest.Properties;
using SharpMod.Song;
using SharpMod;
using SharpMod.DSP;
using SharpMod.Win.UI;
using SharpMod.SoundRenderer;
using SharpMod.UniTracker;
using System.Drawing;

namespace AdvancedNetModTest
{
    delegate void DoNextLineDelegate(SharpModEventArgs e);
    public partial class Form1 : Form
    {
        public SongModule MyMod { get; set; }
        public ModulePlayer Player { get; set; }

        public Form1()
        {
            InitializeComponent();

            checkBox1.Checked = true;
            radioButton2.Checked = true;

            vuMeterLeft.MeterStyle = VuStyle.SA;
            vuMeterRight.MeterStyle = VuStyle.SA;
        }


        void DspAudioProcessor_OnCurrentSampleChanged(int[] leftSample, int[] rightSample)
        {
            vuMeterLeft.Process(leftSample);
            vuMeterRight.Process(rightSample);
        }

        void player_OnGetPlayerInfos(object sender, SharpModEventArgs e)
        {
            if (InvokeRequired && !IsDisposed)
            {
                var method = new GetPlayerInfosHandler(UpdateUip);
                try
                {
                    Invoke(method, new[] { sender, e });
                }
                catch (Exception)
                {
                }
            }
            else
            {
                UpdateUip(sender, e);
            }

        }

        void UpdateUip(object sender, SharpModEventArgs sme)
        {
            tbPos.Text= String.Format("{0:000}/{1:000}", sme.PatternPosition, Player.CurrentModule.Patterns[sme.SongPosition].RowsCount);

            tbPatNo.Text = String.Format("{0:000}", sme.PatternNumber);

            foreach(ListViewItem ob in listView1.Items)
                ob.BackColor = Color.White;
            
            listView1.Items[sme.SongPosition].BackColor = Color.AliceBlue;
            
        }

        private void Button2Click(object sender, EventArgs e)
        {
            if (Player != null)
            {
                if (!Player.IsPlaying)
                {
                    Player.Start();
                    btnPlayStop.Text = Resources.StopText;
                }
                else
                {
                    Player.Stop();
                    btnPlayStop.Text = Resources.PlayText;
                }
            }
        }

        private void Form1FormClosing(object sender, FormClosingEventArgs e)
        {
            if(Player != null)
                Player.Stop();
            base.OnClosing(e);
        }

        private void BtnOpenClick(object sender, EventArgs e)
        {
            if (ofdModFile.ShowDialog() == DialogResult.OK)
            {
                if (Player != null)
                {
                    Player.Stop();
                    btnPlayStop.Text = Resources.PlayText;
                }

                MyMod = ModuleLoader.Instance.LoadModule(ofdModFile.OpenFile());

              /*  MyMod.Patterns[1].Tracks[1].Cells[63].Effect = 0xB;
                MyMod.Patterns[1].Tracks[1].Cells[63].EffectData = 0;
                MyMod.Patterns[1].Tracks[1].ValidateChanges();*/


                Player = new ModulePlayer(MyMod)
                             {
                                 MixCfg =
                                     {
                                         Rate = 48000,
                                         Is16Bits = checkBox1.Checked,
                                         Interpolate = checkBox3.Checked
                                     }
                             };

                if (radioButton1.Checked)
                {
                    Player.MixCfg.Style = SharpMod.Player.RenderingStyle.Mono;
                }
                else if (radioButton2.Checked)
                {
                    Player.MixCfg.Style = SharpMod.Player.RenderingStyle.Stereo;
                }
                else if (radioButton3.Checked)
                {
                    Player.MixCfg.Style = SharpMod.Player.RenderingStyle.Surround;
                }

                var drv = new NAudioWaveChannelDriver(NAudioWaveChannelDriver.Output.DirectSound){Latency=125};
                Player.RegisterRenderer(drv);                
                Player.DspAudioProcessor = new AudioProcessor(1024, 50);
                Player.DspAudioProcessor.OnCurrentSampleChanged += DspAudioProcessor_OnCurrentSampleChanged;
                Player.OnGetPlayerInfos += player_OnGetPlayerInfos;
                var sb = new StringBuilder();

                sb.AppendLine(String.Format("Mod Name: {0}", MyMod.SongName));
                sb.AppendLine(String.Format("Channels: {0:00}", MyMod.ChannelsCount));
                sb.AppendLine(String.Format("Base BPM: {0:000}", MyMod.InitialTempo));
                sb.AppendLine(String.Format("Mod Type: {0}", MyMod.ModType));

                tbModNfo.Text = sb.ToString();

                listView1.Items.Clear();
                int i = 0;
                foreach(var val in MyMod.Positions)
                    listView1.Items.Add(new ListViewItem(string.Format("{0:00}:{1:000}", i++, val)) { Tag = val });
            }
        }

        private void chkLoop_CheckedChanged(object sender, EventArgs e)
        {
            Player.PlayerInstance.mp_loop = chkLoop.Checked;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
           //
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count > 0)
            Player.PlayerInstance.mp_sngpos =(short)( (int)listView1.SelectedIndices[0] );
        }
    }
}
