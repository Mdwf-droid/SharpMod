using SharpMod.Exceptions;
using SharpMod.Song;
using SharpMod.UniTracker;

namespace SharpMod.Loaders
{
    public class M15Loader : ILoader
    {
        private readonly short[] M15_npertab = new short[] { 1712, 1616, 1524, 1440, 1356, 1280, 1208, 1140, 1076, 1016, 960, 906, 856, 808, 762, 720, 678, 640, 604, 570, 538, 508, 480, 453, 428, 404, 381, 360, 339, 320, 302, 285, 269, 254, 240, 226, 214, 202, 190, 180, 170, 160, 151, 143, 135, 127, 120, 113, 107, 101, 95, 90, 85, 80, 75, 71, 67, 63, 60, 56 };

        // raw as-is module header
        private M15ModuleHeader mh;
        private SongModule _module;

        #region private methods
        private bool LoadModuleHeader(M15ModuleHeader mh)
        {
            try
            {

                int t;

                mh.SongName = Reader.ReadString(20);

                for (t = 0; t < 15; t++)
                {
                    mh.Samples[t].samplename = Reader.ReadString(22);
                    mh.Samples[t].length = Reader.ReadMotorolaUWord();
                    mh.Samples[t].finetune = Reader.ReadUByte();
                    mh.Samples[t].volume = Reader.ReadUByte();
                    mh.Samples[t].reppos = Reader.ReadMotorolaUWord();
                    mh.Samples[t].replen = Reader.ReadMotorolaUWord();
                }

                mh.SongLength = Reader.ReadUByte();
                mh.Magic1 = Reader.ReadUByte();
                Reader.ReadSBytes(mh.Positions, 128);

                return !Reader.IsEOF();
            }
            catch (System.IO.IOException)
            {
                return false;
            }
        }


        private void M15_ConvertNote(M15ModNote n)
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
                UniTrack.UniInstrument((short)(instrument - 1));
            }

            if (note != 0)
            {
                UniTrack.UniNote((short)(note + 23));
            }

            UniTrack.UniPTEffect(effect, effdat);
        }



        private short[] M15_ConvertTrack(M15ModNote[] n, int offset)
        {
            UniTrack.UniReset();
            var n_ptr = offset;
            for (var t = 0; t < 64; t++)
            {
                M15_ConvertNote(n[n_ptr]);
                UniTrack.UniNewline();

                n_ptr += _module.ChannelsCount;
            }
            return UniTrack.UniDup();
        }


        /// <summary>
        /// Loads all patterns of a modfile and converts them into the
        /// 3 byte format.
        /// </summary>
        /// <returns></returns>
        private bool M15_LoadPatterns(int patternsCount)
        {
            int s = 0;

            _module.Patterns = new System.Collections.Generic.List<Pattern>(patternsCount);

            // Allocate temporary buffer for loading and converting the patterns                        
            var patbuf = new M15ModNote[64 * _module.ChannelsCount];
            for (int t = 0; t < 64 * _module.ChannelsCount; t++)
                patbuf[t] = new M15ModNote();

            for (int t = 0; t < 64 * _module.ChannelsCount; t++)
            {
                patbuf[t].a = patbuf[t].b = patbuf[t].c = patbuf[t].d = 0;
            }

            for (int t = 0; t < patternsCount; t++)
            {
                if (AllocPatterns != null && !AllocPatterns(_module, t, 64))
                    return false;

                if (AllocTracks != null && !AllocTracks(_module.Patterns[t], _module.ChannelsCount))
                    return false;

                // Load the pattern into the temp buffer and convert it
                for (s = 0; s < (64 * _module.ChannelsCount); s++)
                {
                    patbuf[s].a = Reader.ReadUByte();
                    patbuf[s].b = Reader.ReadUByte();
                    patbuf[s].c = Reader.ReadUByte();
                    patbuf[s].d = Reader.ReadUByte();
                }

                for (s = 0; s < _module.ChannelsCount; s++)
                {
                    if ((_module.Patterns[t].Tracks[s].UniTrack = M15_ConvertTrack(patbuf, s)) == null)
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

        public SharpMod.UniTracker.UniTrk UniTrack { get; set; }

        public bool Init(SharpMod.Song.SongModule module)
        {
            _module = module;
            int i;

            mh = new M15ModuleHeader();

            mh.SongLength = mh.Magic1 = 0;

            for (i = 0; i < 128; i++)
                mh.Positions[i] = 0;

            for (i = 0; i < 15; i++)
            {
                mh.Samples[i].length = mh.Samples[i].reppos = mh.Samples[i].replen = 0;
                mh.Samples[i].finetune = mh.Samples[i].volume = (short)0;
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
            _module.InitialSpeed = 6;
            _module.InitialTempo = 125;
            //get number m_.MLoader.of channels
            _module.ChannelsCount = 4;
            // get ascii type m_.MLoader.of mod
            _module.ModType = new System.String("15-instrument".ToCharArray());
            //make a cstr m_.MLoader.of songname 
            _module.SongName = mh.SongName;
            
            //_module.numpos = mh.songlength; /* copy the songlength */

            // copy the position array 
            for (int t = 0; t < 128; t++)
            {
                _module.Positions.Add(mh.Positions[t]);
            }

            // Count the number of patterns            
            var patCount = 0;
            for (int t = 0; t < 128; t++)
            {
                // <-- BUGFIX... have to check ALL positions
                if (_module.Positions[t] > patCount)
                {
                    patCount = _module.Positions[t];
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
                _module.Instruments[inst_num].NumSmp = 1;

                if (AllocSamples != null && !AllocSamples(_module.Instruments[inst_num]))
                    return false;

                // convert the samplename                
                _module.Instruments[inst_num].InsName = mh.Samples[smpinfo_num].samplename;

                // init the sampleinfo variables and convert the size pointers to longword format
                _module.Instruments[inst_num].Samples[0].C2Spd = Helper.FineTune[mh.Samples[smpinfo_num].finetune & 0xf];
                _module.Instruments[inst_num].Samples[0].Volume = mh.Samples[smpinfo_num].volume;
                _module.Instruments[inst_num].Samples[0].LoopStart = mh.Samples[smpinfo_num].reppos;
                _module.Instruments[inst_num].Samples[0].LoopEnd = _module.Instruments[inst_num].Samples[0].LoopStart + (mh.Samples[smpinfo_num].replen << 1);
                _module.Instruments[inst_num].Samples[0].Length = mh.Samples[smpinfo_num].length << 1;
                _module.Instruments[inst_num].Samples[0].SeekPos = 0;

                _module.Instruments[inst_num].Samples[0].Flags = (SampleFormats.SF_SIGNED);
                if (mh.Samples[smpinfo_num].replen > 1)
                    _module.Instruments[inst_num].Samples[0].Flags |= (SampleFormats.SF_LOOP);

                // fix replen if repend>length 
                if (_module.Instruments[inst_num].Samples[0].LoopEnd > _module.Instruments[inst_num].Samples[0].Length)
                    _module.Instruments[inst_num].Samples[0].LoopEnd = _module.Instruments[inst_num].Samples[0].Length;

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
            var moduleHeader = new M15ModuleHeader();

            if (!LoadModuleHeader(moduleHeader))
                return false;

            for (t = 0; t < 15; t++)
            {

                // all finetunes should be zero
                if (moduleHeader.Samples[t].finetune != 0)
                    return false;

                // all volumes should be <=64
                if (moduleHeader.Samples[t].volume > 64)
                    return false;
            }
            if (moduleHeader.Magic1 > 127)
                return false;
            // and magic1 should be <128

            return true;
        }

        #endregion

        #region internal classes
        class M15MSampInfo
        {
            // sample header as it appears in a module            
            internal string samplename;
            internal int length;
            internal short finetune;
            internal short volume;
            internal int reppos;
            internal int replen;

            public M15MSampInfo()
            {
            }
        }


        class M15ModuleHeader
        {
            // verbatim module header

            // the songname..
            internal string SongName { get; set; }
            // all sampleinfo
            internal M15MSampInfo[] Samples { get; set; }
            // number of patterns used
            internal short SongLength { get; set; }
            // should be 127
            internal short Magic1 { get; set; }
            // which pattern to play at pos
            internal sbyte[] Positions { get; set; }


            public M15ModuleHeader()
            {
                Samples = new M15MSampInfo[15];
                int i;
                for (i = 0; i < 15; i++)
                    Samples[i] = new M15MSampInfo();
                Positions = new sbyte[128];
            }
        }


        class M15ModNote
        {
            internal short a, b, c, d;
        }

        #endregion
    }
}
