using SharpMod.Exceptions;
using SharpMod.IO;
using SharpMod.Song;
using SharpMod.UniTracker;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SharpMod.Loaders
{
    /// <summary>
    /// Fasttracker (XM) module loader
    /// </summary>
    public class XMLoader : ILoader
    {
        public event AllocPatternsHandler AllocPatterns;
        public event AllocTracksHandler AllocTracks;
        public event AllocInstrumentsHandler AllocInstruments;
        public event AllocSamplesHandler AllocSamples;

        private SongModule _module;
                
        private XMHeader mh;

        public String LoaderType
        {
            get
            {
                return "XM";
            }
        }

        public String LoaderVersion => "Portable XM loader v0.4 - for your ears only / MikMak";


        public UniTrk UniTrack { get; set; }


        public ModBinaryReader Reader { get; set; }


        public XMLoader()
        {
            mh = null;
        }


        public bool Test()
        {
            try
            {
                var id = new byte[17];
                
                var szShould = "Extended Module: ";
                var should_be = UTF8Encoding.UTF8.GetBytes(szShould);
                
                int a;
                if (Reader.Read(id, 0, 17) != 17)
                    return false;
                for (a = 0; a < 17; a++)
                {
                    if (id[a] != should_be[a])
                        return false;
                }
                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }


        public bool Init(SongModule module)
        {
            _module = module;

            mh = null;
            
            mh = new XMHeader();

            mh.version = mh.headersize = mh.restart = mh.tempo = mh.bpm = 0;
            mh.songlength = mh.numchn = mh.numpat = mh.numins = mh.flags = 0;
                        
            mh.orders.Initialize();

            return true;
        }


        public void Cleanup()
        {
            if (mh != null)
                mh = null;
        }


        public virtual void XM_ReadNote(XMNote n)
        {
            short cmp;

            n.note = n.ins = n.vol = n.eff = n.dat = 0;


            cmp = Reader.ReadByte();

            if ((cmp & 0x80) != 0)
            {
                if ((cmp & 1) != 0)
                    n.note = Reader.ReadByte();
                if ((cmp & 2) != 0)
                    n.ins = Reader.ReadByte();
                if ((cmp & 4) != 0)
                    n.vol = Reader.ReadByte();
                if ((cmp & 8) != 0)
                    n.eff = Reader.ReadByte();
                if ((cmp & 16) != 0)
                    n.dat = Reader.ReadByte();
            }
            else
            {
                n.note = cmp;

                n.ins = Reader.ReadByte();
                n.vol = Reader.ReadByte();
                n.eff = Reader.ReadByte();
                n.dat = Reader.ReadByte();
            }
            if (n.note == -1)
                n.note = 255;
            if (n.ins == -1)
                n.ins = 255;
            if (n.vol == -1)
                n.vol = 255;
            if (n.eff == -1)
                n.eff = 255;
            if (n.dat == -1)
                n.dat = 255;
        }


        public virtual short[] XM_Convert(XMNote[] xmtrack, int offset, int rows)
        {
            int t;
            short note, ins, vol, eff, dat;

            UniTrack.UniReset();

            int xmi = offset;

            for (t = 0; t < rows; t++)
            {

                note = xmtrack[xmi].note;
                ins = xmtrack[xmi].ins;
                vol = xmtrack[xmi].vol;
                eff = xmtrack[xmi].eff;
                dat = xmtrack[xmi].dat;

                if (note != 0)
                    UniTrack.UniNote((short)(note - 1));

                if (ins != 0)
                    UniTrack.UniInstrument((short)(ins - 1));

                

                switch (vol >> 4)
                {


                    case (short)(0x6):
                        if ((vol & 0xf) != 0)
                        {
                            UniTrack.UniWrite(Effects.UNI_XMEFFECTA);
                            UniTrack.UniWrite((short)(vol & 0xf));
                        }
                        break;


                    case (short)(0x7):
                        if ((vol & 0xf) != 0)
                        {
                            UniTrack.UniWrite(Effects.UNI_XMEFFECTA);
                            UniTrack.UniWrite((short)(vol << 4));
                        }
                        break;

                    /* volume-row fine volume slide is compatible with protracker
                    EBx and EAx effects i.e. a zero nibble means DO NOT SLIDE, as
                    opposed to 'take the last sliding value'.
                    */


                    case (short)(0x8):
                        UniTrack.UniPTEffect((short)0xe, (short)(0xb0 | (vol & 0xf)));
                        break;


                    case (short)(0x9):
                        UniTrack.UniPTEffect((short)0xe, (short)(0xa0 | (vol & 0xf)));
                        break;


                    case (short)(0xa):
                        UniTrack.UniPTEffect((short)0x4, (short)(vol << 4));
                        break;


                    case (short)(0xb):
                        UniTrack.UniPTEffect((short)0x4, (short)(vol & 0xf));
                        break;


                    case (short)(0xc):
                        UniTrack.UniPTEffect((short)0x8, (short)(vol << 4));
                        break;


                    case (short)(0xd):

                        if ((vol & 0xf) != 0)
                        {
                            UniTrack.UniWrite(Effects.UNI_XMEFFECTP);
                            UniTrack.UniWrite((short)(vol & 0xf));
                        }
                        break;


                    case (short)(0xe):

                        if ((vol & 0xf) != 0)
                        {
                            UniTrack.UniWrite(Effects.UNI_XMEFFECTP);
                            UniTrack.UniWrite((short)(vol << 4));
                        }
                        break;


                    case (short)(0xf):
                        UniTrack.UniPTEffect((short)0x3, (short)(vol << 4));
                        break;


                    default:
                        if (vol >= 0x10 && vol <= 0x50)
                        {
                            UniTrack.UniPTEffect((short)0xc, (short)(vol - 0x10));
                        }
                        break;

                }


                switch (eff)
                {


                    case 'G' - 55:
                        if (dat > 64)
                            dat = 64;
                        UniTrack.UniWrite(Effects.UNI_XMEFFECTG);
                        UniTrack.UniWrite(dat);
                        break;


                    case 'H' - 55:
                        UniTrack.UniWrite(Effects.UNI_XMEFFECTH);
                        UniTrack.UniWrite(dat);
                        break;


                    case 'K' - 55:
                        UniTrack.UniNote((short)96);
                        break;


                    case 'L' - 55:
                        break;


                    case 'P' - 55:
                        UniTrack.UniWrite(Effects.UNI_XMEFFECTP);
                        UniTrack.UniWrite(dat);
                        break;


                    case 'R' - 55:
                        UniTrack.UniWrite(Effects.UNI_S3MEFFECTQ);
                        UniTrack.UniWrite(dat);
                        break;


                    case 'T' - 55:
                        UniTrack.UniWrite(Effects.UNI_S3MEFFECTI);
                        UniTrack.UniWrite(dat);
                        break;


                    case 'X' - 55:
                        if ((dat >> 4) == 1)
                        {
                            /* X1 extra fine porta up */
                        }
                        else
                        {
                            /* X2 extra fine porta down */
                        }
                        break;


                    default:
                        if (eff == 0xa)
                        {
                            UniTrack.UniWrite(Effects.UNI_XMEFFECTA);
                            UniTrack.UniWrite(dat);
                        }
                        else if (eff <= 0xf)
                            UniTrack.UniPTEffect(eff, dat);
                        break;

                }

                UniTrack.UniNewline();
                xmi++;
            }
            return UniTrack.UniDup();
        }



        public bool Load()
        {
            try
            {
                XMNote[] xmpat;
                int inst_num;
                int t, u, v, p;
                int next;
                int i;

                /* try to read module header */
                mh.id = Reader.ReadString(17);
                mh.songname = Reader.ReadString(21);
                mh.trackername = Reader.ReadString(20);

                mh.version = Reader.ReadIntelUWord();
                mh.headersize = Reader.ReadIntelULong();
                mh.songlength = (short)Reader.ReadIntelUWord();
                mh.restart = Reader.ReadIntelUWord();
                mh.numchn = (short)Reader.ReadIntelUWord();
                mh.numpat = (short)Reader.ReadIntelUWord();
                mh.numins = (short)Reader.ReadIntelUWord();
                mh.flags = (short)Reader.ReadIntelUWord();
                mh.tempo = Reader.ReadIntelUWord();
                mh.bpm = Reader.ReadIntelUWord();
                Reader.ReadUBytes(mh.orders, 256);

                if (Reader.IsEOF())
                {
                    throw new SharpModException(SharpModExceptionResources.ERROR_LOADING_HEADER);
                }

                /* set module variables */
                _module.InitialSpeed = (short)mh.tempo;
                _module.InitialTempo = (short)mh.bpm;
                _module.ModType = mh.trackername;
                _module.ChannelsCount = mh.numchn;
                _module.SongName = mh.songname;
                _module.RepPos = (short)mh.restart;

                _module.Flags |= UniModPeriods.UF_XMPERIODS;
                if ((mh.flags & 1) != 0)
                    _module.Flags |= UniModPeriods.UF_LINEAR;

                _module.Positions = new List<int>(mh.songlength);
                for (t = 0; t < 256; t++)
                    if (t >= mh.songlength)
                        break;
                    else
                        _module.Positions.Add(mh.orders[t]);

                for (t = 0; t < mh.numpat; t++)
                {
                    XMPatHeader ph = new()
                    {
                        size = Reader.ReadIntelULong(),
                        packing = Reader.ReadUByte(),
                        numrows = (short)Reader.ReadIntelUWord(),
                        packsize = Reader.ReadIntelUWord()
                    };

                    if (AllocPatterns != null && !AllocPatterns(_module, t, ph.numrows))
                        return false;
                    if (AllocTracks != null && !AllocTracks(_module.Patterns[t], _module.ChannelsCount))
                        return false;

                    xmpat = new XMNote[ph.numrows * _module.ChannelsCount];
                    for (i = 0; i < ph.numrows * _module.ChannelsCount; i++)
                        xmpat[i] = new XMNote();

                    for (i = 0; i < ph.numrows * _module.ChannelsCount; i++)
                    {
                        xmpat[i].note = xmpat[i].ins = xmpat[i].vol = xmpat[i].eff = xmpat[i].dat = 0;
                    }


                    if (ph.packsize > 0)
                    {
                        for (u = 0; u < ph.numrows; u++)
                        {
                            for (v = 0; v < _module.ChannelsCount; v++)
                            {
                                XM_ReadNote(xmpat[(v * ph.numrows) + u]);
                            }
                        }
                    }

                    for (v = 0; v < _module.ChannelsCount; v++)
                    {
                        _module.Patterns[t].Tracks[v].Cells = new List<PatternCell>(new PatternCell[ph.numrows]);
                        _module.Patterns[t].Tracks[v].UniTrack = XM_Convert(xmpat, v * ph.numrows, ph.numrows);
                    }
                                        
                }

                if (AllocInstruments != null && !AllocInstruments(_module, mh.numins))
                    return false;

                inst_num = 0;

                for (t = 0; t < _module.Instruments.Count; t++)
                {
                    var ih = new XMInstHeader();

                    /* read instrument header */

                    ih.size = Reader.ReadIntelULong();
                    ih.name = Reader.ReadString(22);
                    ih.type = Reader.ReadUByte();
                    ih.numsmp = (short)Reader.ReadIntelUWord();
                    ih.ssize = Reader.ReadIntelULong();


                    _module.Instruments[inst_num].InsName = ih.name;
                    _module.Instruments[inst_num].NumSmp = ih.numsmp;

                    if (ih.numsmp > 0)
                    {
                        var pth = new XMPatchHeader();
                        var wh = new XMWavHeader();

                        Reader.ReadUBytes(pth.what, 96);
                        Reader.ReadUBytes(pth.volenv, 48);
                        Reader.ReadUBytes(pth.panenv, 48);
                        pth.volpts = Reader.ReadUByte();
                        pth.panpts = Reader.ReadUByte();
                        pth.volsus = Reader.ReadUByte();
                        pth.volbeg = Reader.ReadUByte();
                        pth.volend = Reader.ReadUByte();
                        pth.pansus = Reader.ReadUByte();
                        pth.panbeg = Reader.ReadUByte();
                        pth.panend = Reader.ReadUByte();
                        pth.volflg = Reader.ReadUByte();
                        pth.panflg = Reader.ReadUByte();
                        pth.vibflg = Reader.ReadUByte();
                        pth.vibsweep = Reader.ReadUByte();
                        pth.vibdepth = Reader.ReadUByte();
                        pth.vibrate = Reader.ReadUByte();
                        pth.volfade = Reader.ReadIntelUWord();
                        Reader.ReadIntelSWords(pth.reserved, 11);


                        for (i = 0; i < 96; i++)
                        {
                            _module.Instruments[inst_num].SampleNumber[i] = pth.what[i];
                        }

                        _module.Instruments[inst_num].VolFade = pth.volfade;

                        for (i = 0; i < 6; i++)
                        {
                            _module.Instruments[inst_num].VolEnv[i].Pos = (short)(pth.volenv[i * 4] + (pth.volenv[i * 4 + 1] << 8));
                            _module.Instruments[inst_num].VolEnv[i].Val = (short)(pth.volenv[i * 4 + 2] + (pth.volenv[i * 4 + 3] << 8));
                        }

                        _module.Instruments[inst_num].VolFlg = pth.volflg;
                        _module.Instruments[inst_num].VolSus = pth.volsus;
                        _module.Instruments[inst_num].VolBeg = pth.volbeg;
                        _module.Instruments[inst_num].VolEnd = pth.volend;
                        _module.Instruments[inst_num].VolPts = pth.volpts;


                        /* scale volume envelope: */
                        for (p = 0; p < 12; p++)
                        {
                            _module.Instruments[inst_num].VolEnv[p].Val <<= 2;
                        }

                        for (i = 0; i < 6; i++)
                        {
                            _module.Instruments[inst_num].PanEnv[i].Pos = (short)(pth.panenv[i * 4] + (pth.panenv[i * 4 + 1] << 8));
                            _module.Instruments[inst_num].PanEnv[i].Val = (short)(pth.panenv[i * 4 + 2] + (pth.panenv[i * 4 + 3] << 8));
                        }

                        _module.Instruments[inst_num].PanFlg = pth.panflg;
                        _module.Instruments[inst_num].PanSus = pth.pansus;
                        _module.Instruments[inst_num].PanBeg = pth.panbeg;
                        _module.Instruments[inst_num].PanEnd = pth.panend;
                        _module.Instruments[inst_num].PanPts = pth.panpts;


                        /* scale panning envelope: */
                        for (p = 0; p < 12; p++)
                        {
                            _module.Instruments[inst_num].PanEnv[p].Val <<= 2;
                        }

                        next = 0;

                        for (u = 0; u < ih.numsmp; u++)
                        {
                            wh.length = Reader.ReadIntelULong();
                            wh.loopstart = Reader.ReadIntelULong();
                            wh.looplength = Reader.ReadIntelULong();
                            wh.volume = Reader.ReadUByte();
                            wh.finetune = Reader.ReadSByte();
                            wh.type = Reader.ReadUByte();
                            wh.panning = Reader.ReadUByte();
                            wh.relnote = Reader.ReadSByte();
                            wh.reserved = (sbyte)Reader.ReadUByte();
                            wh.samplename = Reader.ReadString(22);

                            if (u == _module.Instruments[t].Samples.Count)
                                _module.Instruments[t].Samples.Add(new Sample());
                            _module.Instruments[t].Samples[u].SampleName = wh.samplename;
                            _module.Instruments[t].Samples[u].Length = wh.length;
                            _module.Instruments[t].Samples[u].LoopStart = wh.loopstart;
                            _module.Instruments[t].Samples[u].LoopEnd = wh.loopstart + wh.looplength;
                            _module.Instruments[t].Samples[u].Volume = wh.volume;
                            _module.Instruments[t].Samples[u].C2Spd = wh.finetune + 128;
                            _module.Instruments[t].Samples[u].Transpose = wh.relnote;
                            _module.Instruments[t].Samples[u].Panning = wh.panning;
                            _module.Instruments[t].Samples[u].SeekPos = next;
                            _module.Instruments[t].Samples[u].SampleRate = 22050; // Seems to be good...

                            if ((wh.type & 0x10) != 0)
                            {
                                _module.Instruments[t].Samples[u].Length >>= 1;
                                _module.Instruments[t].Samples[u].LoopStart >>= 1;
                                _module.Instruments[t].Samples[u].LoopEnd >>= 1;
                            }

                            next += wh.length;

                            _module.Instruments[t].Samples[u].Flags |= (SampleFormats.SF_OWNPAN);
                            if ((wh.type & 0x3) != 0)
                                _module.Instruments[t].Samples[u].Flags |= (SampleFormats.SF_LOOP);
                            if ((wh.type & 0x2) != 0)
                                _module.Instruments[t].Samples[u].Flags |= (SampleFormats.SF_BIDI);

                            if ((wh.type & 0x10) != 0)
                                _module.Instruments[t].Samples[u].Flags |= (SampleFormats.SF_16BITS);

                            _module.Instruments[t].Samples[u].Flags |= (SampleFormats.SF_DELTA);
                            _module.Instruments[t].Samples[u].Flags |= (SampleFormats.SF_SIGNED);
                        }

                        for (u = 0; u < ih.numsmp; u++)
                            _module.Instruments[inst_num].Samples[u].SeekPos += Reader.Tell();

                        Reader.Seek(next, SeekOrigin.Current);
                    }
                    inst_num++;
                }


                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }


    }

    #region Internal structs
    public class XMHeader
    {
        internal string id; /* ID text: 'Extended module: ' */
        internal string songname; /* Module name, padded with zeroes and 0x1a at the end */
        internal string trackername; /* Tracker name */
        internal int version; /* (word) Version number, hi-byte major and low-byte minor */
        internal int headersize; /* Header size */
        internal short songlength; /* (word) Song length (in patten order table) */
        internal int restart; /* (word) Restart position */
        internal short numchn; /* (word) Number of channels (2,4,6,8,10,...,32) */
        internal short numpat; /* (word) Number of patterns (max 256) */
        internal short numins; /* (word) Number of instruments (max 128) */
        internal short flags; /* (word) Flags: bit 0: 0 = Amiga frequency table (see below) 1 = Linear frequency table */
        internal int tempo; /* (word) Default tempo */
        internal int bpm; /* (word) Default BPM */
        internal short[] orders;
        /* (byte) Pattern order table */

        public XMHeader()
        {
            orders = new short[256];
        }
    }

    public class XMNote
    {
        internal short note, ins, vol, eff, dat;
    }

    class XMInstHeader
    {
        internal int size; /* (dword) Instrument size */
        internal string name; /* (char) Instrument name */
        internal short type; /* (byte) Instrument type (always 0) */
        internal short numsmp; /* (word) Number of samples in instrument */
        internal int ssize;

        public XMInstHeader()
        {
        }
    }


    class XMPatchHeader
    {
        internal short[] what; /* (byte) Sample number for all notes */
        internal short[] volenv; /* (byte) Points for volume envelope */
        internal short[] panenv; /* (byte) Points for panning envelope */
        internal short volpts; /* (byte) Number of volume points */
        internal short panpts; /* (byte) Number of panning points */
        internal short volsus; /* (byte) Volume sustain point */
        internal short volbeg; /* (byte) Volume loop start point */
        internal short volend; /* (byte) Volume loop end point */
        internal short pansus; /* (byte) Panning sustain point */
        internal short panbeg; /* (byte) Panning loop start point */
        internal short panend; /* (byte) Panning loop end point */
        internal short volflg; /* (byte) Volume type: bit 0: On; 1: Sustain; 2: Loop */
        internal short panflg; /* (byte) Panning type: bit 0: On; 1: Sustain; 2: Loop */
        internal short vibflg; /* (byte) Vibrato type */
        internal short vibsweep; /* (byte) Vibrato sweep */
        internal short vibdepth; /* (byte) Vibrato depth */
        internal short vibrate; /* (byte) Vibrato rate */
        internal int volfade; /* (word) Volume fadeout */
        internal short[] reserved; /* (word) Reserved */

        public XMPatchHeader()
        {
            what = new short[96];
            volenv = new short[48];
            panenv = new short[48];
            reserved = new short[11];
        }
    }


    class XMWavHeader
    {
        internal int length; /* (dword) Sample length */
        internal int loopstart; /* (dword) Sample loop start */
        internal int looplength; /* (dword) Sample loop length */
        internal short volume; /* (byte) Volume */
        internal sbyte finetune; /* (byte) Finetune (signed byte -128..+127) */
        internal short type; /* (byte) Type: Bit 0-1: 0 = No loop, 1 = Forward loop, */
        /*                                        2: Ping-pong loop    */
        /*                                        4: 16-bit sampledata */
        internal short panning; /* (byte) Panning (0-255) */
        internal sbyte relnote; /* (byte) Relative note number (signed byte) */
        internal sbyte reserved; /* (byte) Reserved */
        internal string samplename; /* (char) Sample name */

        public XMWavHeader()
        {
        }
    }


    class XMPatHeader
    {
        internal int size; /* (dword) Pattern header length */
        internal short packing; /* (byte) Packing type (always 0) */
        internal short numrows; /* (word) Number of rows in pattern (1..256) */
        internal int packsize; /* (word) Packed patterndata size */
    }

    #endregion
}