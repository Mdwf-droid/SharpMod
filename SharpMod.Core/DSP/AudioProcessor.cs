using System;
using System.Collections.Generic;
using System.Threading;
using SharpMod.Mixer;

namespace SharpMod.DSP
{
    //using AudioFormat = javax.sound.sampled.AudioFormat;
    //using SourceDataLine = javax.sound.sampled.SourceDataLine;

    public class AudioProcessor
    {
        public delegate void CurrentSampleChangedHandler(int[] leftSample, int[] rightSample);
        public event CurrentSampleChangedHandler OnCurrentSampleChanged;
        private readonly object @lock = new object();

        private readonly int desiredBufferSize;
        private readonly long waitForNanos;

        private long internalFramePosition;
        private volatile bool useInternalCounter;
        private int _sampleBufferSize;
        private object locker = new object();

        public int sampleBufferSize
        {
            get { return desiredBufferSize*2; }
           // set { _sampleBufferSize = value; }
        }
        private int[] sampleBuffer;
        private int channels;
        private int currentWritePosition;
        private ProcessorTask _processor;
       

        private class ProcessorTask
        {
            private readonly AudioProcessor me;
            private readonly int[] leftBuffer;
            private readonly int[] rightBuffer;
            private readonly long nanoWait;
            

            public ProcessorTask(AudioProcessor parent)
            {
                this.me = parent;
                this.leftBuffer = new int[me.desiredBufferSize];
                this.rightBuffer = new int[me.desiredBufferSize];
                
                this.nanoWait = parent.waitForNanos;
            }
           
            ///		
            ///		 <summary> *  </summary>
            ///		 * <seealso cref= java.lang.Runnable#run() </seealso>
            ///		 
            public void run()
            {
                int currentReadPosition = (int)(((me.internalFramePosition * me.channels) % me.sampleBufferSize));
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

                if (me.OnCurrentSampleChanged != null)
                    me.OnCurrentSampleChanged(leftBuffer, rightBuffer);

                /*for (int i=0; i<me.callBacks.size(); i++)
                {
                    me.callBacks.get(i).currentSampleChanged(leftBuffer, rightBuffer);
                }
                */

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
            _processor.run();
        }

        ///	
        ///	 * <param name="useInternalCounter"> the useInternalCounter to set </param>
        ///	 
        public virtual bool UseInternalCounter
        {
            set
            {
                this.useInternalCounter = value;
            }
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
            //this.sampleBufferSize =  sourceDataLine.BufferSize;
            //this.sampleBufferSize = mixer.VC_TICKBUF.Length;
            this.sampleBuffer = new int[this.sampleBufferSize];
            this.currentWritePosition = 0;
            this.internalFramePosition = 0;
            this.useInternalCounter = false;
        }


        ///	
        ///	 <summary> * @since 29.09.2007 </summary>
        ///	 * <param name="newSampleData"> </param>
        ///	 * <param name="offset"> </param>
        ///	 * <param name="length"> </param>
        ///	 
        public virtual void writeSampleData(int[] newSampleData, int offset, int length)
        {
            /*lock (@lock)
            {*/
            try
            {
                lock (locker)
                {
                    if (currentWritePosition + length >= sampleBufferSize)
                    {
                        int rest = sampleBufferSize - currentWritePosition;
                        Array.Copy(newSampleData, offset, sampleBuffer, currentWritePosition, rest);
                        //TODO : there is a bug here...
                        Array.Copy(newSampleData, offset + rest, sampleBuffer, 0, currentWritePosition = length - rest);
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
                
               
            }
               
            //}
        }

        ///	
        ///	 <summary> * @since 29.09.2007 </summary>
        ///	 * <param name="newSampleData"> </param>
        ///	 
        public virtual void writeSampleData(int[] newSampleData)
        {
            writeSampleData(newSampleData, 0, newSampleData.Length);
        }
    }

}