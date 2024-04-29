using System;


namespace SharpMod.DSP
{

    ///
    /// <summary> * This is an integer based version, returning float values though
    /// * @author Daniel Becker
    /// * @since 30.09.2007 </summary>
    /// 
    public class Fft
    {
        private const int FRAC_BITS = 16;
        private const int FRAC_FAC = 1 << FRAC_BITS;

        private readonly long[] xre;
        private readonly long[] xim;
        private readonly float[] mag;
        private readonly long[] fftSin;
        private readonly long[] fftCos;
        private readonly int[] fftBr;
        private readonly int ss;
        private readonly int ss2;
        private readonly int nu;

        ///	
        ///	 <summary> * Constructor for FFT </summary>
        ///	 
        public Fft(int pSampleSize)
        {
            ss = pSampleSize;
            ss2 = ss >> 1;

            nu = (int)(Math.Log(ss) / Math.Log(2.0D));

            xre = new long[ss];
            xim = new long[ss];
            mag = new float[ss2];

            fftSin = new long[nu * ss2];
            fftCos = new long[nu * ss2];

            fftBr = new int[ss];

            PrepareFFTTables();
        }

        ///	
        ///	 <summary> * We will here precalculate all sin and cos values
        ///	 * and all other things we can store
        ///	 * @since 03.10.2007 </summary>
        ///	 
        private void PrepareFFTTables()
        {
            int n2 = ss2;
            int nu1 = nu - 1;
            int k = 0;
            int x = 0;
            for (int l = 1; l <= nu; l++)
            {
                while (k < ss)
                {
                    for (int i = 1; i <= n2; i++)
                    {
                        double p = (double)BitRev(k >> nu1, nu);
                        double arg = (Math.PI * p * 2.0D) / (double)ss;
                        fftSin[x] = (long)(Math.Sin(arg) * FRAC_FAC);
                        fftCos[x] = (long)(Math.Cos(arg) * FRAC_FAC);
                        k++;
                        x++;
                    }

                    k += n2;
                }
                k = 0;
                nu1--;
                n2 >>= 1;
            }

            for (k = 0; k < ss; k++)
                fftBr[k] = BitRev(k, nu);
        }

        ///	
        ///	 <summary> * This will calculate the integer sqrt from a long value
        ///	 * @since 03.10.2007 </summary>
        ///	 * <param name="value">
        ///	 * @return </param>
        ///	 
        private static long LongSqrt(long value)
        {
            const int scale = 8;
            int bits = 64;
            long sqrt = 0;
            long rest = 0;
            for (int i = 0; i < scale; i++)
            {
                bits -= 8;
                rest = (rest << 8) | ((value >> bits) & 0xFF);
                long i2 = (sqrt << 5) + 1;

                int k0 = 0;
                while (true)
                {
                    long i3 = rest - i2;
                    if (i3 < 0)
                        break;

                    rest = i3;
                    i2 += 2;
                    k0++;
                }
                sqrt = (sqrt << 4) + k0;
            }

            return sqrt;
        }
        private static int BitRev(int j, int nu)
        {
            int j1 = j;
            int k = 0;
            for (int i = 1; i <= nu; i++)
            {
                int j2 = j1 >> 1;
                k = ((k << 1) + j1) - (j2 << 1);
                j1 = j2;
            }

            return k;
        }

        ///	
        ///	 <summary> * The main routine. We expect sampledata with 1.0<=x<=-1.0
        ///	 * The return value will be in the same spectrum.
        ///	 * @since 03.10.2007 </summary>
        ///	 * <param name="pSample">
        ///	 * @return </param>
        public virtual float[] Calculate(float[] pSample)
        {
            int wAps = pSample.Length / ss;
            int n2 = ss2;
            int nu1 = nu - 1;
            int a = 0;

            var b = 0;
            while (a < pSample.Length)            
            {
                xre[b] = (long)(pSample[a] * FRAC_FAC);
                xim[b] = 0;
                a += wAps;
                b++;
            }

            var x = 0;
            for (var l = 1; l <= nu; l++)
            {
                for (var k = 0; k < ss; k += n2)
                {
                    for (var i = 1; i <= n2; i++)
                    {

                        long c = fftCos[x];

                        long s = fftSin[x];

                        int kn2 = k + n2;

                        long tr = (xre[kn2] * c + xim[kn2] * s) >> FRAC_BITS;

                        long ti = (xim[kn2] * c - xre[kn2] * s) >> FRAC_BITS;
                        xre[kn2] = xre[k] - tr;
                        xim[kn2] = xim[k] - ti;
                        xre[k] += tr;
                        xim[k] += ti;
                        k++;
                        x++;
                    }

                }

                nu1--;
                n2 >>= 1;
            }

            for (int k = 0; k < ss; k++)
            {

                int r = fftBr[k];
                if (r > k)
                {

                    long tr = xre[k];

                    long ti = xim[k];
                    xre[k] = xre[r];
                    xim[k] = xim[r];
                    xre[r] = tr;
                    xim[r] = ti;
                }
            }

            mag[0] = ((float)((LongSqrt(xre[0] * xre[0] + xim[0] * xim[0])) >> FRAC_BITS)) / ((float)ss);
            for (int i = 1; i < ss2; i++)
                mag[i] = ((float)((LongSqrt(xre[i] * xre[i] + xim[i] * xim[i]) << 1) >> FRAC_BITS)) / ((float)ss);

            return mag;
        }
    }

}