using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace SharpMod.SoundRenderer
{
    public class SampleRate
    {
        public const int DefaultSampleRate = 48000;

        private int _samplesPerSecond;

        public SampleRate()
        {
            _samplesPerSecond = DefaultSampleRate;
        }

        public SampleRate(int samplesPerSecond)
        {
            _samplesPerSecond = samplesPerSecond;
        }


        public TimeSpan OneSample
        {
            get { return TimeSpan.FromSeconds(1.0 / _samplesPerSecond); }
        }

        public double SecondsPerSample
        {
            get { return 1.0 / _samplesPerSecond; }
        }


        public int SamplesPerSecond
        {
            get { return _samplesPerSecond; }
            set { _samplesPerSecond = value; }
        }

    }
}
