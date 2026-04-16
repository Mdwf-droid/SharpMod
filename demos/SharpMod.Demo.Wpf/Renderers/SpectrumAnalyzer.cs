using System;

namespace SharpMod.Demo.Wpf.Renderers;

/// <summary>
/// FFT spectrum analyzer avec smoothing (identique à AnalyserNode WebAudio).
/// smoothingTimeConstant = 0.8 comme dans le Blazor.
/// </summary>
public class SpectrumAnalyzer
{
    private readonly int _bandCount;
    private readonly float[] _bands;
    private readonly float[] _smoothBands;
    private readonly float[] _fftBuffer;
    private readonly float[] _window;
    private int _writePos;
    private readonly int _fftSize;
    private readonly float _smoothing;

    public float[] Bands => _smoothBands;

    public SpectrumAnalyzer(int bandCount, int fftSize = 256, float smoothing = 0.8f)
    {
        _bandCount = bandCount;
        _fftSize = fftSize;
        _smoothing = smoothing;
        _bands = new float[bandCount];
        _smoothBands = new float[bandCount];
        _fftBuffer = new float[fftSize];
        _window = new float[fftSize];
        _writePos = 0;

        // Fenêtre de Hann (comme AnalyserNode)
        for (int i = 0; i < fftSize; i++)
            _window[i] = 0.5f * (1f - MathF.Cos(2f * MathF.PI * i / (fftSize - 1)));
    }

    /// <summary>
    /// Alimenté par NAudioTrackerStream.OnSamplesGenerated.
    /// Décode les bytes stéréo 16-bit et accumule dans le buffer FFT.
    /// </summary>
    public void AddStereoBytes(byte[] buffer, int bytesRead)
    {
        // Stéréo 16-bit = 4 bytes par sample
        int sampleCount = bytesRead / 4;
        for (int i = 0; i < sampleCount; i++)
        {
            int offset = i * 4;
            if (offset + 3 >= bytesRead) break;

            // Moyenne L+R
            short left = (short)(buffer[offset] | (buffer[offset + 1] << 8));
            short right = (short)(buffer[offset + 2] | (buffer[offset + 3] << 8));
            float sample = (left + right) / 65536f;

            _fftBuffer[_writePos] = sample;
            _writePos++;

            if (_writePos >= _fftSize)
            {
                _writePos = 0;
                ComputeFFT();
            }
        }
    }

    private void ComputeFFT()
    {
        int n = _fftSize;
        float[] real = new float[n];
        float[] imag = new float[n];

        // Appliquer la fenêtre
        for (int i = 0; i < n; i++)
        {
            real[i] = _fftBuffer[i] * _window[i];
            imag[i] = 0;
        }

        // FFT in-place (Cooley-Tukey)
        int bits = (int)Math.Log2(n);
        for (int i = 0; i < n; i++)
        {
            int j = ReverseBits(i, bits);
            if (j > i)
            {
                (real[i], real[j]) = (real[j], real[i]);
                (imag[i], imag[j]) = (imag[j], imag[i]);
            }
        }

        for (int size = 2; size <= n; size *= 2)
        {
            int halfSize = size / 2;
            float angle = -2f * MathF.PI / size;
            for (int i = 0; i < n; i += size)
            {
                for (int j = 0; j < halfSize; j++)
                {
                    float cos = MathF.Cos(angle * j);
                    float sin = MathF.Sin(angle * j);
                    float tReal = real[i + j + halfSize] * cos - imag[i + j + halfSize] * sin;
                    float tImag = real[i + j + halfSize] * sin + imag[i + j + halfSize] * cos;
                    real[i + j + halfSize] = real[i + j] - tReal;
                    imag[i + j + halfSize] = imag[i + j] - tImag;
                    real[i + j] += tReal;
                    imag[i + j] += tImag;
                }
            }
        }

        // Magnitude → bandes
        int halfN = n / 2;
        int bandsPerBin = halfN / _bandCount;
        if (bandsPerBin < 1) bandsPerBin = 1;

        for (int b = 0; b < _bandCount; b++)
        {
            float sum = 0;
            int start = b * bandsPerBin;
            int end = Math.Min(start + bandsPerBin, halfN);
            for (int i = start; i < end; i++)
            {
                float mag = MathF.Sqrt(real[i] * real[i] + imag[i] * imag[i]);
                sum += mag;
            }
            float avg = sum / (end - start);

            // Convertir en dB puis normaliser (0-1), comme AnalyserNode
            float db = 20f * MathF.Log10(avg + 1e-10f);
            float minDb = -60f;
            float maxDb = 0f;
            float normalized = Math.Clamp((db - minDb) / (maxDb - minDb), 0f, 1f);

            _bands[b] = normalized;
        }

        // ★ SMOOTHING identique à AnalyserNode (smoothingTimeConstant = 0.8)
        for (int b = 0; b < _bandCount; b++)
        {
            _smoothBands[b] = _smoothing * _smoothBands[b] + (1f - _smoothing) * _bands[b];
        }
    }

    private static int ReverseBits(int value, int bits)
    {
        int result = 0;
        for (int i = 0; i < bits; i++)
        {
            result = (result << 1) | (value & 1);
            value >>= 1;
        }
        return result;
    }
}
