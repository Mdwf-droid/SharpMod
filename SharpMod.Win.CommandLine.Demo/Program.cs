using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpMod.UniTracker;
using SharpMod.Song;
using SharpMod;
using System.IO;


namespace SharpMod.Win.CommandLine.Demo
{
    class Program
    {  
        static SongModule myMod = null;

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Modfile full path needed as first arg");
                return;
            }

            FileInfo fi = new FileInfo(args[0]);
            if (!fi.Exists)
            {
                Console.WriteLine( String.Format("File {0} not found"),fi.FullName);
            }

            myMod = ModuleLoader.Instance.LoadModule(fi.FullName);
            ModulePlayer p = new ModulePlayer(myMod);
            p.MixCfg.Rate = 44100;
            p.MixCfg.Is16Bits = true;
            p.MixCfg.Interpolate = true;
            p.MixCfg.NoiseReduction = true;
            SharpMod.SoundRenderer.NAudioWaveChannelDriver drv = new SharpMod.SoundRenderer.NAudioWaveChannelDriver(SharpMod.SoundRenderer.NAudioWaveChannelDriver.Output.WaveOut);
            //SharpMod.SoundRenderer.WaveExporter drv = new SharpMod.SoundRenderer.WaveExporter("test.wav");
            p.RegisterRenderer(drv);
            p.OnGetPlayerInfos +=new GetPlayerInfosHandler(m_OnGetPlayerInfos);
            p.OnCurrentModulePlayEnd += new CurrentModulePlayEndHandler(m_OnCurrentModEnded);
            p.Start();

            Console.Read();
            p.Stop();
        }

        static void m_OnCurrentModEnded(object sender, EventArgs e)
        { }

        static int ctr = 0;
        static int lastp = -1;
        static void m_OnGetPlayerInfos(object sender, SharpMod.SharpModEventArgs e)
        {         
            ctr = 0;

            if (Console.WindowHeight != 71)
            {
                Console.CursorVisible = false;
                Console.WindowHeight = 65;
                Console.BufferWidth = 600;
            }



            for (int i = 0; i < myMod.Patterns[e.SongPosition].RowsCount; i++)
            {
                Console.SetCursorPosition(0, i);
                if(e.PatternPosition == i)
                    Console.Write(">");
                else
                    Console.Write(" ");                
            }
           

            if (lastp != e.SongPosition)
            {
                StringBuilder sb = new StringBuilder();
                Console.SetCursorPosition(0, 0);
                lastp = e.SongPosition;
                for (int i = 0; i < myMod.Patterns[e.SongPosition].RowsCount; i++)
                {
                    for (int j = 0; j < myMod.Patterns[e.SongPosition].Tracks.Count; j++)
                    {
                        sb.Append(" ");
                        sb.Append(myMod.Patterns[e.SongPosition].Tracks[j].Cells[i].ToString());
                        if (j < myMod.Patterns[e.SongPosition].Tracks.Count - 1)
                            sb.Append("\t");
                        else
                            sb.Append("\r\n");
                    }
                }
                Console.Write(sb.ToString());
            }

        }

    }
}
