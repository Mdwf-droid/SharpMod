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
using SharpMod.DSP;
using System.Windows.Media.Imaging;

namespace SharpMod.SilverLight.UI
{
    public partial class VuMeter : UserControl
    {
        public int Fps { get; set; }

        private static int bands = 20;

        private float[] fftLevels = new float[bands];
        private float[] maxFFTLevels = new float[bands];

        private int[] samples;
        private float[] floatSamples;
        private float maxPeakValue = MIXERMAXSAMPLE;
        private float[] maxPeakLevelRampDownValue = new float[bands];        
        private int multiplier = (FFT_SAMPLE_SIZE >> 1) / bands;
        private float RampDownValue;
        private float maxPeakLevelRampDownDelay = 20;
        private int myHalfHeight;
        private int barWidth;

        public static int MIXERMAXSAMPLE = 0x7FFF;
        private static int FFT_SAMPLE_SIZE = 512;

        FFT fft = new FFT(FFT_SAMPLE_SIZE);

        private int anzSamples;
        private Color[] color;
        private Color[] SKcolor;
        private int SKMax;

        public VuStyle VuMeterStyle { get; set; }

        private bool processing;

        WriteableBitmap _wb;

        public VuMeter()
        {
            InitializeComponent();
            this.SizeChanged += new SizeChangedEventHandler(VuMeter_SizeChanged);
            this.Fps = 50;
            this.VuMeterStyle = VuStyle.SA;
            processing = false;


            _wb = new WriteableBitmap((int)this.Width, (int)this.Height);
            
            
        }

        void VuMeter_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.LayoutRoot.Width = e.NewSize.Width;
            this.LayoutRoot.Height = e.NewSize.Height;
            _wb = new WriteableBitmap((int)this.Width, (int)this.Height);
            Prepare();
        }

        public void Update()
        {
            if (processing)
                return;
           
            Draw();
        }

        private void Prepare()
        {
            if (LayoutRoot.Height > 0)
            {

               

                barWidth = _wb.PixelWidth/ bands;

                color = new Color[_wb.PixelHeight + 1];
                for (int i = 0; i <= _wb.PixelHeight; i++)
                {
                    int color1 = i * 255 / _wb.PixelHeight;
                    int color2 = 255 - color1;
                    color[i] = Color.FromArgb(255, (byte)color1, (byte)color2, 0);
                }
                SKMax = 768;
                SKcolor = new Color[SKMax];
                for (int i = 0; i < 256; i++)
                {
                    SKcolor[i] = Color.FromArgb(255, 0, 0, (byte)i);
                }
                for (int i = 256; i < 512; i++)
                {
                    SKcolor[i] = Color.FromArgb(255, (byte)(i - 256), 0, (byte)(511 - i));
                }
                for (int i = 512; i < 768; i++)
                {
                    SKcolor[i] = Color.FromArgb(255, 255, (byte)(i - 512), 0);
                }

                myHalfHeight = _wb.PixelHeight / 2;
            }
        }

        private void Draw()
        {
            if (LayoutRoot.Height > 0)
            {
                this.RampDownValue = 1.0F / (_wb.PixelHeight * ((float)/*FPS*/ Fps / 50F));
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

                if (VuMeterStyle == VuStyle.SA)
                    DrawSAMeter();
                else
                    drawWaveMeter();
            }
        }

        private void DrawSAMeter()
        {
            _wb.Clear(Colors.Black);
            for (int i = 0; i < bands; i++)
            {
                // New Peak Value
                if (fftLevels[i] > maxFFTLevels[i])
                {
                    maxFFTLevels[i] = fftLevels[i];
                    maxPeakLevelRampDownValue[i] = maxPeakLevelRampDownDelay;
                }
                // Let's Draw it...
                int barX = i * barWidth;
                int barX1 = barX + barWidth - 2;
                int barHeight = (int)(_wb.PixelHeight * fftLevels[i]);
                int maxBarHeight = (int)(_wb.PixelHeight * maxFFTLevels[i]);
                int c = (int)barHeight;
                for (int y = _wb.PixelHeight - barHeight; y < _wb.PixelHeight; y++)
                {
                   /* var line = new Line()
                    {
                        Stroke = new SolidColorBrush(color[c--]),
                        X1 = barX,
                        X2 = barX1,
                        Y1 = y,
                        Y2 = y
                    };
                    this.LayoutRoot.Children.Add(line);*/
                    _wb.DrawLine( barX, y, barX1, y, color[c--]);
                    //g.DrawLine(new Pen(color[c--]), barX, y, barX1, y);
                }
                if (maxBarHeight > barHeight)
                {
                    // g.setColor(color[maxBarHeight]);
                    //g.DrawLine(new Pen(color[maxBarHeight]), barX, pbMeter.Height - maxBarHeight, barX1, pbMeter.Height - maxBarHeight);
                    _wb.DrawLine(barX, _wb.PixelHeight - maxBarHeight, barX1, _wb.PixelHeight - maxBarHeight, color[maxBarHeight]);
                    /*var line = new Line()
                    {
                        Stroke = new SolidColorBrush(color[maxBarHeight]),
                        X1 = barX,
                        X2 = barX1,
                        Y1 = _wb.PixelHeight - maxBarHeight,
                        Y2 = _wb.PixelHeight - maxBarHeight
                    };
                    this.LayoutRoot.Children.Add(line);*/
                }
            }
            this.vuMeterImage.Source = _wb;
        }

        private void drawWaveMeter()
        {
            _wb.Clear(Colors.Black);
            /*var middleLine = new Line()
            {
                Stroke = new SolidColorBrush(Colors.Green),
                X1 = 0,
                X2 = this.LayoutRoot.Width,
                Y1 = myHalfHeight,
                Y2 = myHalfHeight
            };

            LayoutRoot.Children.Add(middleLine);*/
            _wb.DrawLine(0, myHalfHeight, _wb.PixelWidth, myHalfHeight, Colors.Green);

            if (samples == null) return;

            int add = (anzSamples / (int)LayoutRoot.Width) >> 1;
            if (add <= 0) add = 1;

            int xpOld = 0;
            int ypOld = myHalfHeight - (samples[0] * myHalfHeight / MIXERMAXSAMPLE);
            if (ypOld < 0) 
                ypOld = 0;
            else if (ypOld > LayoutRoot.Height) ypOld = _wb.PixelHeight;

            if (samples != null && anzSamples > 0)
            {
                //g.setColor(Color.WHITE);
                for (int i = add; i < anzSamples; i += add)
                {
                    int xp = (i * _wb.PixelWidth) / anzSamples;
                    if (xp < 0) xp = 0; else if (xp > LayoutRoot.Width) xp = _wb.PixelWidth;

                    int yp = myHalfHeight - (samples[i] * myHalfHeight / MIXERMAXSAMPLE);
                    if (yp < 0) yp = 0; else if (yp > LayoutRoot.Height) yp = _wb.PixelHeight;

                    _wb.DrawLine(xpOld, ypOld, xp, yp, Colors.White);
                   /* var line = new Line()
                    {
                        Stroke = new SolidColorBrush(Colors.White),
                        X1 = xpOld,
                        X2 = xp,
                        Y1 = ypOld,
                        Y2 = yp
                    };*/

                    vuMeterImage.Source = _wb;

                    xpOld = xp;
                    ypOld = yp;
                }
            }
        }

        public void Process(int[] samplesToProcess)
        {
            if (processing)
                return;

            processing = true;
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
            processing = false;

        }
    }

    public enum VuStyle
    {
        Wave,
        SA,
        SK
    }
}
