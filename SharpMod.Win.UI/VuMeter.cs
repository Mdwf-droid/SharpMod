using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SharpMod.DSP;

namespace SharpMod.Win.UI
{
    public partial class VuMeter : UserControl
    {

        private float[] fftLevels = new float[bands];
        private float[] maxFFTLevels = new float[bands];

        private int[] samples;
        private float[] floatSamples;
        private float maxPeakValue = MIXERMAXSAMPLE;
        private float[] maxPeakLevelRampDownValue = new float[bands];
        private static int bands = 35;
        private int multiplier = (FFT_SAMPLE_SIZE >> 1) / bands;
        private float RampDownValue;
        private float maxPeakLevelRampDownDelay = 20;
        private int myHalfHeight;
        private int barWidth;

        public static int MIXERMAXSAMPLE = 0x7FFF;
        private static int FFT_SAMPLE_SIZE = 512;

        FFT fft = new FFT(FFT_SAMPLE_SIZE);

        private int anzSamples;
        private bool switched;
        private Color[] color;
        private Color[] SKcolor;
        private int SKMax;

        public int Fps { get; set; }
        public VuStyle MeterStyle { get; set; }

        public VuMeter()
        {
            Fps = 25;
            InitializeComponent();
            this.timerRefresh.Interval = 1000 / Fps;
            this.timerRefresh.Start();
        }


        public void Process(int[] samplesToProcess)
        {
            if (samplesToProcess != null)
            {
                anzSamples = samplesToProcess.Length;
                if (samples == null || samples.Length != anzSamples)
                {
                    samples = new int[anzSamples];
                    floatSamples = new float[anzSamples];
                }
                Array.Copy(samplesToProcess, 0, samples, 0, anzSamples);
                for (int i = 0; i < anzSamples; i++)
                {
                    floatSamples[i] = ((float)samplesToProcess[i]) / (float)maxPeakValue;
                }
                float[] resultFFTSamples = fft.calculate(floatSamples);

                int bd = 0;
                for (int a = 0; bd < bands; bd++)
                {
                    a += multiplier;
                    float wFs = resultFFTSamples[a];

                    for (int b = 1; b < multiplier; b++) wFs += resultFFTSamples[a + b];
                    wFs *= (float)Math.Log(bd + 2);

                    if (wFs > 1.0F) wFs = 1.0F;
                    if (wFs > fftLevels[bd]) fftLevels[bd] = wFs;
                }
            }
            else
            {
                samples = null;
                floatSamples = null;
            }

           
        }

        private void DrawMeter()
        {
            this.RampDownValue = 1.0F / ((float)pbMeter.Height * ((float)/*FPS*/ Fps / 50F));
            this.maxPeakLevelRampDownDelay = this.RampDownValue / Fps;

            for (int i = 0; i < bands; i++)
            {
                fftLevels[i] -= RampDownValue;
                if (fftLevels[i] < 0.0F) fftLevels[i] = 0.0F;

                maxFFTLevels[i] -= maxPeakLevelRampDownValue[i];
                if (maxFFTLevels[i] < 0.0F)
                    maxFFTLevels[i] = 0.0F;
                else
                    maxPeakLevelRampDownValue[i] += maxPeakLevelRampDownDelay;
            }

            barWidth = pbMeter.Width / bands;

            color = new Color[pbMeter.Height + 1];
            for (int i = 0; i <= pbMeter.Height; i++)
            {
                int color1 = i * 255 / pbMeter.Height;
                int color2 = 255 - color1;
                color[i] = Color.FromArgb(color1, color2, 0);
            }
            SKMax = 768;
            SKcolor = new Color[SKMax];
            for (int i = 0; i < 256; i++)
            {
                SKcolor[i] = Color.FromArgb(0, 0, i);
            }
            for (int i = 256; i < 512; i++)
            {
                SKcolor[i] = Color.FromArgb(i - 256, 0, 511 - i);
            }
            for (int i = 512; i < 768; i++)
            {
                SKcolor[i] = Color.FromArgb(255, i - 512, 0);
            }

            myHalfHeight = pbMeter.Height/2;
            if (MeterStyle == VuStyle.SA)
                drawSAMeter();
            if (MeterStyle == VuStyle.Wave)
                drawWaveMeter();
            if (MeterStyle == VuStyle.SK)
                drawSKMeter();

        }

        private void drawWaveMeter()
        {
            Bitmap canvas = new Bitmap(pbMeter.Width, pbMeter.Height);
            Graphics g = Graphics.FromImage(canvas);
            Pen pen = new System.Drawing.Pen(Color.WhiteSmoke);

            g.Clear(Color.Black);           
            
            g.DrawLine(Pens.Green, 0, myHalfHeight, pbMeter.Width, myHalfHeight);

            if (samples == null) return;

            int add = (anzSamples / pbMeter.Width) >> 1;
            if (add <= 0) add = 1;

            int xpOld = 0;
            int ypOld = myHalfHeight - (samples[0] * myHalfHeight / MIXERMAXSAMPLE);
            if (ypOld < 0) ypOld = 0; else if (ypOld > pbMeter.Height) ypOld = pbMeter.Height;

            if (samples != null && anzSamples > 0)
            {
                //g.setColor(Color.WHITE);
                for (int i = add; i < anzSamples; i += add)
                {
                    int xp = (i * pbMeter.Width) / anzSamples;
                    if (xp < 0) xp = 0; else if (xp > pbMeter.Width) xp = pbMeter.Width;

                    int yp = myHalfHeight - (samples[i] * myHalfHeight / MIXERMAXSAMPLE);
                    if (yp < 0) yp = 0; else if (yp > pbMeter.Height) yp = pbMeter.Height;

                    g.DrawLine(Pens.White, xpOld, ypOld, xp, yp);
                    xpOld = xp;
                    ypOld = yp;
                }
            }

            pbMeter.Image = canvas;
            g.Dispose();
        }


        private void drawSAMeter()//, int newTop, int newLeft, int newWidth, int newHeight)
        {
           
            /*g.setColor(Color.BLACK);
            g.fillRect(0, 0, newWidth, newHeight);*/
            Bitmap canvas = new Bitmap(pbMeter.Width, pbMeter.Height);
            Graphics g = Graphics.FromImage(canvas);
            Pen pen = new System.Drawing.Pen(Color.WhiteSmoke);

            g.Clear(Color.Black);
            for (int i = 0; i < bands; i++)
            {
                // New Peak Value
                if (fftLevels[i] > maxFFTLevels[i])
                {
                    maxFFTLevels[i] = fftLevels[i];
                    maxPeakLevelRampDownValue[i] = maxPeakLevelRampDownDelay;
                }
                // Let's Draw it...
                int barX = i *  barWidth;
                int barX1 = barX + barWidth - 2;
                int barHeight = (int)((float)pbMeter.Height * fftLevels[i]);
                int maxBarHeight = (int)((float)pbMeter.Height * maxFFTLevels[i]);
                int c = barHeight;
                for (int y = pbMeter.Height - barHeight; y < pbMeter.Height; y++)
                {
                    //g.setColor(color[c--]);
                    g.DrawLine(new Pen(color[c--]), barX, y, barX1, y);
                }
                if (maxBarHeight > barHeight)
                {
                    // g.setColor(color[maxBarHeight]);
                    g.DrawLine(new Pen(color[maxBarHeight]), barX, pbMeter.Height - maxBarHeight, barX1, pbMeter.Height - maxBarHeight);
                }               
            }
            pbMeter.Image = canvas;
            g.Dispose();
        }

        private void drawSKMeter()
        {
            Bitmap canvas = new Bitmap(pbMeter.Width, pbMeter.Height);
            Graphics g = Graphics.FromImage(canvas);
            switched = !switched;
            if (switched)
            {
                /*g.setColor(Color.BLACK);
                g.fillRect(0, 0, newWidth, newHeight);*/
                //g.Clear(Color.Black);
                
            }
            if(pbMeter.Image != null)
                g.DrawImage(pbMeter.Image, 1, 0, pbMeter.Width - 1, pbMeter.Height);
            //g.copyArea(0, 0, newWidth - 1, newHeight, 1, 0);
            int max = bands - 1;
            for (int i = 0; i <= max; i++)
            {
                int bary = (pbMeter.Height * (max - i)) / bands;
                //g.setColor(SKcolor[(int)(((float)SKMax) * fftLevels[i])]);
                g.DrawLine(new Pen(SKcolor[(int)(((float)SKMax) * fftLevels[i])]), 0, bary, 0, bary + 2);
            }

            pbMeter.Image = canvas;
            g.Dispose();
        }

        private void timerRefresh_Tick(object sender, EventArgs e)
        {
            DrawMeter();
        }
    }

    public enum VuStyle
    {
        Wave,
        SA,
        SK
    }
}
