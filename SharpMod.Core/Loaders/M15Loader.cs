using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpMod.UniTracker;
using SharpMod.Exceptions;
using SharpMod.Song;

namespace SharpMod.Loaders
{
    public class M15Loader: ILoader
    {
        private readonly short[] M15_npertab = new short[] { 1712, 1616, 1524, 1440, 1356, 1280, 1208, 1140, 1076, 1016, 960, 906, 856, 808, 762, 720, 678, 640, 604, 570, 538, 508, 480, 453, 428, 404, 381, 360, 339, 320, 302, 285, 269, 254, 240, 226, 214, 202, 190, 180, 170, 160, 151, 143, 135, 127, 120, 113, 107, 101, 95, 90, 85, 80, 75, 71, 67, 63, 60, 56 };

        // raw as-is module header
        private M15_MODULEHEADER mh; 
        private M15_MODNOTE[] patbuf;
        private SongModule _module;

        #region private methods
        private bool LoadModuleHeader(M15_MODULEHEADER mh)
        {
            try
            {

                int t;

                mh.songname = Reader.ReadString(20);

                for (t = 0; t < 15; t++)
                {                    
                    mh.samples[t].samplename = Reader.ReadString(22);
                    mh.samples[t].length = Reader.ReadMotorolaUWord();
                    mh.samples[t].finetune = Reader.ReadUByte();
                    mh.samples[t].volume = Reader.ReadUByte();
                    mh.samples[t].reppos = Reader.ReadMotorolaUWord();
                    mh.samples[t].replen = Reader.ReadMotorolaUWord();
                }

                mh.songlength = Reader.ReadUByte();
                mh.magic1 = Reader.ReadUByte();
                Reader.ReadSBytes(mh.positions, 128);

                return !Reader.isEOF();
            }
            catch (System.IO.IOException)
            {
                return false;
            }
        }


        private void M15_ConvertNote(M15_MODNOTE n)
        {
            short instrument, effect, effdat, note;
            int period;

            // extract the various information from the 4 bytes that make up a single note
            instrument = (short)((n.a & 0x10) | (n.c >> 4));
            period = (((int)n.a & 0xf) << 8) + n.b;
            effect = (short)(n.c & 0xf);
            effdat = n.d;

            // Convert the period to a note number
            note = 0;
            if (period != 0)
            {
                for (note = 0; note < 60; note++)
                {
                    if (period >= M15_npertab[note])
                        break;
                }
                note++;
                if (note == 61)
                    note = 0;
            }

            if (instrument != 0)
            {                
                this.UniTrack.UniInstrument((short)(instrument - 1));
            }

            if (note != 0)
            {             
                this.UniTrack.UniNote((short)(note + 23));
            }
                        
            this.UniTrack.UniPTEffect(effect, effdat);
        }



        private short[] M15_ConvertTrack(M15_MODNOTE[] n, int offset)
        {
            int t;

            //m_.MUniTrk.UniReset();
            this._uniTrk.UniReset();
            int n_ptr = offset;
            for (t = 0; t < 64; t++)
            {
                M15_ConvertNote(n[n_ptr]);
                this._uniTrk.UniNewline();
                //n_ptr += m_.MLoader.of.numchn;
                n_ptr += this._module.ChannelsCount;
            }
            return this._uniTrk.UniDup();//m_.MUniTrk.UniDup();
        }


        /// <summary>
        /// Loads all patterns of a modfile and converts them into the
        /// 3 byte format.
        /// </summary>
        /// <returns></returns>
        private bool M15_LoadPatterns(int patternsCount)
        {
            int s = 0;

            this._module.Patterns = new System.Collections.Generic.List<Pattern>(patternsCount);

            // Allocate temporary buffer for loading and converting the patterns                        
            patbuf = new M15_MODNOTE[64 * this._module.ChannelsCount];
            for (int t = 0; t < 64 * this._module.ChannelsCount; t++)
                patbuf[t] = new M15_MODNOTE();

            for (int t = 0; t < 64 * this._module.ChannelsCount; t++)
            {
                patbuf[t].a = (short)(patbuf[t].b = (short)(patbuf[t].c = (short)(patbuf[t].d = 0)));
            }

            for (int t = 0; t < patternsCount; t++)
            {
                if (this.AllocPatterns != null && !AllocPatterns(_module, t, 64))
                    return false;

                if (this.AllocTracks != null && !AllocTracks(this._module.Patterns[t], _module.ChannelsCount))
                    return false;
               
                // Load the pattern into the temp buffer and convert it
                for (s = 0; s < (64 * this._module.ChannelsCount); s++)
                {
                    patbuf[s].a = Reader.ReadUByte();
                    patbuf[s].b = Reader.ReadUByte();
                    patbuf[s].c = Reader.ReadUByte();
                    patbuf[s].d = Reader.ReadUByte();
                }

                for (s = 0; s < this._module.ChannelsCount; s++)
                {                    
                    if ((this._module.Patterns[t].Tracks[s].UniTrack = M15_ConvertTrack(patbuf, s)) == null)
                        return false;
                }
            }

            return true;
        }
        #endregion

        #region ILoader Members

        public event AllocPatternsHandler AllocPatterns;

        public event AllocTracksHandler AllocTracks;

        public event AllocInstrumentsHandler AllocInstruments;

        public event AllocSamplesHandler AllocSamples;

        public SharpMod.IO.ModBinaryReader Reader
        {
            get;
            set;
        }

        public string LoaderType
        {
            get { return "15-instrument module"; }
        }

        public string LoaderVersion
        {
            get { return "Portable MOD loader v0.11"; }
        }

        private UniTrk _uniTrk;
        public SharpMod.UniTracker.UniTrk UniTrack
        {
            get
            {
                return _uniTrk;
            }
            set
            {
                _uniTrk = value;
            }
        }

        public bool Init(SharpMod.Song.SongModule module)
        {
            this._module = module;
            int i;

            patbuf = null;
            
            mh = new M15_MODULEHEADER();

            mh.songlength = (short)(mh.magic1 = 0);
            
            for (i = 0; i < 128; i++)
                mh.positions[i] = 0;

            for (i = 0; i < 15; i++)
            {
                mh.samples[i].length = mh.samples[i].reppos = mh.samples[i].replen = 0;
                mh.samples[i].finetune = (short)(mh.samples[i].volume = (short)0);               
            }

            return true;
        }

        public bool Load()
        {          
           
            int inst_num, smpinfo_num;

            // try to read module header
            if (!LoadModuleHeader(mh))
            {
                throw new SharpModException(SharpModExceptionResources.ERROR_LOADING_HEADER);
            }

            // set module variables
            this._module.InitialSpeed = 6;
            this._module.InitialTempo = 125;
            //get number m_.MLoader.of channels
            this._module.ChannelsCount = 4;
            // get ascii type m_.MLoader.of mod
            this._module.ModType = new System.String("15-instrument".ToCharArray()); 
            //make a cstr m_.MLoader.of songname 
            this._module.SongName = mh.songname; //m_.MLoader.DupStr(mh.songname, 20); 
            //this._module.numpos = mh.songlength; /* copy the songlength */

            // copy the position array 
            for (int t = 0; t < 128; t++)
            {
                this._module.Positions.Add( mh.positions[t]);
            }

            // Count the number of patterns
            //this._module.NumPat = 0;
            int patCount = 0;
            for (int t = 0; t < 128; t++)
            {
                // <-- BUGFIX... have to check ALL positions
                if (this._module.Positions[t] > patCount)
                {
                    patCount = this._module.Positions[t];
                }
            }
            patCount++;

            // Finally, init the sampleinfo structures 
            // init source pointer
            smpinfo_num = 0; 
            // init dest pointer
            inst_num = 0; 
           
            if (AllocInstruments != null && !AllocInstruments(_module, 15))
                return false;
            for (int t = 0; t < 15; t++)
            {
                //m_.MLoader.of.instruments[inst_num].numsmp = 1;
                this._module.Instruments[inst_num].NumSmp = 1;

                if (AllocSamples != null && !AllocSamples(this._module.Instruments[inst_num]))
                    return false;

                //q=m_.MLoader.of.instruments[inst_num].samples;

                // convert the samplename
                //m_.MLoader.of.instruments[inst_num].insname = m_.MLoader.DupStr(mh.samples[smpinfo_num].samplename, 22);
                this._module.Instruments[inst_num].InsName = mh.samples[smpinfo_num].samplename;

                // init the sampleinfo variables and convert the size pointers to longword format
                this._module.Instruments[inst_num].Samples[0].C2Spd = Helper.FineTune[mh.samples[smpinfo_num].finetune & 0xf];
                this._module.Instruments[inst_num].Samples[0].Volume = mh.samples[smpinfo_num].volume;
                this._module.Instruments[inst_num].Samples[0].LoopStart = mh.samples[smpinfo_num].reppos;
                this._module.Instruments[inst_num].Samples[0].LoopEnd = this._module.Instruments[inst_num].Samples[0].LoopStart + ((int)mh.samples[smpinfo_num].replen << 1);
                this._module.Instruments[inst_num].Samples[0].Length = mh.samples[smpinfo_num].length << 1;
                this._module.Instruments[inst_num].Samples[0].SeekPos = 0;

                this._module.Instruments[inst_num].Samples[0].Flags = (SampleFormatFlags.SF_SIGNED);
                if (mh.samples[smpinfo_num].replen > 1)
                    this._module.Instruments[inst_num].Samples[0].Flags |= (SampleFormatFlags.SF_LOOP);
					
                // fix replen if repend>length 
                if (this._module.Instruments[inst_num].Samples[0].LoopEnd > this._module.Instruments[inst_num].Samples[0].Length)
                    this._module.Instruments[inst_num].Samples[0].LoopEnd = this._module.Instruments[inst_num].Samples[0].Length;

                // point to next source sampleinfo
                smpinfo_num++;
                // point to next destiny sampleinfo
                inst_num++;
            }

            if (!M15_LoadPatterns(patCount))
                return false;
            return true;
        }

        public bool Test()
        {
            int t;
            M15_MODULEHEADER mh = new M15_MODULEHEADER();

            if (!LoadModuleHeader(mh))
                return false;

            for (t = 0; t < 15; t++)
            {

                // all finetunes should be zero
                if (mh.samples[t].finetune != 0)
                    return false;

                // all volumes should be <=64
                if (mh.samples[t].volume > 64)
                    return false;
            }
            if (mh.magic1 > 127)
                return false;
            // and magic1 should be <128

            return true;
        }

        #endregion

        #region internal classes
        class M15_MSAMPINFO
        {
            // sample header as it appears in a module            
            internal string samplename;
            internal int length;
            internal short finetune;
            internal short volume;
            internal int reppos;
            internal int replen;

            public M15_MSAMPINFO()
            {
                //samplename = new sbyte[22];
            }
        }


        class M15_MODULEHEADER
        {
            // verbatim module header
            
            // the songname..
            internal string songname;
            // all sampleinfo
            internal M15_MSAMPINFO[] samples;
            // number of patterns used
            internal short songlength;
            // should be 127
            internal short magic1;
            // which pattern to play at pos
            internal sbyte[] positions;
            

            public M15_MODULEHEADER()
            {
                //songname = new sbyte[20];
                samples = new M15_MSAMPINFO[15];
                int i;
                for (i = 0; i < 15; i++)
                    samples[i] = new M15_MSAMPINFO();
                positions = new sbyte[128];
            }
        }


        class M15_MODNOTE
        {
            internal short a, b, c, d;
        }
	
        #endregion
    }
}
