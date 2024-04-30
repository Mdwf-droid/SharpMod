using SharpMod.Exceptions;
using SharpMod.IO;
using SharpMod.Song;
using SharpMod.UniTracker;
using System;
using System.IO;

namespace SharpMod.Loaders
{
    /// <summary>
    /// Screamtracker (S3M) module loader
    /// </summary>
    public class S3MLoader : ILoader
    {
        public const String S3M_Version = "Screamtracker 3.xx";

        private S3MNote[] s3mbuf; /* pointer to a complete S3M pattern */
        private int[] paraptr; /* parapointer array (see S3M docs) */

        protected internal S3MHeader mh;

        private readonly short[] remap;    /* should be [32] */

        public String LoaderType
        {
            get
            {
                return "S3M";
            }
        }

        public String LoaderVersion
        {
            get
            {
                return "Portable S3M loader v0.2";
            }
        }

        public ModBinaryReader Reader { get; set; }

        public S3MLoader()
        {

            mh = null;

            remap = new short[32];
        }

        public bool Init(SongModule module)
        {
            _module = module;

            int i;

            s3mbuf = null;
            paraptr = null;


            s3mbuf = new S3MNote[16 * 64];

            for (i = 0; i < 16 * 64; i++)
                s3mbuf[i] = new S3MNote();


            mh = new S3MHeader
            {
                T1a = 0,
                Type = 0
            };
            mh.Unused1[0] = 0;
            mh.Unused1[1] = 0;
            mh.OrdNum = 0;
            mh.InsNum = 0;
            mh.PatNum = 0;
            mh.Flags = 0;
            mh.Tracker = 0;
            mh.FileFormat = 0;
            mh.Special = 0;
            mh.MasterVol = 0;
            mh.InitSpeed = 0;
            mh.InitTempo = 0;
            mh.MasterMult = 0;
            mh.UltraClick = 0;
            mh.PanTable = 0;



            mh.Unused2.Initialize();
            mh.Channels.Initialize();



            return true;
        }

        public bool Test()
        {
            try
            {
                byte[] id = new byte[4];

                Reader.Seek(44, SeekOrigin.Begin);

                if (Reader.Read(id, 0, 4) != 4)
                    return false;

                if (System.Text.UTF8Encoding.UTF8.GetString(id, 0, id.Length) == "SCRM")
                    return true;
                return false;
            }
            catch (System.IO.IOException)
            {
                return false;
            }
        }

        public void Cleanup()
        {
            if (s3mbuf != null)
                s3mbuf = null;
            if (paraptr != null)
                paraptr = null;
            if (mh != null)
                mh = null;
        }

        public virtual bool S3M_ReadPattern()
        {
            try
            {

                int row = 0, flag, ch;

                int i;
                for (i = 0; i < 16 * 64; i++)
                {
                    s3mbuf[i].Note = s3mbuf[i].Ins = s3mbuf[i].Vol = s3mbuf[i].Cmd = s3mbuf[i].Inf = 255;
                }



                while (row < 64)
                {
                    flag = Reader.ReadByte();

                    if (flag == -1)
                    {
                        throw new SharpModException(SharpModExceptionResources.ERROR_LOADING_PATTERN);
                    }

                    if (flag != 0)
                    {

                        ch = flag & 31;

                        if (mh.Channels[ch] < 16)
                        {
                            if ((flag & 32) != 0)
                            {
                                s3mbuf[(64 * remap[ch]) + row].Note = Reader.ReadUByte();
                                s3mbuf[(64 * remap[ch]) + row].Ins = Reader.ReadUByte();
                            }

                            if ((flag & 64) != 0)
                            {
                                s3mbuf[(64 * remap[ch]) + row].Vol = Reader.ReadUByte();
                            }

                            if ((flag & 128) != 0)
                            {
                                s3mbuf[(64 * remap[ch]) + row].Cmd = Reader.ReadUByte();
                                s3mbuf[(64 * remap[ch]) + row].Inf = Reader.ReadUByte();
                            }
                        }
                        else
                        {
                            for (int b = 0; b < ((((flag & 32) != 0) ? 2 : 0) + (((flag & 64) != 0) ? 1 : 0) + (((flag & 128) != 0) ? 2 : 0)); b++)
                            {
                                Reader.ReadUByte();
                            }
                        }
                    }
                    else
                        row++;
                }
                return true;
            }
            catch (System.IO.IOException)
            {
                return false;
            }
        }


        public virtual short[] S3M_ConvertTrack(S3MNote[] tr, int offset)
        {
            int t;

            short note, ins, vol, cmd, inf, lo, hi;

            UniTrack.UniReset();
            for (t = offset; t < offset + 64; t++)
            {

                note = tr[t].Note;
                ins = tr[t].Ins;
                vol = tr[t].Vol;
                cmd = tr[t].Cmd;
                inf = tr[t].Inf;
                lo = (short)(inf & 0xf);
                hi = (short)(inf >> 4);

                if (ins != 0 && ins != 255 && ins != (-1))
                {
                    UniTrack.UniInstrument((short)(ins - 1));
                }

                if ((note != 255) && (note != -1))
                {
                    if (note == 254)
                        UniTrack.UniPTEffect(0xc, 0);
                    /* <- note off command */
                    else
                        UniTrack.UniNote((short)((((note & 0xF0) >> 4) * 12) + (note & 0xf)));
                    /* <- normal note */
                }

                if ((vol < 255) && (vol != -1))
                {
                    UniTrack.UniPTEffect(0xc, vol);
                }

                if (cmd != 255)
                {
                    switch (cmd)
                    {
                        case 1:
                            UniTrack.UniWrite(Effects.UNI_S3MEFFECTA);
                            UniTrack.UniWrite(inf);
                            break;
                        case 2:
                            UniTrack.UniPTEffect(0xb, inf);
                            break;
                        case 3:
                            UniTrack.UniPTEffect(0xd, inf);
                            break;
                        case 4:
                            UniTrack.UniWrite(Effects.UNI_S3MEFFECTD);
                            UniTrack.UniWrite(inf);
                            break;
                        case 5:
                            UniTrack.UniWrite(Effects.UNI_S3MEFFECTE);
                            UniTrack.UniWrite(inf);
                            break;
                        case 6:
                            UniTrack.UniWrite(Effects.UNI_S3MEFFECTF);
                            UniTrack.UniWrite(inf);
                            break;
                        case 7:
                            UniTrack.UniPTEffect(0x3, inf);
                            break;
                        case 8:
                            UniTrack.UniPTEffect(0x4, inf);
                            break;
                        case 9:
                            UniTrack.UniWrite(Effects.UNI_S3MEFFECTI);
                            UniTrack.UniWrite(inf);
                            break;
                        case (short)(0xa):
                            UniTrack.UniPTEffect(0x0, inf);
                            break;
                        case (short)(0xb):
                            UniTrack.UniPTEffect(0x4, 0);
                            UniTrack.UniWrite(Effects.UNI_S3MEFFECTD);
                            UniTrack.UniWrite(inf);
                            break;
                        case (short)(0xc):
                            UniTrack.UniPTEffect(0x3, 0);
                            UniTrack.UniWrite(Effects.UNI_S3MEFFECTD);
                            UniTrack.UniWrite(inf);
                            break;
                        case (short)(0xf):
                            UniTrack.UniPTEffect(0x9, inf);
                            break;
                        case (short)(0x11):
                            UniTrack.UniWrite(Effects.UNI_S3MEFFECTQ);
                            UniTrack.UniWrite(inf);
                            break;
                        case (short)(0x12):
                            UniTrack.UniPTEffect(0x6, inf);
                            break;
                        case (short)(0x13):
                            switch (hi)
                            {
                                case 0:
                                    UniTrack.UniPTEffect(0xe, lo);
                                    break;
                                case 1:
                                    UniTrack.UniPTEffect(0xe, (short)(0x30 | lo));
                                    break;
                                case 2:
                                    UniTrack.UniPTEffect(0xe, (short)(0x50 | lo));
                                    break;
                                case 3:
                                    UniTrack.UniPTEffect(0xe, (short)(0x40 | lo));
                                    break;
                                case 4:
                                    UniTrack.UniPTEffect(0xe, (short)(0x70 | lo));
                                    break;
                                case 8:
                                    UniTrack.UniPTEffect(0xe, (short)(0x80 | lo));
                                    break;
                                case 0xb:
                                    UniTrack.UniPTEffect(0xe, (short)(0x60 | lo));
                                    break;
                                case 0xc:
                                    UniTrack.UniPTEffect(0xe, (short)(0xC0 | lo));
                                    break;
                                case 0xd:
                                    UniTrack.UniPTEffect(0xe, (short)(0xD0 | lo));
                                    break;
                                case 0xe:
                                    UniTrack.UniPTEffect(0xe, (short)(0xE0 | lo));
                                    break;
                            }
                            break;


                        case 0x14:
                            if (inf > 0x20)
                            {
                                UniTrack.UniWrite(Effects.UNI_S3MEFFECTT);
                                UniTrack.UniWrite(inf);
                            }
                            break;


                        case 0x18:
                            UniTrack.UniPTEffect((short)0x8, inf);
                            break;
                    }
                }

                UniTrack.UniNewline();
            }
            return UniTrack.UniDup();
        }


        public bool Load()
        {
            try
            {

                int t, u = 0;
                int inst_num;
                short[] isused = new short[16];
                sbyte[] pan = new sbyte[32];

                /* try to read module header */
                mh.Songname = Reader.ReadString(28);
                mh.T1a = Reader.ReadSByte();
                mh.Type = Reader.ReadSByte();
                Reader.ReadSBytes(mh.Unused1, 2);
                mh.OrdNum = Reader.ReadIntelUWord();
                mh.InsNum = Reader.ReadIntelUWord();
                mh.PatNum = Reader.ReadIntelUWord();
                mh.Flags = Reader.ReadIntelUWord();
                mh.Tracker = Reader.ReadIntelUWord();
                mh.FileFormat = Reader.ReadIntelUWord();
                mh.Scrm = Reader.ReadString(4);

                mh.MasterVol = Reader.ReadUByte();
                mh.InitSpeed = Reader.ReadUByte();
                mh.InitTempo = Reader.ReadUByte();
                mh.MasterMult = Reader.ReadUByte();
                mh.UltraClick = Reader.ReadUByte();
                mh.PanTable = Reader.ReadUByte();
                Reader.ReadSBytes(mh.Unused2, 8);
                mh.Special = Reader.ReadIntelUWord();
                Reader.ReadUBytes(mh.Channels, 32);

                if (Reader.IsEOF())
                {
                    throw new SharpModException(SharpModExceptionResources.ERROR_LOADING_HEADER);
                }

                /* set module variables */
                _module.ModType = new System.String(S3M_Version.ToCharArray());
                _module.SongName = mh.Songname;
                _module.InitialSpeed = mh.InitSpeed;
                _module.InitialTempo = mh.InitTempo;

                // count the number this.UniModule channels used
                _module.ChannelsCount = 0;

                for (t = 0; t < 32; t++)
                    remap[t] = 0;
                for (t = 0; t < 16; t++)
                    isused[t] = 0;

                // set a flag for each channel (1 out this.UniModule this.UniModule 16) thats being used:
                for (t = 0; t < 32; t++)
                {
                    if (mh.Channels[t] < 16)
                    {
                        isused[mh.Channels[t]] = 1;
                    }
                }

                // give each this.UniModule them a different number
                for (t = 0; t < 16; t++)
                {
                    if (isused[t] != 0)
                    {
                        isused[t] = (short)_module.ChannelsCount;
                        _module.ChannelsCount++;
                    }
                }

                /* build the remap array */
                for (t = 0; t < 32; t++)
                {
                    if (mh.Channels[t] < 16)
                    {
                        remap[t] = isused[mh.Channels[t]];
                    }
                }

                /* set panning positions */
                for (t = 0; t < 32; t++)
                {
                    if (mh.Channels[t] < 16)
                    {
                        if (mh.Channels[t] < 8)
                        {
                            _module.Panning[remap[t]] = 0x30;
                        }
                        else
                        {
                            _module.Panning[remap[t]] = 0xc0;
                        }
                    }
                }


                short[] tmp = new short[mh.OrdNum];
                Reader.ReadUBytes(tmp, mh.OrdNum);

                foreach (short pos in tmp)
                    _module.Positions.Add(pos);

                paraptr = new int[mh.InsNum + mh.PatNum];

                /* read the instrument+pattern parapointers */
                Reader.ReadIntelUWords(paraptr, mh.InsNum + mh.PatNum);

                if (mh.PanTable == 252)
                {
                    /* read the panning table */
                    Reader.ReadSBytes(pan, 32);

                    /* set panning positions according to panning table (new for st3.2) */
                    for (t = 0; t < 32; t++)
                    {
                        if (((pan[t] & 0x20) != 0) && mh.Channels[t] < 16)
                        {
                            _module.Panning[remap[t]] = (short)((pan[t] & 0xf) << 4);
                        }
                    }
                }

                /* now is a good time to check if the header was too short :) */
                if (Reader.IsEOF())
                {
                    throw new SharpModException(SharpModExceptionResources.ERROR_LOADING_HEADER);

                }

                if (AllocInstruments != null && !AllocInstruments(_module, mh.InsNum))
                    return false;

                inst_num = 0;

                for (t = 0; t < mh.InsNum; t++)
                {
                    S3MSample s = new();

                    _module.Instruments[inst_num].NumSmp = 1;

                    /* seek to instrument position */
                    Reader.Seek(paraptr[t] << 4, SeekOrigin.Begin);

                    /* and load sample info */
                    s.type = Reader.ReadUByte();
                    s.filename = Reader.ReadString(12);
                    s.memsegh = Reader.ReadUByte();
                    s.memsegl = Reader.ReadIntelUWord();
                    s.length = Reader.ReadIntelULong();
                    s.loopbeg = Reader.ReadIntelULong();
                    s.loopend = Reader.ReadIntelULong();
                    s.volume = Reader.ReadUByte();
                    s.dsk = Reader.ReadUByte();
                    s.pack = Reader.ReadUByte();
                    s.flags = Reader.ReadUByte();
                    s.c2spd = Reader.ReadIntelULong();
                    Reader.ReadSBytes(s.unused, 12);
                    s.sampname = Reader.ReadString(28);
                    s.scrs = Reader.ReadString(4);

                    if (Reader.IsEOF())
                    {
                        throw new SharpModException(SharpModExceptionResources.ERROR_LOADING_HEADER);
                    }

                    _module.Instruments[inst_num].Samples.Add(new Sample());

                    _module.Instruments[inst_num].InsName = s.sampname;
                    _module.Instruments[inst_num].Samples[0].C2Spd = s.c2spd;
                    _module.Instruments[inst_num].Samples[0].Length = s.length;
                    _module.Instruments[inst_num].Samples[0].LoopStart = s.loopbeg;
                    _module.Instruments[inst_num].Samples[0].LoopEnd = s.loopend;
                    _module.Instruments[inst_num].Samples[0].Volume = s.volume;
                    _module.Instruments[inst_num].Samples[0].SeekPos = (((int)s.memsegh) << 16 | s.memsegl) << 4;

                    _module.Instruments[inst_num].Samples[0].Flags = 0;

                    if ((s.flags & 1) != 0)
                        _module.Instruments[inst_num].Samples[0].Flags |= (SampleFormats.SF_LOOP);
                    if ((s.flags & 4) != 0)
                        _module.Instruments[inst_num].Samples[0].Flags |= (SampleFormats.SF_16BITS);
                    if (mh.FileFormat == 1)
                        _module.Instruments[inst_num].Samples[0].Flags |= (SampleFormats.SF_SIGNED);

                    _module.Instruments[inst_num].Samples[0].SampleRate = 22050;

                    /* DON'T load sample if it doesn't have the SCRS tag */
                    if (s.scrs.Length < 4 || (!((s.scrs[0] == 'S') && (s.scrs[1] == 'C') && (s.scrs[2] == 'R') && (s.scrs[3] == 'S'))))
                    {
                        _module.Instruments[inst_num].Samples[0].Length = 0;
                    }

                    inst_num++;
                }


                for (t = 0; t < mh.PatNum; t++)
                {
                    if (AllocPatterns != null && !AllocPatterns(_module, t, 64))
                        return false;
                    if (AllocTracks != null && !AllocTracks(_module.Patterns[t], _module.ChannelsCount))
                        return false;

                    /* seek to pattern position ( + 2 skip pattern length ) */
                    Reader.Seek((paraptr[mh.InsNum + t] << 4) + 2, SeekOrigin.Begin);
                    if (!S3M_ReadPattern())
                        return false;

                    for (u = 0; u < _module.ChannelsCount; u++)
                    {
                        _module.Patterns[t].Tracks[u].UniTrack = S3M_ConvertTrack(s3mbuf, u * 64);
                    }
                }

                return true;
            }
            catch (System.IO.IOException)
            {
                return false;
            }
        }

        #region ILoader Members


        public UniTrk UniTrack
        {
            get;
            set;
        }

        private SongModule _module;

        public event AllocPatternsHandler AllocPatterns;
        public event AllocTracksHandler AllocTracks;
        public event AllocInstrumentsHandler AllocInstruments;
#pragma warning disable CS0067 // L'événement 'S3MLoader.AllocSamples' n'est jamais utilisé
        public event AllocSamplesHandler AllocSamples;
#pragma warning restore CS0067 // L'événement 'S3MLoader.AllocSamples' n'est jamais utilisé




        #endregion
    }

    #region Module Internals structures
    public class S3MNote
    {
        public short Note { get; set; }
        public short Ins { get; set; }
        public short Vol { get; set; }
        public short Cmd { get; set; }
        public short Inf { get; set; }
    }

    /* Raw S3M header struct: */

    public class S3MHeader
    {
        public string Songname { get; set; }
        public sbyte T1a { get; set; }
        public sbyte Type { get; set; }
        public sbyte[] Unused1 { get; set; }
        public int OrdNum { get; set; }
        public int InsNum { get; set; }
        public int PatNum { get; set; }
        public int Flags { get; set; }
        public int Tracker { get; set; }
        public int FileFormat { get; set; }
        public string Scrm { get; set; }
        public short MasterVol { get; set; }
        public short InitSpeed { get; set; }
        public short InitTempo { get; set; }
        public short MasterMult { get; set; }
        public short UltraClick { get; set; }
        public short PanTable { get; set; }
        public sbyte[] Unused2 { get; set; }
        public int Special { get; set; }
        public short[] Channels { get; set; }

        public S3MHeader()
        {
            Songname = String.Empty;
            Unused1 = new sbyte[2];
            Scrm = String.Empty;
            Unused2 = new sbyte[8];
            Channels = new short[32];
        }
    }

    /* Raw S3M sampleinfo struct: */

    class S3MSample
    {
        internal short type;
        internal string filename;
        internal short memsegh;
        internal int memsegl;
        internal int length;
        internal int loopbeg;
        internal int loopend;
        internal short volume;
        internal short dsk;
        internal short pack;
        internal short flags;
        internal int c2spd;
        internal sbyte[] unused;
        internal string sampname;
        internal string scrs;

        public S3MSample()
        {
            filename = string.Empty;
            unused = new sbyte[12];
            sampname = string.Empty;
            scrs = string.Empty;
        }
    }
    #endregion

}