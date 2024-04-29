
using SharpMod.DSP;
using SharpMod.Player;
using System;

namespace SharpMod.Mixer
{
    public delegate void TickHandler();
    public delegate short BPMRequestHandler();

    /// <summary>
    /// All-c sample mixing routines, using a 32 bits mixing buffer
    /// </summary>
    public class ChannelsMixer
    {
        private delegate void MixingFunctionHandler(byte[] srce, int[] dest, int dest_offset, int index, int increment, short todo);
        private event MixingFunctionHandler OnMixingFunction;

        public event TickHandler OnTickHandler;
        public event BPMRequestHandler OnBPMRequest;

        int RVc1, RVc2, RVc3, RVc4, RVc5, RVc6, RVc7, RVc8;
        int[] RVbufL1 = null;
        int[] RVbufL2 = null;
        int[] RVbufL3 = null;
        int[] RVbufL4 = null;
        int[] RVbufL5 = null;
        int[] RVbufL6 = null;
        int[] RVbufL7 = null;
        int[] RVbufL8 = null;
        int[] RVbufR1 = null;
        int[] RVbufR2 = null;
        int[] RVbufR3 = null;
        int[] RVbufR4 = null;
        int[] RVbufR5 = null;
        int[] RVbufR6 = null;
        int[] RVbufR7 = null;
        int[] RVbufR8 = null;

        private const int REVERBERATION = 110000;


        private const int CLICK_SHIFT = 6;
        private readonly int CLICK_BUFFER = (1 << CLICK_SHIFT);

        internal AudioProcessor _audioProcessor;

        private MixConfig _mixCfg;

        internal MixConfig MixCfg
        {
            get { return _mixCfg; }
            set
            {
                _mixCfg = value;
                RVc1 = (5000 * _mixCfg.Rate) / REVERBERATION;
                RVc2 = (5078 * _mixCfg.Rate) / REVERBERATION;
                RVc3 = (5313 * _mixCfg.Rate) / REVERBERATION;
                RVc4 = (5703 * _mixCfg.Rate) / REVERBERATION;
                RVc5 = (6250 * _mixCfg.Rate) / REVERBERATION;
                RVc6 = (6953 * _mixCfg.Rate) / REVERBERATION;
                RVc7 = (7813 * _mixCfg.Rate) / REVERBERATION;
                RVc8 = (8828 * _mixCfg.Rate) / REVERBERATION;

                RVbufL1 = new int[RVc1 + 1];
                RVbufL2 = new int[RVc2 + 1];
                RVbufL3 = new int[RVc3 + 1];
                RVbufL4 = new int[RVc4 + 1];
                RVbufL5 = new int[RVc5 + 1];
                RVbufL6 = new int[RVc6 + 1];
                RVbufL7 = new int[RVc7 + 1];
                RVbufL8 = new int[RVc8 + 1];

                RVbufR1 = new int[RVc1 + 1];
                RVbufR2 = new int[RVc2 + 1];
                RVbufR3 = new int[RVc3 + 1];
                RVbufR4 = new int[RVc4 + 1];
                RVbufR5 = new int[RVc5 + 1];
                RVbufR6 = new int[RVc6 + 1];
                RVbufR7 = new int[RVc7 + 1];
                RVbufR8 = new int[RVc8 + 1];
            }

        }

        public static readonly int TICKLSIZE = 8192; //3600
        public static readonly int TICKWSIZE = (TICKLSIZE * 2);
        public static readonly int TICKBSIZE = (TICKWSIZE * 2);



        protected internal int[] VC_TICKBUF; // [TICKLSIZE] 
        protected internal ChannelInfo[] vinf; // [32]
        protected internal ChannelInfo vnf;

        protected internal short samplesthatfit;
        protected internal int idxsize;
        protected internal int idxlpos;
        protected internal int idxlend;
        protected internal int maxvol;

        protected internal int per256;
        protected internal int ampshift;

        protected internal int lvolmul;
        protected internal int rvolmul;

        public WaveTable WaveTable { get; set; }

        protected internal int TICKLEFT;

        protected internal const int FRACBITS = 11;
        protected internal static readonly int FRACMASK = ((1 << FRACBITS) - 1);


        internal int ChannelsCount { get; set; }

        internal int BPM { get; set; }


        public ChannelsMixer(MixConfig mixCfg)
        {
            MixCfg = mixCfg;

            int i;

            VC_TICKBUF = new int[TICKLSIZE];
            vinf = new ChannelInfo[32];
            for (i = 0; i < 32; i++)
                vinf[i] = new ChannelInfo();

            VC_TICKBUF.Initialize();

            for (i = 0; i < 32; i++)
            {
                vinf[i].Flags = 0;
                vinf[i].Start = 0;
                vinf[i].Size = 0;
                vinf[i].Reppos = 0;
                vinf[i].Repend = 0;
                vinf[i].Frq = 0;
                vinf[i].Current = 0;
                vinf[i].Increment = 0;
                vinf[i].LeftVolMul = 0;
                vinf[i].RightVolMul = 0;
                vinf[i].Handle = 0;
                vinf[i].Vol = 0;
                vinf[i].Pan = 0;
                vinf[i].Kick = false;
                vinf[i].Active = false;
            }

            vnf = null;

            samplesthatfit = 0;
            idxsize = 0;
            idxlpos = 0;
            idxlend = 0;
            maxvol = 0;
            per256 = 0;
            ampshift = 0;
            lvolmul = 0;
            rvolmul = 0;

            TICKLEFT = 0;
        }


        protected internal virtual void VC_Sample32To8Copy(int[] srce, sbyte[] dest, int dest_offset, int count, short shift)
        {
            int c;
            int shifti = (24 - ampshift);
            int src_idx = 0, dest_idx = dest_offset;

            while ((count--) != 0)
            {
                c = (sbyte)(srce[src_idx] >> shifti);
                if (c > 127)
                    c = (sbyte)127;
                else if (c < -128)
                    c = -128;
                dest[dest_idx++] = (sbyte)(c + 128);
                src_idx++;
            }
        }


        protected internal virtual void VC_Sample32To16Copy(int[] srce, sbyte[] dest, int dest_offset, int count, short shift)
        {
            int c;
            int shifti = (16 - ampshift);
            int src_idx = 0, dest_idx = dest_offset;

            while (count-- > 0)
            {
                c = srce[src_idx] >> shifti;
                if (c > 32767)
                    c = 32767;
                else if (c < -32768)
                    c = -32768;

                if (BitConverter.IsLittleEndian)
                {
                    dest[dest_idx++] = (sbyte)(c & 0xFF);
                    dest[dest_idx++] = (sbyte)((c >> 8) & 0xFF);
                }
                else
                {
                    dest[dest_idx++] = (sbyte)((c >> 8) & 0xFF);
                    dest[dest_idx++] = (sbyte)(c & 0xFF);
                }
                src_idx++;
            }
        }

        /// <summary>
        /// Converts the fraction 'dividend/divisor' into a fixed point longword.
        /// Used to set Sample Frequency 
        /// </summary>
        /// <param name="dividend"></param>
        /// <param name="divisor"></param>
        /// <returns></returns>
        protected internal static int Fraction2Long(int dividend, int divisor)
        {
            int whole, part;

            whole = dividend / divisor;
            part = ((dividend % divisor) << FRACBITS) / divisor;

            return ((whole << FRACBITS) | part);
        }


        protected internal virtual int Samples2Bytes(int samples)
        {
            if (MixCfg.Is16Bits) 
                samples <<= 1;
            if (MixCfg.Style != RenderingStyle.Mono) 
                samples <<= 1;
            return samples;
        }


        protected internal virtual int Bytes2Samples(int bytes)
        {
            if (MixCfg.Is16Bits)
                bytes >>= 1;
            if (MixCfg.Style != RenderingStyle.Mono)
                bytes >>= 1;
            return bytes;
        }




        protected internal virtual void MixStereoNormal(byte[] srce, int[] dest, int dest_offset, int index, int increment, short todo)
        {
            var sample = 0;
            var dest_idx = dest_offset;

            while ((todo--) > 0)
            {
                sample = (sbyte)srce[index >> FRACBITS];
                index += increment;

                if (vnf.RampVol > 0)
                {
                    dest[dest_idx++] += (
                      (((vnf.OldLeftVol * vnf.RampVol) +
                          (vnf.LeftVolMul * (CLICK_BUFFER - vnf.RampVol))
                        ) * sample) >> CLICK_SHIFT);
                    dest[dest_idx++] += (
                      (((vnf.OldRightVol * vnf.RampVol) +
                          (vnf.RightVolMul * (CLICK_BUFFER - vnf.RampVol))
                        ) * sample) >> CLICK_SHIFT);
                    vnf.RampVol--;
                }
                else
                    if (vnf.Click > 0)
                {
                    dest[dest_idx++] += (
                    (((vnf.LeftVolMul * (CLICK_BUFFER - vnf.Click)) *
                        sample) + (vnf.LastValLeft * vnf.Click))
                      >> CLICK_SHIFT);
                    dest[dest_idx++] += (
                    (((vnf.RightVolMul * (CLICK_BUFFER - vnf.Click)) *
                        sample) + (vnf.LastValRight * vnf.Click))
                      >> CLICK_SHIFT);
                    vnf.Click--;
                }
                else
                {
                    dest[dest_idx++] += vnf.LeftVolMul * sample;
                    dest[dest_idx++] += vnf.RightVolMul * sample;
                }
            }
            vnf.LastValLeft = vnf.LeftVolMul * sample;
            vnf.LastValRight = vnf.RightVolMul * sample;


        }


        protected internal virtual void MixMonoNormal(byte[] srce, int[] dest, int dest_offset, int index, int increment, short todo)
        {
            sbyte sample;
            int dest_idx = dest_offset;

            while (todo > 0)
            {
                sample = (sbyte)srce[index >> FRACBITS];
                dest[dest_idx++] += lvolmul * sample;
                index += increment;
                todo--;
            }
        }

        protected internal virtual void MixSurroundNormal(byte[] srce, int[] dest, int dest_offset, int index, int increment, short todo)
        {
            int sample = 0;
            int whoop;
            int dest_idx = dest_offset;

            while ((todo--) > 0)
            {
                sample = (sbyte)srce[index >> FRACBITS];

                index += increment;

                if (vnf.RampVol > 0)
                {
                    whoop = (
                      (((vnf.OldLeftVol * vnf.RampVol) +
                          (vnf.LeftVolMul * (CLICK_BUFFER - vnf.RampVol))) *
                        sample) >> CLICK_SHIFT);
                    dest[dest_idx++] += whoop;
                    dest[dest_idx++] -= whoop;
                    vnf.RampVol--;
                }
                else
                    if (vnf.Click > 0)
                {
                    whoop = (
                      (((vnf.LeftVolMul * (CLICK_BUFFER - vnf.Click)) *
                          sample) +
                        (vnf.LastValLeft * vnf.Click)) >> CLICK_SHIFT);
                    dest[dest_idx++] += whoop;
                    dest[dest_idx++] -= whoop;
                    vnf.Click--;
                }
                else
                {
                    dest[dest_idx++] += vnf.LeftVolMul * sample;
                    dest[dest_idx++] -= vnf.LeftVolMul * sample;
                }
            }
            vnf.LastValLeft = vnf.LeftVolMul * sample;
            vnf.LastValRight = vnf.LeftVolMul * sample;

        }

        protected internal virtual void MixStereoInterp(byte[] srce, int[] dest, int dest_offset, int index, int increment, short todo)
        {
            int sample;
            int lvolsel = vnf.LeftVolMul;
            int rvolsel = vnf.RightVolMul;
            int rampvol = vnf.RampVol;
            int a;
            int b;
            int dest_idx = dest_offset;

            if (rampvol > 0)
            {
                int oldlvol = vnf.OldLeftVol - lvolsel;
                int oldrvol = vnf.OldRightVol - rvolsel;
                while ((todo--) > 0)
                {
                    a = (sbyte)srce[index >> FRACBITS];
                    b = (sbyte)srce[1 + (index >> FRACBITS)];
                    sample = (short)(a + (((b - a) * (index & FRACMASK)) >> FRACBITS));

                    index += increment;

                    dest[dest_idx++] += ((lvolsel << CLICK_SHIFT) + oldlvol * rampvol)
                               * sample >> CLICK_SHIFT;
                    dest[dest_idx++] += ((rvolsel << CLICK_SHIFT) + oldrvol * rampvol)
                               * sample >> CLICK_SHIFT;
                    if (--rampvol <= 0)
                        break;
                }
                vnf.RampVol = rampvol;
                if (todo < 0)
                    return;
            }

            while ((todo--) > 0)
            {
                a = (sbyte)srce[index >> FRACBITS];
                b = (sbyte)srce[1 + (index >> FRACBITS)];
                sample = (short)(a + (((b - a) * (index & FRACMASK)) >> FRACBITS));

                index += increment;

                dest[dest_idx++] += lvolsel * sample;
                dest[dest_idx++] += rvolsel * sample;
            }

        }


        protected internal virtual void MixMonoInterp(byte[] srce, int[] dest, int dest_offset, int index, int increment, short todo)
        {
            short sample, a, b;
            int dest_idx = dest_offset;
            int rampvol = vnf.RampVol;

            if (rampvol > 0)
            {
                int oldlvol = vnf.OldVol - vnf.LeftVolMul;
                while ((todo--) > 0)
                {
                    a = (sbyte)srce[index >> FRACBITS];
                    b = (sbyte)srce[1 + (index >> FRACBITS)];
                    sample = (short)(a + (((b - a) * (index & FRACMASK)) >> FRACBITS));

                    index += increment;

                    dest[dest_idx++] += ((vnf.LeftVolMul << CLICK_SHIFT) + oldlvol * rampvol)
                               * sample >> CLICK_SHIFT;
                    if (--rampvol <= 0)
                        break;
                }
                vnf.RampVol = rampvol;
                if (todo < 0)
                    return;
            }


            while (todo > 0)
            {
                a = srce[index >> FRACBITS];
                b = srce[1 + (index >> FRACBITS)];
                sample = (short)(a + (((b - a) * (index & FRACMASK)) >> FRACBITS));

                dest[dest_idx++] += lvolmul * sample;

                index += increment;
                todo--;
            }
        }

        protected internal virtual void MixSurroundInterp(byte[] srce, int[] dest, int dest_offset, int index, int increment, short todo)
        {
            short sample, a, b;
            int dest_idx = dest_offset;
            int vol;
            int rampvol = vnf.RampVol;
            int lvolsel = vnf.LeftVolMul;
            int rvolsel = vnf.RightVolMul;
            int oldvol;

            if (lvolsel > rvolsel)
            {
                vol = lvolsel;
                oldvol = vnf.OldLeftVol;
            }
            else
            {
                vol = rvolsel;
                oldvol = vnf.OldRightVol;
            }

            if (rampvol > 0)
            {
                oldvol -= vol;
                while ((todo--) > 0)
                {
                    sample = (short)(srce[index >> FRACBITS] +
                           ((srce[(index >> FRACBITS) + 1] - srce[index >> FRACBITS])
                            * (index & FRACMASK) >> FRACBITS));
                    index += increment;

                    sample = (short)(((vol << CLICK_SHIFT) + oldvol * rampvol)
                           * sample >> CLICK_SHIFT);
                    dest[dest_idx++] += sample;
                    dest[dest_idx++] -= sample;

                    if (--rampvol <= 0)
                        break;
                }
                vnf.RampVol = rampvol;
                if (todo < 0)
                    return;
            }

            while (todo > 0)
            {
                a = (sbyte)srce[index >> FRACBITS];
                b = (sbyte)srce[1 + (index >> FRACBITS)];
                sample = (short)(a + (((b - a) * (index & FRACMASK)) >> FRACBITS));

                dest[dest_idx++] += vol * sample;
                dest[dest_idx++] += vol * sample;
                index += increment;
                todo--;
            }
        }


        /// <summary>
        /// This functions returns the number of resamplings we can do so that:
        ///
        /// - it never accesses indexes bigger than index 'end'
        /// - it doesn't do more than 'resamplingsToDo' resamplings
        /// </summary>
        /// <param name="index"></param>
        /// <param name="end"></param>
        /// <param name="increment"></param>
        /// <param name="resamplingsToDo"></param>
        /// <returns></returns>
        internal static int NewPredict(int index, int end, int increment, int resamplingsToDo)
        {
            int di;

            di = (end - index) / increment;
            index += (di * increment);

            if (increment < 0)
            {
                while (index >= end)
                {
                    index += increment;
                    di++;
                }
            }
            else
            {
                while (index <= end)
                {
                    index += increment;
                    di++;
                }
            }
            return ((di < resamplingsToDo) ? di : resamplingsToDo);
        }

        /// <summary>
        /// Mixes 'samplesToDo' stereo or mono samples of the current channel to the tickbuffer.
        /// </summary>
        /// <param name="ptr"></param>
        /// <param name="samplesToDo"></param>
        protected internal virtual void VC_AddChannel(int[] ptr, int samplesToDo)
        {
            int end;
            int done;

            int ptr_idx = 0;

            while (samplesToDo > 0)
            {

                // update the 'current' index so the sample loops, or
                // stops playing if it reached the end of the sample
                if ((vnf.Flags & (SampleFormats.SF_REVERSE)) != 0)
                {
                    // The sample is playing in reverse
                    if ((vnf.Flags & (SampleFormats.SF_LOOP)) != 0)
                    {
                        // the sample is looping, so check if it reached the loopstart index
                        if (vnf.Current < idxlpos)
                        {
                            if ((vnf.Flags & (SampleFormats.SF_BIDI)) != 0)
                            {
                                // sample is doing bidirectional loops, so 'bounce'
                                // the current index against the idxlpos
                                vnf.Current = idxlpos + (idxlpos - vnf.Current);
                                vnf.Flags &= ~(SampleFormats.SF_REVERSE);
                                vnf.Increment = -vnf.Increment;
                            }
                            // normal backwards looping, so set the current position to loopend index
                            else
                                vnf.Current = idxlend - (idxlpos - vnf.Current);
                        }
                    }
                    else
                    {
                        // the sample is not looping, so check if it reached index 0
                        if (vnf.Current < 0)
                        {

                            // playing index reached 0, so stop playing this sample
                            vnf.Current = 0;
                            vnf.Active = false;
                            break;
                        }
                    }
                }
                else
                {
                    // The sample is playing forward
                    if ((vnf.Flags & (SampleFormats.SF_LOOP)) != 0)
                    {
                        // the sample is looping, so check if it reached the loopend index
                        if (vnf.Current > idxlend)
                        {
                            if ((vnf.Flags & (SampleFormats.SF_BIDI)) != 0)
                            {
                                // sample is doing bidirectional loops, so 'bounce' the current index against the idxlend
                                vnf.Flags |= (SampleFormats.SF_REVERSE);
                                vnf.Increment = -vnf.Increment;
                                vnf.Current = idxlend - (vnf.Current - idxlend); /* ?? */
                            }
                            // normal backwards looping, so set the current position to loopend index 
                            else
                                vnf.Current = idxlpos + (vnf.Current - idxlend);
                        }
                    }
                    else
                    {
                        // sample is not looping, so check if it reached the last position
                        if (vnf.Current > idxsize)
                        {

                            // yes, so stop playing this sample
                            vnf.Current = 0;
                            vnf.Active = false;
                            break;
                        }
                    }
                }

                // Ask a far ptr at the sample address vnf.current at byte offset, and
                // number of samples shall be valid (BEFORE segment crossing occurs)
                byte[] sample = WaveTable.Samples[vnf.Handle];
                if (sample == null)            
                {
                    vnf.Current = 0;
                    vnf.Active = false;
                    break;
                }

                if ((vnf.Flags & (SampleFormats.SF_REVERSE)) != 0)
                    end = ((vnf.Flags & (SampleFormats.SF_LOOP)) != 0) ? idxlpos : 0;
                else
                    end = ((vnf.Flags & (SampleFormats.SF_LOOP)) != 0) ? idxlend : idxsize;

                // If the sample is simply not available, or if sample has to be stopped sample stop and stop
                // mix 'em: 
                done = NewPredict(vnf.Current, end, vnf.Increment, samplesToDo);

                if (done == 0)
                {
                    vnf.Active = false;
                    break;
                }

                // optimisation: don't mix anything if volume is zero
                if (vnf.Vol != 0 || vnf.RampVol != 0)
                {
                    SampleMix(sample, ptr, ptr_idx, vnf.Current, vnf.Increment, (short)done);
                }
                else
                {
                    vnf.LastValLeft = 0;
                    vnf.LastValRight = 0;
                }

                vnf.Current += (vnf.Increment * done);
                samplesToDo -= done;
                ptr_idx += (MixCfg.Style != RenderingStyle.Mono) ? (done << 1) : done;
            }


        }

        /// <summary>
        /// Mixes 'samplesToDo' samples to 'buf'.. The number of samples has
        /// to fit into the tickbuffer.
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="buf_offset"></param>
        /// <param name="samplesToDo"></param>
        protected internal virtual void VC_FillTick(sbyte[] buf, int buf_offset, short samplesToDo)
        {
            int t;

            //clear the mixing buffer:
            Array.Clear(VC_TICKBUF, 0, ((MixCfg.Style != RenderingStyle.Mono) ? (samplesToDo << 1) : (samplesToDo)));

            for (t = 0; t < ChannelsCount; t++)
            {
                vnf = vinf[t];

                if (vnf.Active)
                {
                    idxsize = (vnf.Size << FRACBITS) - 1;
                    idxlpos = vnf.Reppos << FRACBITS;
                    idxlend = (vnf.Repend << FRACBITS) - 1;
                    lvolmul = vnf.LeftVolMul;
                    rvolmul = vnf.RightVolMul;
                    VC_AddChannel(VC_TICKBUF, samplesToDo);
                }
            }

            if (MixCfg.NoiseReduction)
                MixLowPass_Stereo(VC_TICKBUF, samplesToDo);

            if (MixCfg.Reverb > 0)
                MixReverb_Stereo(VC_TICKBUF, samplesToDo);

            if (_audioProcessor != null)
            {
                int[] DspOutput = new int[samplesToDo << 1];
                for (int i = 0; i < samplesToDo << 1; i++)
                    DspOutput[i] = VC_TICKBUF[i] >> (16 - ampshift);
                _audioProcessor.WriteSampleData(DspOutput, 0, samplesToDo << 1);
                _audioProcessor.Run();
            }

            if (MixCfg.Is16Bits)                
                VC_Sample32To16Copy(VC_TICKBUF, buf, buf_offset, MixCfg.Style != RenderingStyle.Mono ? samplesToDo << 1 : samplesToDo, (short)(16 - ampshift));
            else
                VC_Sample32To8Copy(VC_TICKBUF, buf, buf_offset, MixCfg.Style != RenderingStyle.Mono ? samplesToDo << 1 : samplesToDo, (short)(24 - ampshift));

        }


        /// <summary>
        /// Writes 'samplesToDo' mixed SAMPLES (!!) to 'buf'. When samplesToDo is bigger than the
        /// number of samples that fit into VC_TICKBUF, the mixing operation is split
        /// up into a number of smaller chunks.
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="buf_offset"></param>
        /// <param name="samplesToDo"></param>
        protected internal virtual void VC_WritePortion(sbyte[] buf, int buf_offset, short samplesToDo)
        {
            short part;
            int buf_ptr = buf_offset;

            // write 'part' samples to the buffer
            while (samplesToDo != 0)
            {
                part = (samplesToDo < samplesthatfit) ? samplesToDo : samplesthatfit;
                VC_FillTick(buf, buf_ptr, part);
                buf_ptr += Samples2Bytes(part);
                samplesToDo = (short)(samplesToDo - part);
            }
        }


        public virtual void VC_WriteSamples(sbyte[] buf, int todo)
        {
            int t;
            short part;
            int buf_ptr = 0;

            while (todo > 0)
            {
                if (TICKLEFT == 0)
                {
                    TickHandler();

                    TICKLEFT = (125 * MixCfg.Rate) / (50 * BPM);

                    // compute volume, frequency counter & panning parameters for each channel.
                    for (t = 0; t < ChannelsCount; t++)
                    {
                        int pan, vol, lvol, rvol;

                        if (vinf[t].Kick)
                        {
                            vinf[t].Current = (vinf[t].Start << FRACBITS);
                            vinf[t].Active = true;
                            vinf[t].Kick = false;
                            vinf[t].Click = CLICK_BUFFER;
                            vinf[t].RampVol = 0;
                        }

                        if (vinf[t].Frq == 0)
                            vinf[t].Active = false;

                        if (vinf[t].Active)
                        {
                            vinf[t].Increment = Fraction2Long(vinf[t].Frq, MixCfg.Rate);

                            if ((vinf[t].Flags & (SampleFormats.SF_REVERSE)) != 0)
                                vinf[t].Increment = -vinf[t].Increment;

                            vol = vinf[t].Vol;
                            pan = vinf[t].Pan;

                            vinf[t].OldLeftVol = vinf[t].LeftVolMul;
                            vinf[t].OldRightVol = vinf[t].RightVolMul;

                                                        
                            if (MixCfg.Style == RenderingStyle.Stereo ||
                               MixCfg.Style == RenderingStyle.Surround)
                            {
                                lvol = (vol * ((pan < 128) ? 128 : (255 - pan))) / 128;
                                rvol = (vol * ((pan > 128) ? 128 : pan)) / 128;
                                vinf[t].LeftVolMul = (maxvol * lvol) / 64;
                                vinf[t].RightVolMul = (maxvol * rvol) / 64;
                            }
                            else
                            {
                                vinf[t].LeftVolMul = (maxvol * vol) / 64;
                            }
                        }
                    }
                }

                part = (short)((TICKLEFT < todo) ? TICKLEFT : todo);

                VC_WritePortion(buf, buf_ptr, part);

                TICKLEFT -= part;
                todo -= part;

                buf_ptr += Samples2Bytes(part);
            }
        }

        /// <summary>
        /// Writes 'bytesToDo' mixed chars (!!) to 'buf'. It returns the number of
        /// chars actually written to 'buf' (which is rounded to number of samples
        /// that fit into 'bytesToDo' bytes).
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="bytesToDo"></param>
        /// <returns></returns>
        public virtual int VC_WriteBytes(sbyte[] buf, int bytesToDo)
        {
            bytesToDo = Bytes2Samples(bytesToDo);
            VC_WriteSamples(buf, bytesToDo);
            return Samples2Bytes(bytesToDo);
        }

        /// <summary>
        /// Fill the buffer with 'bytesToDo' bytes of silence (it depends on the mixing
        /// mode how the buffer is filled)
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="bytesToDo"></param>
        public virtual void VC_SilenceBytes(sbyte[] buf, short bytesToDo)
        {
            // clear the buffer to zero (16 bits signed ) or 0x80 (8 bits unsigned)          
            if (MixCfg.Is16Bits)
            {
                Array.Clear(buf, 0, bytesToDo);
            }
            else
            {
                Array.Fill<sbyte>(buf, -128);
            }
        }


        public virtual void VC_PlayStart()
        {
            int t;

            for (t = 0; t < 32; t++)
            {
                vinf[t].Current = 0;
                vinf[t].Flags = 0;
                vinf[t].Handle = 0;
                vinf[t].Kick = false;
                vinf[t].Active = false;
                vinf[t].Frq = 10000;
                vinf[t].Vol = 0;
                vinf[t].Pan = ((t & 1) != 0) ? ((short)0) : ((short)255);
            }

            if (ChannelsCount > 0)
                // sanity check - avoid core dump! 
                maxvol = 16777216 / (ChannelsCount);
            else
                maxvol = 16777216;

            // instead of using a amplifying lookup table, I'm using a simple shift
            //amplify now.. amplifying doubles with every extra 4 channels, and also
            //doubles in stereo mode.. this seems to give similar volume levels
            //across the channel range
            ampshift = ChannelsCount / 8;

            OnMixingFunction = null;


            if (MixCfg.Interpolate)
            {
                if (MixCfg.Style == RenderingStyle.Surround)
                    OnMixingFunction += new MixingFunctionHandler(MixSurroundInterp);
                else if (MixCfg.Style == RenderingStyle.Stereo)
                    OnMixingFunction += new MixingFunctionHandler(MixStereoInterp);
                else
                    OnMixingFunction += new MixingFunctionHandler(MixMonoInterp);

            }
            else
            {
                if (MixCfg.Style == RenderingStyle.Surround)
                    OnMixingFunction += new MixingFunctionHandler(MixSurroundNormal);
                else if (MixCfg.Style == RenderingStyle.Stereo)
                    OnMixingFunction += new MixingFunctionHandler(MixStereoNormal);
                else
                    OnMixingFunction += new MixingFunctionHandler(MixMonoNormal);
            }

            samplesthatfit = (short)TICKLSIZE;

            if (MixCfg.Style != RenderingStyle.Mono)
                samplesthatfit >>= 1;
            TICKLEFT = 0;
        }

        protected internal virtual void SampleMix(byte[] srce, int[] dest, int dest_offset, int index, int increment, short todo)
        {
            OnMixingFunction?.Invoke(srce, dest, dest_offset, index, increment, todo);

        }


        public virtual void VC_PlayStop()
        {
        }


        public virtual bool VC_Init()
        {
            return true;
        }


        public virtual void VC_Exit()
        {
        }


        public virtual void VC_VoiceSetVolume(short voice, short vol)
        {
            // protect against clicks if volume variation is too high
            if (Math.Abs((int)vinf[voice].Vol - (int)vol) > 32)
                vinf[voice].RampVol = CLICK_BUFFER;
            vinf[voice].Vol = vol;

        }


        public virtual void VC_VoiceSetFrequency(short voice, int frq)
        {
            vinf[voice].Frq = frq;
        }


        public virtual void VC_VoiceSetPanning(short voice, short pan)
        {
            // protect against clicks if panning variation is too high
            if (Math.Abs((int)vinf[voice].Pan - (int)pan) > 48)
                vinf[voice].RampVol = CLICK_BUFFER;
            vinf[voice].Pan = pan;
        }


        public virtual void VC_VoicePlay(short voice, int handle, int start, int size, int reppos, int repend, SampleFormats flags)
        {
            if (start >= size)
                return;

            if ((flags & (SampleFormats.SF_LOOP)) != 0 && repend > size)
                repend = size;

            vinf[voice].Flags = flags;
            vinf[voice].Handle = handle;
            vinf[voice].Start = start;
            vinf[voice].Size = size;
            vinf[voice].Reppos = reppos;
            vinf[voice].Repend = repend;
            vinf[voice].Kick = true;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void TickHandler()
        {
            // play 1 tick of the module
            OnTickHandler?.Invoke();

            var bpm = OnBPMRequest?.Invoke() ?? 0;                

            MD_SetBPM(bpm);
        }

        public virtual void MD_SetBPM(short bpm)
        {
            if (bpm < 0)
                bpm = (short)(bpm + ((-bpm / 256) + 1) * 256);
            BPM = (short)(bpm % 256);
        }

        private int nLeftNR = 0;
        private int nRightNR = 0;

        /// <summary>
        /// Do Noise Reduction in Stereo
        /// </summary>
        /// <param name="srce"></param>
        /// <param name="count"></param>
        public void MixLowPass_Stereo(int[] srce, int count)
        {
            int n1 = nLeftNR;
            int n2 = nRightNR;

            int nr = count;
            int idx = 0;
            while (nr-- > 0)
            {
                int vnr = srce[idx] >> 1;
                srce[idx] = vnr + n1;
                n1 = vnr;
                vnr = srce[idx + 1] >> 1;
                srce[idx + 1] = vnr + n2;
                n2 = vnr;
                idx += 2;
            }
            nLeftNR = n1;
            nRightNR = n2;
        }

        /// <summary>
        /// Do Noise Reduction in Mono
        /// </summary>
        /// <param name="srce"></param>
        /// <param name="count"></param>
        public void MixLowPass_Mono(int[] srce, int count)
        {
            int n1 = nLeftNR;

            int nr = count;
            int idx = 0;
            while (nr-- > 0)
            {
                int vnr = srce[idx] >> 1;
                srce[idx] = vnr + n1;
                n1 = vnr;
                idx++;
            }
            nLeftNR = n1;
        }

        private int RVRindex = 0;

        /// <summary>
        /// Do Reverb
        /// </summary>
        /// <param name="srce"></param>
        /// <param name="count"></param>
        public void MixReverb_Normal(int[] srce, int count)
        {
            int speedup;
            int ReverbPct;
            int ptrIdx = 0;

            int loc1, loc2, loc3, loc4;
            int loc5, loc6, loc7, loc8;

            ReverbPct = 58 + (MixCfg.Reverb << 2);
            loc1 = RVRindex % RVc1;
            loc2 = RVRindex % RVc2;
            loc3 = RVRindex % RVc3;
            loc4 = RVRindex % RVc4;
            loc5 = RVRindex % RVc5;
            loc6 = RVRindex % RVc6;
            loc7 = RVRindex % RVc7;
            loc8 = RVRindex % RVc8;

            while ((count--) > 0)
            {

                // Compute the left channel echo buffers
                speedup = srce[ptrIdx] >> 3;

                //RVbufL##n [loc##n ]=speedup+((ReverbPct*RVbufL##n [loc##n ])>>7)
                RVbufL1[loc1] = speedup + ((ReverbPct * RVbufL1[loc1]) >> 7);
                RVbufL2[loc2] = speedup + ((ReverbPct * RVbufL1[loc2]) >> 7);
                RVbufL3[loc3] = speedup + ((ReverbPct * RVbufL1[loc3]) >> 7);
                RVbufL4[loc4] = speedup + ((ReverbPct * RVbufL1[loc4]) >> 7);
                RVbufL5[loc5] = speedup + ((ReverbPct * RVbufL1[loc5]) >> 7);
                RVbufL6[loc6] = speedup + ((ReverbPct * RVbufL1[loc6]) >> 7);
                RVbufL7[loc7] = speedup + ((ReverbPct * RVbufL1[loc7]) >> 7);
                RVbufL8[loc8] = speedup + ((ReverbPct * RVbufL1[loc8]) >> 7);


                //Prepare to compute actual finalized data
                RVRindex++;

                loc1 = RVRindex % RVc1;
                loc2 = RVRindex % RVc2;
                loc3 = RVRindex % RVc3;
                loc4 = RVRindex % RVc4;
                loc5 = RVRindex % RVc5;
                loc6 = RVRindex % RVc6;
                loc7 = RVRindex % RVc7;
                loc8 = RVRindex % RVc8;

                // left channel
                srce[ptrIdx++] += RVbufL1[loc1] - RVbufL2[loc2]
                    + RVbufL3[loc3] - RVbufL4[loc4]
                    + RVbufL5[loc5] - RVbufL6[loc6]
                    + RVbufL7[loc7] - RVbufL8[loc8];
            }

        }

        void MixReverb_Stereo(int[] srce, int count)
        {
            int speedup;
            int ReverbPct;
            int ptrIdx = 0;

            int loc1, loc2, loc3, loc4;
            int loc5, loc6, loc7, loc8;

            ReverbPct = 92 + (MixCfg.Reverb << 1);

            loc1 = RVRindex % RVc1;
            loc2 = RVRindex % RVc2;
            loc3 = RVRindex % RVc3;
            loc4 = RVRindex % RVc4;
            loc5 = RVRindex % RVc5;
            loc6 = RVRindex % RVc6;
            loc7 = RVRindex % RVc7;
            loc8 = RVRindex % RVc8;

            while ((count--) > 0)
            {
                // Compute the left channel echo buffers

                speedup = srce[ptrIdx] >> 3;

                RVbufL1[loc1] = speedup + ((ReverbPct * RVbufL1[loc1]) >> 7);
                RVbufL2[loc2] = speedup + ((ReverbPct * RVbufL2[loc2]) >> 7);
                RVbufL3[loc3] = speedup + ((ReverbPct * RVbufL3[loc3]) >> 7);
                RVbufL4[loc4] = speedup + ((ReverbPct * RVbufL4[loc4]) >> 7);
                RVbufL5[loc5] = speedup + ((ReverbPct * RVbufL5[loc5]) >> 7);
                RVbufL6[loc6] = speedup + ((ReverbPct * RVbufL6[loc6]) >> 7);
                RVbufL7[loc7] = speedup + ((ReverbPct * RVbufL7[loc7]) >> 7);
                RVbufL8[loc8] = speedup + ((ReverbPct * RVbufL8[loc8]) >> 7);


                /* Compute the right channel echo buffers */
                speedup = srce[ptrIdx + 1] >> 3;


                RVbufR1[loc1] = speedup + ((ReverbPct * RVbufR1[loc1]) >> 7);
                RVbufR2[loc2] = speedup + ((ReverbPct * RVbufR2[loc2]) >> 7);
                RVbufR3[loc3] = speedup + ((ReverbPct * RVbufR3[loc3]) >> 7);
                RVbufR4[loc4] = speedup + ((ReverbPct * RVbufR4[loc4]) >> 7);
                RVbufR5[loc5] = speedup + ((ReverbPct * RVbufR5[loc5]) >> 7);
                RVbufR6[loc6] = speedup + ((ReverbPct * RVbufR6[loc6]) >> 7);
                RVbufR7[loc7] = speedup + ((ReverbPct * RVbufR7[loc7]) >> 7);
                RVbufR8[loc8] = speedup + ((ReverbPct * RVbufR8[loc8]) >> 7);


                /* Prepare to compute actual finalized data */

                RVRindex++;

                loc1 = RVRindex % RVc1;
                loc2 = RVRindex % RVc2;
                loc3 = RVRindex % RVc3;
                loc4 = RVRindex % RVc4;
                loc5 = RVRindex % RVc5;
                loc6 = RVRindex % RVc6;
                loc7 = RVRindex % RVc7;
                loc8 = RVRindex % RVc8;


                /* left channel then right channel */

                srce[ptrIdx++] += RVbufL1[loc1] - RVbufL2[loc2] +
                     RVbufL3[loc3] - RVbufL4[loc4] +
                     RVbufL5[loc5] - RVbufL6[loc6] +
                     RVbufL7[loc7] - RVbufL8[loc8];


                srce[ptrIdx++] += RVbufR1[loc1] - RVbufR2[loc2] +
                    RVbufR3[loc3] - RVbufR4[loc4] +
                    RVbufR5[loc5] - RVbufR6[loc6] +
                    RVbufR7[loc7] - RVbufR8[loc8];

            }

        }

    }
}