using SharpMod.Mixer;
using System;

namespace SharpMod.DSP
{
    public class AudioProcessor
    {
        public delegate void CurrentSampleChangedHandler(int[] leftSample, int[] rightSample);
        public event CurrentSampleChangedHandler OnCurrentSampleChanged;

        private readonly int desiredBufferSize;
        private readonly long waitForNanos;

        private long internalFramePosition;
        private readonly object locker = new object();

        public int sampleBufferSize
        {
            get { return desiredBufferSize * 2; }
        }
        private int[] sampleBuffer;
        private int channels;
        private int currentWritePosition;
        private readonly ProcessorTask _processor;


        private sealed class ProcessorTask
        {
            private readonly AudioProcessor me;
            private readonly int[] leftBuffer;
            private readonly int[] rightBuffer;


            public ProcessorTask(AudioProcessor parent)
            {
                me = parent;
                leftBuffer = new int[me.desiredBufferSize];
                rightBuffer = new int[me.desiredBufferSize];

            }

            ///		
            ///		 <summary> *  </summary>
            ///		 * <seealso cref= java.lang.Runnable#run() </seealso>
            ///		 
            public void Run()
            {
                int currentReadPosition = (int)(me.internalFramePosition * me.channels % me.sampleBufferSize);
                for (int i = 0; i < me.desiredBufferSize; i++)
                {
                    if (currentReadPosition >= me.sampleBufferSize)
                        currentReadPosition = 0;
                    if (me.channels == 2)
                    {
                        leftBuffer[i] = me.sampleBuffer[currentReadPosition++];
                        rightBuffer[i] = me.sampleBuffer[currentReadPosition++];
                    }
                    else
                    {
                        leftBuffer[i] = rightBuffer[i] = me.sampleBuffer[currentReadPosition++];
                    }
                }

                me.OnCurrentSampleChanged?.Invoke(leftBuffer, rightBuffer);
            }
        }
        ///	
        ///	 <summary> * Constructor for AudioProcessor </summary>
        ///	 * <param name="desiredBufferSize"> </param>
        ///	 * <param name="desiredFPS"> </param>
        ///	 
        public AudioProcessor(int desiredBufferSize, int desiredFPS)
            : base()
        {
            this.desiredBufferSize = desiredBufferSize;
            this.waitForNanos = 1000000000L / (long)desiredFPS;
            _processor = new ProcessorTask(this);
        }
        ///	
        ///	 <summary> * Constructor for AudioProcessor </summary>
        ///	 
        public AudioProcessor()
            : this(1024, 70)
        {
        }

        public void Run()
        {
            _processor.Run();
        }



        ///	
        ///	 * <param name="internalFramePosition"> the internalFramePosition to set
        ///	 * This is the amount of samples written </param>
        ///	 
        public virtual long InternalFramePosition
        {
            set
            {
                this.internalFramePosition = value;
            }
        }

        ///	
        ///	 <summary> * @since 29.09.2007 </summary>
        ///	 * <param name="sourceDataLine"> </param>
        ///	 * <param name="sampleBufferSize"> </param>
        ///	 
        public virtual void initializeProcessor(ChannelsMixer mixer)
        {
            this.channels = mixer.MixCfg.Style == SharpMod.Player.RenderingStyle.Mono ? 1 : 2;
            this.sampleBuffer = new int[this.sampleBufferSize];
            this.currentWritePosition = 0;
            this.internalFramePosition = 0;
        }


        ///	
        ///	 <summary> * @since 29.09.2007 </summary>
        ///	 * <param name="newSampleData"> </param>
        ///	 * <param name="offset"> </param>
        ///	 * <param name="length"> </param>
        ///	 
        public virtual void WriteSampleData(int[] newSampleData, int offset, int length)
        {
            try
            {
                lock (locker)
                {
                    if (currentWritePosition + length >= sampleBufferSize)
                    {
                        int rest = sampleBufferSize - currentWritePosition;
                        Array.Copy(newSampleData, offset, sampleBuffer, currentWritePosition, rest);
                        //TODO : there is a bug here...
                        currentWritePosition = length - rest;
                        Array.Copy(newSampleData, offset + rest, sampleBuffer, 0, currentWritePosition);
                    }
                    else
                    {
                        Array.Copy(newSampleData, offset, sampleBuffer, currentWritePosition, length);
                        currentWritePosition += length;
                    }
                }
            }
            catch
            {
                //do nothing
            }
        }

        ///	
        ///	 <summary> * @since 29.09.2007 </summary>
        ///	 * <param name="newSampleData"> </param>
        ///	 
        public virtual void writeSampleData(int[] newSampleData)
        {
            WriteSampleData(newSampleData, 0, newSampleData.Length);
        }
    }

}