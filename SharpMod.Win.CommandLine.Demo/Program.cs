using SharpMod.Song;
using SharpMod.SoundRenderer;
using System;
using System.IO;
using System.Text;


namespace SharpMod.Win.CommandLine.Demo
{
    internal static class Program
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
                Console.WriteLine($"File {fi.FullName} not found");
            }

            myMod = ModuleLoader.Instance.LoadModule(fi.FullName);
            ModulePlayer p = new ModulePlayer(myMod);
            p.MixCfg.Rate = 44100;
            p.MixCfg.Is16Bits = true;
            p.MixCfg.Interpolate = true;
            p.MixCfg.NoiseReduction = true;
            var drv = new NAudioWaveChannelDriver(NAudioWaveChannelDriver.Output.WaveOut);
            //SharpMod.SoundRenderer.WaveExporter drv = new SharpMod.SoundRenderer.WaveExporter("test.wav");
            p.RegisterRenderer(drv);
            p.OnGetPlayerInfos += new GetPlayerInfosHandler(m_OnGetPlayerInfos);
            p.OnCurrentModulePlayEnd += new CurrentModulePlayEndHandler(OnCurrentModEnded);
            p.Start();

            Console.Read();
            p.Stop();
        }

        static void OnCurrentModEnded(object sender, EventArgs e)
        {
            throw new NotSupportedException();
        }

        static int lastp = -1;
        static void m_OnGetPlayerInfos(object sender, SharpMod.SharpModEventArgs e)
        {
            if (Console.WindowHeight != 71)
            {
                Console.CursorVisible = false;
                Console.WindowHeight = 65;
                Console.BufferWidth = 600;
            }



            for (int i = 0; i < myMod.Patterns[e.SongPosition].RowsCount; i++)
            {
                Console.SetCursorPosition(0, i);
                if (e.PatternPosition == i)
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
