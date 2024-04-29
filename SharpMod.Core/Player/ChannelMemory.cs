using SharpMod.Song;

namespace SharpMod.Player
{
    public class ChannelMemory
    {

        public Instrument Instrument
        {
            get;
            set;
        }


        public Sample Sample
        {
            get;
            set;
        }


        /// <summary>
        /// fading volume
        /// </summary>
        public int FadeVol
        {
            get;
            set;
        }



        public EnvPr VolEnv
        {
            get;
            set;
        }


        public EnvPr PanEnv
        {
            get;
            set;
        }


        /// <summary>
        /// if true=key is pressed.
        /// </summary>
        public bool KeyOn
        {
            get;
            set;
        }

        /// <summary>
        /// if true=sample has to be restarted 
        /// </summary>
        public bool Kick
        {
            get;
            set;
        }

        /// <summary>
        ///  which sample number (0-31)
        /// </summary>
        public short SampleNumber
        {
            get;
            set;
        }

        /// <summary>
        /// which sample-handle
        /// </summary>
        public int Handle
        {
            get;
            set;
        }

        /// <summary>
        /// The start byte index in the sample
        /// </summary>
        public int Start
        {
            get;
            set;
        }

        /// <summary>
        /// panning position
        /// </summary>
        public short Panning
        {
            get;
            set;
        }

        /// <summary>
        /// panslide speed
        /// </summary>
        public short PanSlideSpd
        {
            get;
            set;
        }

        /// <summary>
        /// amiga volume (0 t/m 64) to play the sample at
        /// </summary>
        public sbyte Volume
        {
            get;
            set;
        }

        /// <summary>
        /// period to play the sample at
        /// </summary>
        public int Period
        {
            get;
            set;
        }

        // You should not have to use the values
        // below in the player routine



        public sbyte Transpose
        {
            get;
            set;
        }


        public short Note
        {
            get;
            set;
        }


        public short OwnPer
        {
            get;
            set;
        }

        public short OwnVol
        {
            get;
            set;
        }

        /// <summary>
        /// row currently playing on this channel
        /// </summary>
        public short[] Row
        {
            get;
            set;
        }


        public int RowPos
        {
            get;
            set;
        }

        /// <summary>
        /// retrig value (0 means don't retrig) 
        /// </summary>
        public sbyte Retrig
        {
            get;
            set;
        }

        /// <summary>
        /// what finetune to use
        /// </summary>
        public int C2spd
        {
            get;
            set;
        }


        /// <summary>
        /// tmp volume
        /// </summary>
        public sbyte TmpVolume
        {
            get;
            set;
        }


        /// <summary>
        /// tmp period 
        /// </summary>
        public int TmpPeriod
        {
            get;
            set;
        }

        /// <summary>
        /// period to slide to (with effect 3 or 5)
        /// </summary>
        public int WantedPeriod
        {
            get;
            set;
        }


        public int SlideSpeed
        {
            get;
            set;
        }

        /// <summary>
        /// noteslide speed (toneportamento)
        /// </summary>
        public int PortSpeed
        {
            get;
            set;
        }

        /// <summary>
        /// s3m tremor (effect I) counter 
        /// </summary>
        public short S3mTremor
        {
            get;
            set;
        }

        /// <summary>
        /// s3m tremor ontime/offtime
        /// </summary>
        public short S3mTrOnOff
        {
            get;
            set;
        }

        /// <summary>
        /// last used volslide
        /// </summary>
        public short S3mVolSlide
        {
            get;
            set;
        }

        /// <summary>
        /// last used retrig speed 
        /// </summary>
        public short S3mRtgSpeed
        {
            get;
            set;
        }

        /// <summary>
        /// last used retrig slide
        /// </summary>
        public short S3mRtgSlide
        {
            get;
            set;
        }

        /// <summary>
        /// glissando (0 means off) 
        /// </summary>
        public short Glissando
        {
            get;
            set;
        }

        public short WaveControl
        {
            get;
            set;
        }

        /// <summary>
        /// current vibrato position
        /// </summary>
        public sbyte VibPos
        {
            get;
            set;
        }

        /// <summary>
        /// current vibrato speed
        /// </summary>
        public short VibSpd
        {
            get;
            set;
        }

        /// <summary>
        /// current vibrato depth
        /// </summary>
        public short VibDepth
        {
            get;
            set;
        }

        /// <summary>
        /// current tremolo position
        /// </summary>
        public sbyte TrmPos
        {
            get;
            set;
        }

        /// <summary>
        /// current tremolo speed
        /// </summary>
        public short TrmSpd
        {
            get;
            set;
        }

        /// <summary>
        /// current tremolo depth
        /// </summary>
        public short TrmDepth
        {
            get;
            set;
        }

        /// <summary>
        /// last used sample-offset (effect 9)
        /// </summary>
        public int SampleOffset
        {
            get;
            set;
        }

        public ChannelMemory()
        {
            VolEnv = new EnvPr();
            PanEnv = new EnvPr();
        }
    }
}