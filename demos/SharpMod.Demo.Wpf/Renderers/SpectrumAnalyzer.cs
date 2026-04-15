using System;

namespace SharpMod.Demo.Wpf.Renderers;

/// <summary>
/// Spectrum analyzer : FFT sur flux stéréo 16-bit → bandes logarithmiques.
/// Thread-safe (lock interne).
/// </summary>
public class SpectrumAnalyzer
{
    private readonly int _fftSize;
    private readonly int _bandCount;
    private readonly float[] _fftBuffer;
    private readonly float[] _window;
    private readonly float[] _magnitudes;
    private readonly float[] _bands;
    private readonly float[] _bandsSmooth;
    private int _writePos;
    private readonly object _lock = new();

    /// <summary>Bandes lissées (0.0–1.0) pour affichage.</summary>
    public float[] Bands => _bandsSmooth;

    /// <summary>Nombre de bandes.</summary>
    public int BandCount => _bandCount;

    public SpectrumAnalyzer(int fftSize = 512, int bandCount = 32)
    {
        _fftSize = fftSize;
        _bandCount = bandCount;
        _fftBuffer = new float[fftSize];
        _window = new float[fftSize];
        _magnitudes = new float[fftSize / 2];
        _bands = new float[bandCount];
        _bandsSmooth = new float[bandCount];

        // Fenêtre de Hanning
        for (int i = 0; i < fftSize; i++)
            _window[i] = 0.5f * (1f - MathF.Cos(2f * MathF.PI * i / (fftSize - 1)));
    }

    /// <summary>
    /// Ajoute des samples depuis un buffer audio stéréo 16-bit interleaved.
    /// Fait un downmix mono (L+R)/2. Peut être appelé depuis n'importe quel thread.
    /// </summary>
    public void AddStereoBytes(byte[] buffer, int bytesCount)
    {
        lock (_lock)
        {
            // Format: 16-bit signed, stéréo interleaved → 4 bytes par frame
            int frames = bytesCount / 4;
            for (int i = 0; i < frames; i++)
            {
                int off = i * 4;
                if (off + 3 >= buffer.Length) break;

                short left = (short)(buffer[off] | (buffer[off + 1] << 8));
                short right = (short)(buffer[off + 2] | (buffer[off + 3] << 8));

                float mono = (left + right) / 65536f;

                _fftBuffer[_writePos] = mono;
                _writePos = (_writePos + 1) % _fftSize;

                // Buffer plein → calculer la FFT
                if (_writePos == 0)
                    ComputeFFT();
            }
        }
    }

    private void ComputeFFT()
    {
        float[] real = new float[_fftSize];
        float[] imag = new float[_fftSize];

        // Appliquer la fenêtre
        for (int i = 0; i < _fftSize; i++)
            real[i] = _fftBuffer[i] * _window[i];

        // FFT in-place
        FFT(real, imag);

        // Magnitudes
        for (int i = 0; i < _fftSize / 2; i++)
            _magnitudes[i] = MathF.Sqrt(real[i] * real[i] + imag[i] * imag[i]);

        // Regrouper en bandes
        ComputeBands();
    }

    private void ComputeBands()
    {
        int halfSize = _fftSize / 2;

        for (int b = 0; b < _bandCount; b++)
        {
            // Découpage logarithmique
            float t0 = (float)b / _bandCount;
            float t1 = (float)(b + 1) / _bandCount;
            int i0 = (int)(MathF.Pow(t0, 2) * halfSize);
            int i1 = Math.Max(i0 + 1, (int)(MathF.Pow(t1, 2) * halfSize));
            i1 = Math.Min(i1, halfSize);

            float sum = 0;
            int count = 0;
            for (int i = i0; i < i1; i++)
            {
                sum += _magnitudes[i];
                count++;
            }

            float avg = count > 0 ? sum / count : 0;
            _bands[b] = Math.Min(avg * 8f, 1f);

            // Lissage : montée rapide, descente lente
            if (_bands[b] > _bandsSmooth[b])
                _bandsSmooth[b] = _bands[b];
            else
                _bandsSmooth[b] = _bandsSmooth[b] * 0.88f + _bands[b] * 0.12f;
        }
    }

    /// <summary>FFT Cooley-Tukey radix-2 in-place.</summary>
    private static void FFT(float[] real, float[] imag)
    {
        int n = real.Length;

        // Bit-reversal
        for (int i = 1, j = 0; i < n; i++)
        {
            int bit = n >> 1;
            for (; (j & bit) != 0; bit >>= 1)
                j ^= bit;
            j ^= bit;

            if (i < j)
            {
                (real[i], real[j]) = (real[j], real[i]);
                (imag[i], imag[j]) = (imag[j], imag[i]);
            }
        }

        // Butterfly
        for (int len = 2; len <= n; len *= 2)
        {
            float ang = -2f * MathF.PI / len;
            float wR = MathF.Cos(ang);
            float wI = MathF.Sin(ang);

            for (int i = 0; i < n; i += len)
            {
                float curR = 1f, curI = 0f;
                for (int j = 0; j < len / 2; j++)
                {
                    int u = i + j;
                    int v = i + j + len / 2;

                    float tR = curR * real[v] - curI * imag[v];
                    float tI = curR * imag[v] + curI * real[v];

                    real[v] = real[u] - tR;
                    imag[v] = imag[u] - tI;
                    real[u] += tR;
                    imag[u] += tI;

                    float newR = curR * wR - curI * wI;
                    curI = curR * wI + curI * wR;
                    curR = newR;
                }
            }
        }
    }
}
