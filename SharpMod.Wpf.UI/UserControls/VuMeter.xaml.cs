using SharpMod.DSP;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;


namespace SharpMod.Wpf.UI.UserControls
{
    public partial class VuMeter : UserControl
    {
        public int Fps { get; set; }

        private static int bands = 20;

        private float[] fftLevels = new float[bands];
        private float[] maxFFTLevels = new float[bands];

        private int[]? samples;
        private float[]? floatSamples;
        private float maxPeakValue = MIXERMAXSAMPLE;
        private float[] maxPeakLevelRampDownValue = new float[bands];
        private int multiplier = (FFT_SAMPLE_SIZE >> 1) / bands;
        private float RampDownValue;
        private float maxPeakLevelRampDownDelay = 20;
        private int myHalfHeight;
        private int barWidth;

        private static int MIXERMAXSAMPLE = 0x7FFF;
        private static int FFT_SAMPLE_SIZE = 512;

        Fft fft = new Fft(FFT_SAMPLE_SIZE);

        private int anzSamples;
        private Color[]? color;
        private Color[]? SKcolor;
        private int SKMax;

        public VuStyle VuMeterStyle { get; set; }

        private bool processing;

        public VuMeter()
        {
            InitializeComponent();
            this.SizeChanged += new SizeChangedEventHandler(VuMeter_SizeChanged);
            this.Fps = 50;
            this.VuMeterStyle = VuStyle.SA;
            processing = false;
        }

        void VuMeter_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.LayoutRoot.Width = e.NewSize.Width;
            this.LayoutRoot.Height = e.NewSize.Height;

            Prepare();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            Update();
            base.OnRender(drawingContext);
        }

        public void Update()
        {
            if (processing)
                return;

            Draw();
        }

        private void Prepare()
        {
            if (LayoutRoot.ActualHeight > 0)
            {
                barWidth = (int)LayoutRoot.ActualWidth / bands;

                color = new Color[(int)LayoutRoot.ActualHeight + 1];
                for (int i = 0; i <= (int)LayoutRoot.ActualHeight; i++)
                {
                    int color1 = i * 255 / (int)LayoutRoot.ActualHeight;
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

                myHalfHeight = (int)LayoutRoot.ActualHeight / 2;
            }
        }

        private void Draw()
        {
            if (LayoutRoot.ActualHeight > 0)
            {
                this.RampDownValue = 1.0F / ((int)LayoutRoot.ActualHeight * ((float)/*FPS*/ Fps / 50F));
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
            if (color == null)
                return;

            this.LayoutRoot.Children.Clear();
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
                int barHeight = (int)((int)LayoutRoot.ActualHeight * fftLevels[i]);
                int maxBarHeight = (int)((int)LayoutRoot.ActualHeight * maxFFTLevels[i]);

                var brush = new LinearGradientBrush(
                [
                    new GradientStop(color[barHeight], 0),
                    //new GradientStop(Colors.Yellow,0.5),
                    new GradientStop(color[0], 1),

                ], 90);
                var rect2 = new Rectangle()
                {

                    Stroke = brush,
                    Fill = brush,
                    Width = barX1 - barX,
                    Height = barHeight,

                };

                Canvas.SetLeft(rect2, barX);
                Canvas.SetBottom(rect2, 0);
                this.LayoutRoot.Children.Add(rect2);

                if (maxBarHeight > barHeight)
                {

                    var line = new Line()
                    {
                        Stroke = new SolidColorBrush(color[maxBarHeight]),
                        X1 = barX,
                        X2 = barX1,
                        Y1 = (int)LayoutRoot.ActualHeight - maxBarHeight,
                        Y2 = (int)LayoutRoot.ActualHeight - maxBarHeight
                    };
                    this.LayoutRoot.Children.Add(line);
                }
            }

        }

        private void drawWaveMeter()
        {
            this.LayoutRoot.Children.Clear();

            var middleLine = new Line()
            {
                Stroke = new SolidColorBrush(Colors.Green),
                X1 = 0,
                X2 = this.LayoutRoot.ActualWidth,
                Y1 = myHalfHeight,
                Y2 = myHalfHeight
            };

            LayoutRoot.Children.Add(middleLine);

            if (samples == null) return;

            int add = (anzSamples / (int)LayoutRoot.Width) >> 1;
            if (add <= 0) add = 1;

            int xpOld = 0;
            int ypOld = myHalfHeight - (samples[0] * myHalfHeight / MIXERMAXSAMPLE);
            if (ypOld < 0)
                ypOld = 0;
            else if (ypOld > LayoutRoot.Height) ypOld = (int)LayoutRoot.Height;

            if (samples != null && anzSamples > 0)
            {
                for (int i = add; i < anzSamples; i += add)
                {
                    int xp = (i * (int)LayoutRoot.Width) / anzSamples;
                    if (xp < 0) xp = 0; else if (xp > LayoutRoot.Width) xp = (int)LayoutRoot.Width;

                    int yp = myHalfHeight - (samples[i] * myHalfHeight / MIXERMAXSAMPLE);
                    if (yp < 0) yp = 0; else if (yp > LayoutRoot.Height) yp = (int)LayoutRoot.Height;

                    var line = new Line()
                    {
                        Stroke = new SolidColorBrush(Colors.LightSteelBlue),
                        X1 = xpOld,
                        X2 = xp,
                        Y1 = ypOld,
                        Y2 = yp
                    };
                    LayoutRoot.Children.Add(line);

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

                if (floatSamples == null)
                    return;

                Array.Copy(samplesToProcess, 0, samples, 0, anzSamples);
                for (int i = 0; i < anzSamples; i++)
                {
                    floatSamples[i] = ((float)samplesToProcess[i]) / maxPeakValue;
                }
                float[] resultFFTSamples = fft.Calculate(floatSamples);

                var a = 0;
                for (var bd = 0; bd < bands; bd++)
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
                samples = default;
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
