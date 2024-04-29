
using SharpMod.Exceptions;
using SharpMod.IO;
using SharpMod.Song;
using SharpMod.UniTracker;
using System;
using System.Collections.Generic;
using System.IO;

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
    public class ModLoader : ILoader
    {
        public event AllocPatternsHandler AllocPatterns;
        public event AllocTracksHandler AllocTracks;
        public event AllocInstrumentsHandler AllocInstruments;
        public event AllocSamplesHandler AllocSamples;

        private SongModule _module;

        protected internal ModuleHeader mh; /* raw as-is module header */
        protected internal ModNote[] patbuf;

        internal const int MODULEHEADERSIZE = 1084;

        internal const System.String protracker = "Protracker";
        internal const System.String startracker = "Startracker";
        internal const System.String fasttracker = "Fasttracker";
        internal const System.String ins15tracker = "15-instrument";
        internal const System.String oktalyzer = "Oktalyzer";
        internal const System.String taketracker = "TakeTracker";

        internal readonly ModType[] modtypes;

        internal static readonly short[] npertab =
        [1712,
            1616,
            1524,
            1440,
            1356,
            1280,
            1208,
            1140,
            1076,
            1016,
            960,
            906,
            856,
            808,
            762,
            720,
            678,
            640,
            604,
            570,
            538,
            508,
            480,
            453,
            428,
            404,
            381,
            360,
            339,
            320,
            302,
            285,
            269,
            254,
            240,
            226,
            214,
            202,
            190,
            180,
            170,
            160,
            151,
            143,
            135,
            127,
            120,
            113,
            107,
            101,
            95,
            90,
            85,
            80,
            75,
            71,
            67,
            63,
            60,
            56];

        public String LoaderType => "Standard module";

        public String LoaderVersion => "Portable MOD loader v0.11";

        public UniTrk UniTrack { get; set; }


        public ModBinaryReader Reader { get; set; }

        public ModLoader()
        {
            mh = null;
            patbuf = null;
            modtypes = [new ModType("M.K.", 4, protracker), new ModType("M!K!", 4, protracker), new ModType("FLT4", 4, startracker), new ModType("4CHN", 4, fasttracker), new ModType("6CHN", 6, fasttracker), new ModType("8CHN", 8, fasttracker), new ModType("CD81", 8, oktalyzer), new ModType("OKTA", 8, oktalyzer), new ModType("16CN", 16, taketracker), new ModType("32CN", 32, taketracker), new ModType("    ", 4, ins15tracker)];

        }

        public bool Test()
        {
            try
            {
                int t, i;

                byte[] id = new byte[4];

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
                }

                return false;
            }
            catch (System.IO.IOException)
            {
                return false;
            }
        }


        public bool Init(SongModule module)
        {
            _module = module;

            patbuf = null;

            mh = new ModuleHeader();

            mh.SongLength = mh.Magic1 = 0;

            mh.Positions.Initialize();

            mh.Magic2.Initialize();


            for (int i = 0; i < 31; i++)
            {
                mh.Samples[i].Length = mh.Samples[i].RepPos = mh.Samples[i].RepLen = 0;
                mh.Samples[i].FineTune = mh.Samples[i].Volume = 0;

            }

            return true;
        }


        public void Cleanup()
        {
            if (mh != null)
                mh = null;
            if (patbuf != null)
                patbuf = null;
        }

        public virtual void ConvertNote(ModNote n)
        {
            short instrument, effect, effdat, note;
            int period;

            /* extract the various information from the 4 bytes that
			make up a single note */

            instrument = (short)((n.A & 0x10) | (n.C >> 4));
            period = (((int)n.A & 0xf) << 8) + n.B;
            effect = (short)(n.C & 0xf);
            effdat = n.D;

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
                UniTrack.UniInstrument((short)(instrument - 1));
            }

            if (note != 0)
            {
                UniTrack.UniNote((short)(note + 23));
            }

            UniTrack.UniPTEffect(effect, effdat);
        }


        public virtual short[] ConvertTrack(ModNote[] n, int which_track)
        {
            int t;
            int idx_n = 0;

            UniTrack.UniReset();
            for (t = 0; t < 64; t++)
            {
                ConvertNote(n[idx_n + which_track]);
                UniTrack.UniNewline();
                idx_n += _module.ChannelsCount;
            }
            return UniTrack.UniDup();
        }

        /// <summary>
        /// Loads all patterns of a modfile and converts them into the
        /// 3 byte format.
        /// </summary>
        /// <returns></returns>
        public virtual bool ML_LoadPatterns(int patternsCount)
        {
            int t, s = 0;

            _module.Patterns = new List<Pattern>(patternsCount);


            // Allocate temporary buffer for loading and converting the patterns
            patbuf = new ModNote[64 * _module.ChannelsCount];

            for (t = 0; t < 64 * _module.ChannelsCount; t++)
            {
                patbuf[t] = new ModNote();
                patbuf[t].A = patbuf[t].B = patbuf[t].C = patbuf[t].D = 0;
            }


            for (t = 0; t < patternsCount; t++)
            {
                if (AllocPatterns != null && !AllocPatterns(_module, t, 64))
                    return false;

                if (AllocTracks != null && !AllocTracks(_module.Patterns[t], _module.ChannelsCount))
                    return false;
                // Load the pattern into the temp buffer and convert it
                for (s = 0; s < 64 * _module.ChannelsCount; s++)
                {
                    patbuf[s].A = Reader.ReadUByte();
                    patbuf[s].B = Reader.ReadUByte();
                    patbuf[s].C = Reader.ReadUByte();
                    patbuf[s].D = Reader.ReadUByte();
                }

                for (s = 0; s < _module.ChannelsCount; s++)
                {
                    if ((_module.Patterns[t].Tracks[s].UniTrack = ConvertTrack(patbuf, s)) == null)
                        return false;
                }
            }

            return true;
        }


        public bool Load()
        {
            try
            {
                int modtype;
                int inst_num;
                int smpinfo_num;

                // try to read module header
                mh.SongName = Reader.ReadString(20);

                for (int t = 0; t < 31; t++)
                {
                    mh.Samples[t].SampleName = Reader.ReadString(22);
                    mh.Samples[t].Length = Reader.ReadMotorolaUWord();
                    mh.Samples[t].FineTune = Reader.ReadUByte();
                    mh.Samples[t].Volume = Reader.ReadUByte();
                    mh.Samples[t].RepPos = Reader.ReadMotorolaUWord();
                    mh.Samples[t].RepLen = Reader.ReadMotorolaUWord();
                }

                mh.SongLength = Reader.ReadUByte();
                mh.Magic1 = Reader.ReadUByte();

                Reader.ReadSBytes(mh.Positions, 128);
                Reader.ReadSBytes(mh.Magic2, 4);

                if (Reader.IsEOF())
                {
                    throw new SharpModException(SharpModExceptionResources.ERROR_LOADING_HEADER);
                }

                // find out which ID string				
                for (modtype = 0; modtype < 10; modtype++)
                {
                    int pos;
                    for (pos = 0; pos < 4; pos++)
                        if (mh.Magic2[pos] != modtypes[modtype].id[pos])
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
                _module.InitialSpeed = 6;
                _module.InitialTempo = 125;
                // get number of channels
                _module.ChannelsCount = modtypes[modtype].channels;
                // get ascii type of mod 
                _module.ModType = new System.String(modtypes[modtype].name.ToCharArray());
                // make a cstr this.UniModule songname
                _module.SongName = mh.SongName;
                // copy the songlength
                _module.Positions = new System.Collections.Generic.List<int>(mh.SongLength);

                // copy the position array
                for (int t = 0; t < 128; t++)
                {
                    _module.Positions.Add(mh.Positions[t]);
                    if (t >= mh.SongLength)
                        break;
                }

                // Count the number of patterns                
                var patCount = 0;
                for (int t = 0; t < mh.SongLength; t++)
                {
                    /* <-- BUGFIX... have to check ALL positions */
                    if (_module.Positions[t] > patCount)
                    {
                        patCount = _module.Positions[t];
                    }
                }
                patCount++;


                if (AllocInstruments != null && !AllocInstruments(_module, 31))
                    return false;

                smpinfo_num = 0; /* init source pointer */
                inst_num = 0; /* init dest pointer */

                for (int t = 0; t < 31; t++)
                {
                    _module.Instruments[inst_num].NumSmp = 1;

                    if (AllocSamples != null && !AllocSamples(_module.Instruments[inst_num]))
                        return false;

                    //convert the samplename
                    _module.Instruments[inst_num].InsName = mh.Samples[smpinfo_num].SampleName;

                    //init the sampleinfo variables and convert the size pointers to longword format
                    _module.Instruments[inst_num].Samples[0].C2Spd = Helper.FineTune[mh.Samples[smpinfo_num].FineTune & 0xf];
                    _module.Instruments[inst_num].Samples[0].Volume = mh.Samples[smpinfo_num].Volume;
                    _module.Instruments[inst_num].Samples[0].LoopStart = mh.Samples[smpinfo_num].RepPos << 1;
                    _module.Instruments[inst_num].Samples[0].LoopEnd = _module.Instruments[inst_num].Samples[0].LoopStart + (mh.Samples[smpinfo_num].RepLen << 1);
                    _module.Instruments[inst_num].Samples[0].Length = mh.Samples[smpinfo_num].Length << 1;
                    _module.Instruments[inst_num].Samples[0].SeekPos = 0;

                    _module.Instruments[inst_num].Samples[0].Flags = (SampleFormats.SF_SIGNED);
                    if (mh.Samples[smpinfo_num].RepLen > 1)
                        _module.Instruments[inst_num].Samples[0].Flags |= (SampleFormats.SF_LOOP);

                    //fix replen if repend>length					
                    if (_module.Instruments[inst_num].Samples[0].LoopEnd > _module.Instruments[inst_num].Samples[0].Length)
                        _module.Instruments[inst_num].Samples[0].LoopEnd = _module.Instruments[inst_num].Samples[0].Length;

                    _module.Instruments[inst_num].Samples[0].SampleRate = 8363;
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

    public class MSampInfo
    {
        /* sample header as it appears in a module */
        public string SampleName { get; set; }
        public int Length { get; set; }
        public short FineTune { get; set; }
        public short Volume { get; set; }
        public int RepPos { get; set; }
        public int RepLen { get; set; }

        public MSampInfo()
        {
            SampleName = string.Empty;
        }
    }


    public struct ModNote
    {
        public short A { get; set; }
        public short B { get; set; }
        public short C { get; set; }
        public short D { get; set; }
    }


    /* verbatim module header */
    public class ModuleHeader
    {
        /* the songname.. */
        public string SongName { get; set; }
        /* all sampleinfo */
        public MSampInfo[] Samples { get; set; }
        /* number of patterns used */
        public short SongLength { get; set; }
        /* should be 127 */
        public short Magic1 { get; set; }
        /* which pattern to play at pos */
        public sbyte[] Positions { get; set; }
        /* string "M.K." or "FLT4" or "FLT8" */
        public sbyte[] Magic2 { get; set; }

        public ModuleHeader()
        {
            SongName = string.Empty;
            Samples = new MSampInfo[31];
            int i;
            for (i = 0; i < 31; i++)
                Samples[i] = new MSampInfo();
            Positions = new sbyte[128];
            Magic2 = new sbyte[4];
        }
    }



    class ModType
    {
        /* struct to identify type of module */
        public sbyte[] id;
        public short channels;
        public System.String name;

        public ModType()
        {
            id = new sbyte[5];
        }
        public ModType(System.String init_id, int init_chn, System.String init_name)
        {
            id = new sbyte[5];
            byte[] tmp;

            tmp = System.Text.UTF8Encoding.UTF8.GetBytes(init_id);
            Buffer.BlockCopy(tmp, 0, id, 0, 4);
            id[4] = 0;

            channels = (short)init_chn;
            name = new System.String(init_name.ToCharArray());
        }
    }

    #endregion
}