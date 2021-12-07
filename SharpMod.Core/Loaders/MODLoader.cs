using System;
using System.IO;
using SharpMod.Exceptions;
using SharpMod;
using SharpMod.IO;
using SharpMod.UniTracker;
using SharpMod.Song;

namespace SharpMod.Loaders
{	
    /// <summary>
    /// Generic MOD loader
    /// Old (amiga) noteinfo:
	///	
	///	_____byte 1_____   byte2_    _____byte 3_____   byte4_
	///	/                \ /      \  /                \ /      \
	///	0000          0000-00000000  0000          0000-00000000
	///	
	///	Upper four    12 bits for    Lower four    Effect command.
	///	bits of sam-  note period.   bits of sam-
	///	ple number.                  ple number.
    /// </summary>
	public class MODLoader:ILoader
	{
        public event AllocPatternsHandler AllocPatterns;
        public event AllocTracksHandler AllocTracks;
        public event AllocInstrumentsHandler AllocInstruments;
        public event AllocSamplesHandler AllocSamples;

        private UniTrk _uniTrack;
        private SongModule _module;
        /*private UniMod _uniMod;
        private Stream _modStream;*/

		protected internal MODULEHEADER mh; /* raw as-is module header */
		protected internal MODNOTE[] patbuf;
		
		internal const int MODULEHEADERSIZE = 1084;
		
		internal const System.String protracker = "Protracker";		
		internal const System.String startracker = "Startracker";		
		internal const System.String fasttracker = "Fasttracker";		
		internal const System.String ins15tracker = "15-instrument";		
		internal const System.String oktalyzer = "Oktalyzer";		
		internal const System.String taketracker = "TakeTracker";
		
		//internal MODTYPE[] modtypes = new MODTYPE[]{new MODTYPE("M.K.", 4, protracker), new MODTYPE("M!K!", 4, protracker), new MODTYPE("FLT4", 4, startracker), new MODTYPE("4CHN", 4, fasttracker), new MODTYPE("6CHN", 6, fasttracker), new MODTYPE("8CHN", 8, fasttracker), new MODTYPE("CD81", 8, oktalyzer), new MODTYPE("OKTA", 8, oktalyzer), new MODTYPE("16CN", 16, taketracker), new MODTYPE("32CN", 32, taketracker), new MODTYPE("    ", 4, ins15tracker)};
		internal static MODTYPE[] modtypes; // = new MODTYPE[]{new MODTYPE("M.K.", 4, protracker), new MODTYPE("M!K!", 4, protracker), new MODTYPE("FLT4", 4, startracker), new MODTYPE("4CHN", 4, fasttracker), new MODTYPE("6CHN", 6, fasttracker), new MODTYPE("8CHN", 8, fasttracker), new MODTYPE("CD81", 8, oktalyzer), new MODTYPE("OKTA", 8, oktalyzer), new MODTYPE("16CN", 16, taketracker), new MODTYPE("32CN", 32, taketracker), new MODTYPE("    ", 4, ins15tracker)};
		
		internal static short[] npertab = new short[]
        {1712, 1616, 1524, 1440, 1356, 1280, 1208, 1140, 1076, 1016, 960, 906,
            856, 808, 762, 720, 678, 640, 604, 570, 538, 508, 480, 453, 428, 404,
            381, 360, 339, 320, 302, 285, 269, 254, 240, 226, 214, 202, 190, 180,
            170, 160, 151, 143, 135, 127, 120, 113, 107, 101, 95, 90, 85, 80, 75,
            71, 67, 63, 60, 56};

        public String LoaderType
        {
            get
            {
                return "Standard module";
            }
        }

        public String LoaderVersion
        {
            get
            {
                return "Portable MOD loader v0.11";
            }
        }

        public UniTrk UniTrack
        {
            get
            {
                return _uniTrack;
            }
            set
            {
                _uniTrack = value;
            }
        }

       /* public UniMod UniModule
        {
            get
            {
                return _uniMod;
            }
            set
            {
                _uniMod = value;
            }
        }

       


        public System.IO.Stream ModStream
        {
            get
            {
                return _modStream;
            }
            set
            {
                _modStream = value;
            }
        }*/

        public ModBinaryReader Reader{ get; set; }

		public MODLoader()
		{
			mh = null;
			patbuf = null;
			modtypes = new MODTYPE[]{new MODTYPE("M.K.", 4, protracker), new MODTYPE("M!K!", 4, protracker), new MODTYPE("FLT4", 4, startracker), new MODTYPE("4CHN", 4, fasttracker), new MODTYPE("6CHN", 6, fasttracker), new MODTYPE("8CHN", 8, fasttracker), new MODTYPE("CD81", 8, oktalyzer), new MODTYPE("OKTA", 8, oktalyzer), new MODTYPE("16CN", 16, taketracker), new MODTYPE("32CN", 32, taketracker), new MODTYPE("    ", 4, ins15tracker)};
           
		}
		
		public  bool Test()
		{
			try
			{
                //Reader = new ModBinaryReader(ModStream);
				int t, i;
				
				byte[] id = new byte[4];
				
				//MmIO.Instance.Seek(this.ModStream, MODULEHEADERSIZE - 4, SeekEnum.SEEK_SET);
                Reader.Seek(MODULEHEADERSIZE - 4, SeekOrigin.Begin);
                
                if (Reader.Read(id, 0, 4) != 4)
					return false;
				
				/* find out which ID string */
				
				for (t = 0; t < 10; t++)
				{
					for (i = 0; i < 4; i++)
						if (id[i] != modtypes[t].id[i])
							break;
					if (i == 4)
						return true;
					//if(!memcmp(id,modtypes[t].id,4)) return 1;
				}
				
				return false;
			}
			catch (System.IO.IOException)
			{
				return false;
			}
		}
		
		
		public  bool Init(SongModule module )
		{
            _module = module;

			patbuf = null;
			
			mh = new MODULEHEADER();
			
			mh.songlength = (short) (mh.magic1 = 0);
			/*for (i = 0; i < 20; i++)
				mh.songname[i] = 0;*/

            mh.positions.Initialize();
			/*for (i = 0; i < 128; i++)
				mh.positions[i] = 0;*/
            mh.magic2.Initialize();
			/*for (i = 0; i < 4; i++)
				mh.magic2[i] = 0;*/
			
			
			for (int i = 0; i < 31; i++)
			{
				mh.samples[i].length = mh.samples[i].reppos = mh.samples[i].replen = 0;
				mh.samples[i].finetune = (short) (mh.samples[i].volume = 0);
				/*for (j = 0; j < 22; j++)
					mh.samples[i].samplename[j] = 0;*/
			}
			
			return true;
		}
		
		
		public  void  Cleanup()
		{
			if (mh != null)
				mh = null;
			if (patbuf != null)
				patbuf = null;
		}
		
		public virtual void  ConvertNote(MODNOTE n)
		{
			short instrument, effect, effdat, note;
			int period;
			
			/* extract the various information from the 4 bytes that
			make up a single note */
			
			instrument = (short) ((n.a & 0x10) | (n.c >> 4));
			period = (((int) n.a & 0xf) << 8) + n.b;
			effect = (short) (n.c & 0xf);
			effdat = n.d;
			
			/* Convert the period to a note number */
			
			note = 0;
			if (period != 0)
			{
				for (note = 0; note < 60; note++)
				{
					if (period >= npertab[note])
						break;
				}
				note++;
				if (note == 61)
					note = 0;
			}
			
			if (instrument != 0)
			{
				this.UniTrack.UniInstrument((short) (instrument - 1));
			}
			
			if (note != 0)
			{
                this.UniTrack.UniNote((short)(note + 23));
			}

            this.UniTrack.UniPTEffect(effect, effdat);
		}
		
		
		public virtual short[] ConvertTrack(MODNOTE[] n, int which_track)
		{
			int t;
			int idx_n = 0;

            this.UniTrack.UniReset();
			for (t = 0; t < 64; t++)
			{
				ConvertNote(n[idx_n + which_track]);
                this.UniTrack.UniNewline();
				idx_n += this._module.ChannelsCount;
			}
            return this.UniTrack.UniDup();
		}
		
		/// <summary>
		/// Loads all patterns of a modfile and converts them into the
		/// 3 byte format.
		/// </summary>
		/// <returns></returns>
		public virtual bool ML_LoadPatterns(int patternsCount)
		{
			int t, s = 0;

            this._module.Patterns = new System.Collections.Generic.List<Pattern>(patternsCount);
			
			
			// Allocate temporary buffer for loading and converting the patterns
			patbuf = new MODNOTE[64 * this._module.ChannelsCount];

            for (t = 0; t < 64 * this._module.ChannelsCount; t++)
			{
				patbuf[t] = new MODNOTE();
				patbuf[t].a = (short) (patbuf[t].b = (short) (patbuf[t].c = (short) (patbuf[t].d = 0)));
			}


            for (t = 0; t < patternsCount; t++)
			{
                if (this.AllocPatterns != null && !AllocPatterns(_module, t, 64))
                    return false;

                if (this.AllocTracks != null && !AllocTracks(this._module.Patterns[t], _module.ChannelsCount))
                    return false;
                // Load the pattern into the temp buffer and convert it
				for (s = 0; s < (int) (64 * this._module.ChannelsCount); s++)
				{
                    patbuf[s].a = Reader.ReadUByte();
                    patbuf[s].b = Reader.ReadUByte();
                    patbuf[s].c = Reader.ReadUByte();
                    patbuf[s].d = Reader.ReadUByte();
				}

                for (s = 0; s < this._module.ChannelsCount; s++)
				{
					if ((this._module.Patterns[t].Tracks[s].UniTrack = ConvertTrack(patbuf, s)) == null)
						return false;
				}
			}
			
			return true;
		}
		
		
		public  bool Load()
		{
			try
			{				
				int modtype;
                int inst_num;
                int smpinfo_num;
				
				// try to read module header
                mh.songname = Reader.ReadString(20);
				
				for (int t = 0; t < 31; t++)
				{					
					mh.samples[t].samplename = Reader.ReadString( 22);
                    mh.samples[t].length = Reader.ReadMotorolaUWord();
                    mh.samples[t].finetune = Reader.ReadUByte();
                    mh.samples[t].volume = Reader.ReadUByte();
                    mh.samples[t].reppos = Reader.ReadMotorolaUWord();
                    mh.samples[t].replen = Reader.ReadMotorolaUWord();
				}

                mh.songlength = (short)Reader.ReadUByte();
                mh.magic1 = (short)Reader.ReadUByte();

                Reader.ReadSBytes(mh.positions, 128);
                Reader.ReadSBytes(mh.magic2, 4);
				
                if(Reader.isEOF())
				{				
                    throw new SharpModException(SharpModExceptionResources.ERROR_LOADING_HEADER);
				}
				
				// find out which ID string				
				for (modtype = 0; modtype < 10; modtype++)
				{
					int pos;
					for (pos = 0; pos < 4; pos++)
						if (mh.magic2[pos] != modtypes[modtype].id[pos])
							break;
					if (pos == 4)
						break;					
				}
				
				if (modtype == 10)
				{					
					// unknown modtype 				
                    throw new SharpModException(SharpModExceptionResources.ERROR_NOT_A_MODULE);
				}
				
				// set module variables				
				this._module.InitialSpeed = 6;
                this._module.InitialTempo = 125;
                // get number of channels
                this._module.ChannelsCount = modtypes[modtype].channels;
                // get ascii type of mod 
                this._module.ModType = new System.String(modtypes[modtype].name.ToCharArray()); 
                // make a cstr this.UniModule songname
                this._module.SongName = mh.songname;
                // copy the songlength
                this._module.Positions = new System.Collections.Generic.List<int>(mh.songlength);
				
				// copy the position array
				for (int t = 0; t < 128; t++)
				{
                    this._module.Positions.Add(mh.positions[t]);
                    if (t >= mh.songlength)
                        break;
				}
				
				// Count the number of patterns
                //this._module.NumPat = 0;
                int patCount = 0;
                for (int t = 0; t < mh.songlength/*128*/; t++)
                {
                    /* <-- BUGFIX... have to check ALL positions */
                    if (this._module.Positions[t] > patCount)
                    {
                        patCount = this._module.Positions[t];
                    }
                }
                patCount++;

				//this.UniModule.NumPat++;
				//this.UniModule.NumTrk = (short) (this.UniModule.NumPat * this.UniModule.NumChn);
				
				//Finally, init the sampleinfo structures				
				//this._module.NumIns = 31;
				
                if (AllocInstruments != null && !AllocInstruments(_module,31))
                    return false;
				
				smpinfo_num = 0; /* init source pointer */
				inst_num = 0; /* init dest pointer */
				
				for (int t = 0; t < /*this.UniModule.NumIns*/ 31; t++)
				{
					
					this._module.Instruments[inst_num].NumSmp = 1;
					
                    if (AllocSamples != null && !AllocSamples(this._module.Instruments[inst_num]))
                        return false;
					
					//q=this.UniModule.instruments[inst_num].samples;
					
					//convert the samplename
                    this._module.Instruments[inst_num].InsName = mh.samples[smpinfo_num].samplename;
					
					//init the sampleinfo variables and convert the size pointers to longword format
                    this._module.Instruments[inst_num].Samples[0].C2Spd = Helper.FineTune[mh.samples[smpinfo_num].finetune & 0xf];
                    this._module.Instruments[inst_num].Samples[0].Volume = mh.samples[smpinfo_num].volume;
                    this._module.Instruments[inst_num].Samples[0].LoopStart = (int)mh.samples[smpinfo_num].reppos << 1;
                    this._module.Instruments[inst_num].Samples[0].LoopEnd = this._module.Instruments[inst_num].Samples[0].LoopStart + ((int)mh.samples[smpinfo_num].replen << 1);
                    this._module.Instruments[inst_num].Samples[0].Length = (int)mh.samples[smpinfo_num].length << 1;
                    this._module.Instruments[inst_num].Samples[0].SeekPos = 0;

                    this._module.Instruments[inst_num].Samples[0].Flags = (SampleFormatFlags.SF_SIGNED);
					if (mh.samples[smpinfo_num].replen > 1)
                        this._module.Instruments[inst_num].Samples[0].Flags |= (SampleFormatFlags.SF_LOOP);
					
					//fix replen if repend>length					
                    if (this._module.Instruments[inst_num].Samples[0].LoopEnd > this._module.Instruments[inst_num].Samples[0].Length)
                        this._module.Instruments[inst_num].Samples[0].LoopEnd = this._module.Instruments[inst_num].Samples[0].Length;

                    this._module.Instruments[inst_num].Samples[0].SampleRate = 8363;
                    //point to next source sampleinfo
                    smpinfo_num++;
                    //point to next destiny sampleinfo
					inst_num++; 
				}

                if (!ML_LoadPatterns(patCount))
					return false;
				return true;
			}
			catch (System.IO.IOException)
			{
				return false;
			}
		}

        
    }

    #region Internal Structs
    #region old
    public class MSAMPINFO
    {
        /* sample header as it appears in a module */
        public string samplename;
        public int length;
        public short finetune;
        public short volume;
        public int reppos;
        public int replen;

        public MSAMPINFO()
        {
            samplename = string.Empty;
        }
    }
    #endregion

    //	public struct MSAMPINFO
    //	{
    //		/* sample header as it appears in a module */
    //		public char[] samplename;
    //		public int length;
    //		public short finetune;
    //		public short volume;
    //		public int reppos;
    //		public int replen;
    //		
    //		public MSAMPINFO()
    //		{
    //			samplename = new char[22];
    //		}
    //	}

    public struct MODNOTE
    {
        public short a;
        public short b;
        public short c;
        public short d;
    }

    #region old
    //	public class MODNOTE
    //	{
    //		public short a, b, c, d;
    //	}
    #endregion

    //	public struct MODULEHEADER
    //	{
    //		/* verbatim module header */		
    //		public char[] songname; /* the songname.. */
    //		public MSAMPINFO[] samples; /* all sampleinfo */
    //		public short songlength; /* number of patterns used */
    //		public short magic1; /* should be 127 */
    //		public sbyte[] positions; /* which pattern to play at pos */
    //		public sbyte[] magic2;
    //		/* string "M.K." or "FLT4" or "FLT8" */
    //		public MODULEHEADER()
    //		{
    //			songname = new char[20];
    //			samples = new MSAMPINFO[31];
    //			int i;
    //			for (i = 0; i < 31; i++)
    //				samples[i] = new MSAMPINFO();
    //			positions = new sbyte[128];
    //			magic2 = new sbyte[4];
    //		}
    //	}

    public class MODULEHEADER
    {
        /* verbatim module header */
        public string songname; /* the songname.. */
        public MSAMPINFO[] samples; /* all sampleinfo */
        public short songlength; /* number of patterns used */
        public short magic1; /* should be 127 */
        public sbyte[] positions; /* which pattern to play at pos */
        public sbyte[] magic2;
        /* string "M.K." or "FLT4" or "FLT8" */
        public MODULEHEADER()
        {
            songname = string.Empty;
            samples = new MSAMPINFO[31];
            int i;
            for (i = 0; i < 31; i++)
                samples[i] = new MSAMPINFO();
            positions = new sbyte[128];
            magic2 = new sbyte[4];
        }
    }



    class MODTYPE
    {
        /* struct to identify type of module */
        public sbyte[] id;
        public short channels;
        public System.String name;
        //char *    name;
        public MODTYPE()
        {
            id = new sbyte[5];
        }
        public MODTYPE(System.String init_id, int init_chn, System.String init_name)
        {
            id = new sbyte[5];
            byte[] tmp;
            //SupportClass.GetSBytesFromString(init_id, 0, 4, ref id, 0);
            tmp = System.Text.UTF8Encoding.UTF8.GetBytes(init_id);
            Buffer.BlockCopy(tmp, 0, id, 0, 4);
            id[4] = 0; //'\x0000';

            channels = (short)init_chn;
            name = new System.String(init_name.ToCharArray());
        }
    }
    
    #endregion
}