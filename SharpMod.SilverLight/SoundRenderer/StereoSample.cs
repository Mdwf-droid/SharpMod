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
    public class StereoSample
    {
        public int Left { get; set; }
        public int Right { get; set; }

        public StereoSample()
        {
        }

        public StereoSample(int left, int right)
        {
            Left = left;
            Right = right;
        }

        // assumption is volume is between 0 and 1
        public void AdjustForVolume(double volume)
        {
            // math.round would be more accurate, but more costly
            Left = (int)(Left * volume);
            Right = (int)(Right * volume);
        }
        
        public byte[] GetDownSampledBytes()
        {
            // down sample from 32 bits to 16 to help eliminate distortion
            short shortLeft = (short)(Left >> 16);
            short shortRight = (short)(Right >> 16);

            // todo: make this more efficient using bit shifting and masks
            byte[] left = BitConverter.GetBytes(shortLeft);
            byte[] right = BitConverter.GetBytes(shortRight);

            byte[] combined = new byte[4];
            combined[0] = left[0];
            combined[1] = left[1];
            combined[2] = right[0];
            combined[3] = right[1];

            return combined;
        }

        public int GetDownSampledByteCount()
        {
            return sizeof(short) * 2;
        }
    }
}
