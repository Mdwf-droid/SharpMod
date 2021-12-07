using System;
using SharpMod.UniTracker;
using SharpMod.IO;
using SharpMod.Exceptions;
using System.IO;
using SharpMod.Song;

namespace SharpMod.Loaders
{
    /// <summary>
    /// Screamtracker (S3M) module loader
    /// </summary>
    public class S3MLoader : ILoader
    {
        public const String S3M_Version = "Screamtracker 3.xx";

        public S3MNOTE[] s3mbuf; /* pointer to a complete S3M pattern */
        public int[] paraptr; /* parapointer array (see S3M docs) */

        protected internal S3MHEADER mh;

        public short[] remap;    //[32];

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
            this._module = module;

            int i;

            s3mbuf = null;
            paraptr = null;

            //if(!(s3mbuf=(S3MNOTE *)m_.MLoader.MyMalloc(16*64*sizeof(S3MNOTE)))) return 0;
            s3mbuf = new S3MNOTE[16 * 64];

            for (i = 0; i < 16 * 64; i++)
                s3mbuf[i] = new S3MNOTE();
            //if(!(mh=(S3MHEADER *)m_.MLoader.MyCalloc(1,sizeof(S3MHEADER)))) return 0;

            mh = new S3MHEADER();

            mh.t1a = 0;
            mh.type = 0;
            mh.unused1[0] = 0;
            mh.unused1[1] = 0;
            mh.ordnum = 0;
            mh.insnum = 0;
            mh.patnum = 0;
            mh.flags = 0;
            mh.tracker = 0;
            mh.fileformat = 0;
            mh.special = 0;
            mh.mastervol = 0;
            mh.initspeed = 0;
            mh.inittempo = 0;
            mh.mastermult = 0;
            mh.ultraclick = 0;
            mh.pantable = 0;

            /*for (i = 0; i < 28; i++)
                mh.songname[i] = 0;*/
            
            /*for (i = 0; i < 4; i++)
                mh.scrm[i] = 0;

            for (i = 0; i < 8; i++)
                mh.unused2[i] = 0;

            for (i = 0; i < 32; i++)
                mh.channels[i] = 0;*/
           
            mh.unused2.Initialize();
            mh.channels.Initialize();



            return true;
        }

        public bool Test()
        {
            try
            {
                byte[] id = new byte[4];
                //MmIO.Instance.Seek(this.ModStream, 0x2c, SeekEnum.SEEK_SET);
                Reader.Seek(44, SeekOrigin.Begin);
                //if(!fread(id,4,1,this.ModStream)) return 0;
                //if (this.ModStream.read(id, 0, 4) != 4)
                //if (SupportClass.BulkReader.read( this.ModStream,id, 0, 4) != 4)
                //if (this.ModStream.Read(id, 0, 4) != 4)
                if(Reader.Read(id,0,4) != 4)
                    return false;
                //if(!memcmp(id,"SCRM",4)) return 1;
                //if (((char)id[0] == 'S') && ((char)id[1] == 'C') && ((char)id[2] == 'R') && ((char)id[3] == 'M'))
                if (System.Text.UTF8Encoding.UTF8.GetString(id,0,id.Length) == "SCRM") 
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
                //S3MNOTE *n;
                //S3MNOTE dummy;

                /* clear pattern data */

                //memset(s3mbuf,255,16*64*sizeof(S3MNOTE));
                {
                    int i;
                    for (i = 0; i < 16 * 64; i++)
                    {
                        s3mbuf[i].note = (short)(s3mbuf[i].ins = (short)(s3mbuf[i].vol = (short)(s3mbuf[i].cmd = (short)(s3mbuf[i].inf = 255))));
                    }
                }


                while (row < 64)
                {

                    //flag=fgetc(this.ModStream);
                    //flag = this.ModStream.read();
                    flag = Reader.ReadByte(); //this.ModStream.ReadByte();

                    if (flag == -1)
                    {
                        throw new SharpModException(SharpModExceptionResources.ERROR_LOADING_PATTERN);
                    }

                    if (flag != 0)
                    {

                        ch = flag & 31;

                        if (mh.channels[ch] < 16)
                        {
                            //n=&s3mbuf[(64*remap[ch])+row];
                            if ((flag & 32) != 0)
                            {
                                //n.note=fgetc(this.ModStream);
                                s3mbuf[(64 * remap[ch]) + row].note = Reader.ReadUByte();// MmIO.Instance.ReadUByte(this.ModStream);
                                //n.ins=fgetc(this.ModStream);
                                s3mbuf[(64 * remap[ch]) + row].ins = Reader.ReadUByte();//MmIO.Instance.ReadUByte(this.ModStream);
                            }

                            if ((flag & 64) != 0)
                            {
                                //n.vol=fgetc(this.ModStream);
                                s3mbuf[(64 * remap[ch]) + row].vol = Reader.ReadUByte();//MmIO.Instance.ReadUByte(this.ModStream);
                            }

                            if ((flag & 128) != 0)
                            {
                                //n.cmd=fgetc(this.ModStream);
                                //n.inf=fgetc(this.ModStream);
                                s3mbuf[(64 * remap[ch]) + row].cmd = Reader.ReadUByte();//MmIO.Instance.ReadUByte(this.ModStream);
                                s3mbuf[(64 * remap[ch]) + row].inf = Reader.ReadUByte();//MmIO.Instance.ReadUByte(this.ModStream);
                            }
                        }
                        else
                        {
                            //n=&dummy;
                            for (int b = 0; b < ((((flag & 32) != 0) ? 2 : 0) + (((flag & 64) != 0) ? 1 : 0) + (((flag & 128) != 0) ? 2 : 0)); b++)
                            {
                                Reader.ReadUByte();//MmIO.Instance.ReadUByte(this.ModStream);
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


        public virtual short[] S3M_ConvertTrack(S3MNOTE[] tr, int offset)
        {
            int t;

            short note, ins, vol, cmd, inf, lo, hi;

            this.UniTrack.UniReset();
            for (t = offset; t < offset + 64; t++)
            {

                note = tr[t].note;
                ins = tr[t].ins;
                vol = tr[t].vol;
                cmd = tr[t].cmd;
                inf = tr[t].inf;
                lo = (short)(inf & 0xf);
                hi = (short)(inf >> 4);


                //if(ins!=0 && ins!=255){
                if (ins != 0 && ins != 255 && ins != (-1))
                {
                    this.UniTrack.UniInstrument((short)(ins - 1));
                }

                //if(note!=255){
                if ((note != 255) && (note != -1))
                {
                    if (note == 254)
                        this.UniTrack.UniPTEffect((short)0xc, (short)0);
                    /* <- note off command */
                    else
                        this.UniTrack.UniNote((short)((((note & 0xF0) >> 4) * 12) + (note & 0xf)));
                    /* <- normal note */
                }

                //if(vol<255){
                if ((vol < 255) && (vol != -1))
                {
                    this.UniTrack.UniPTEffect((short)0xc, vol);
                    /*			this.UniTrack.UniWrite(this.UniTrack.UNI_S3MVOLUME); */
                    /*			this.UniTrack.UniWrite(vol); */
                }

                if (cmd != 255)
                {
                    switch (cmd)
                    {


                        case 1:
                            this.UniTrack.UniWrite(Effects.UNI_S3MEFFECTA);
                            this.UniTrack.UniWrite(inf);
                            break;


                        case 2:
                            this.UniTrack.UniPTEffect((short)0xb, inf);
                            break;


                        case 3:
                            this.UniTrack.UniPTEffect((short)0xd, inf);
                            break;


                        case 4:
                            this.UniTrack.UniWrite(Effects.UNI_S3MEFFECTD);
                            this.UniTrack.UniWrite(inf);
                            break;


                        case 5:
                            this.UniTrack.UniWrite(Effects.UNI_S3MEFFECTE);
                            this.UniTrack.UniWrite(inf);
                            break;


                        case 6:
                            this.UniTrack.UniWrite(Effects.UNI_S3MEFFECTF);
                            this.UniTrack.UniWrite(inf);
                            break;


                        case 7:
                            this.UniTrack.UniPTEffect((short)0x3, inf);
                            break;


                        case 8:
                            this.UniTrack.UniPTEffect((short)0x4, inf);
                            break;


                        case 9:
                            this.UniTrack.UniWrite(Effects.UNI_S3MEFFECTI);
                            this.UniTrack.UniWrite(inf);
                            break;


                        case (short)(0xa):
                            this.UniTrack.UniPTEffect((short)0x0, inf);
                            break;


                        case (short)(0xb):
                            this.UniTrack.UniPTEffect((short)0x4, (short)0);
                            this.UniTrack.UniWrite(Effects.UNI_S3MEFFECTD);
                            this.UniTrack.UniWrite(inf);
                            break;


                        case (short)(0xc):
                            this.UniTrack.UniPTEffect((short)0x3, (short)0);
                            this.UniTrack.UniWrite(Effects.UNI_S3MEFFECTD);
                            this.UniTrack.UniWrite(inf);
                            break;


                        case (short)(0xf):
                            this.UniTrack.UniPTEffect((short)0x9, (short)inf);
                            break;


                        case (short)(0x11):
                            this.UniTrack.UniWrite(Effects.UNI_S3MEFFECTQ);
                            this.UniTrack.UniWrite(inf);
                            break;


                        case (short)(0x12):
                            this.UniTrack.UniPTEffect((short)0x6, (short)inf);
                            break;


                        case (short)(0x13):
                            switch (hi)
                            {


                                case 0:
                                    this.UniTrack.UniPTEffect((short)0xe, (short)(0x00 | lo));
                                    break;


                                case 1:
                                    this.UniTrack.UniPTEffect((short)0xe, (short)(0x30 | lo));
                                    break;


                                case 2:
                                    this.UniTrack.UniPTEffect((short)0xe, (short)(0x50 | lo));
                                    break;


                                case 3:
                                    this.UniTrack.UniPTEffect((short)0xe, (short)(0x40 | lo));
                                    break;


                                case 4:
                                    this.UniTrack.UniPTEffect((short)0xe, (short)(0x70 | lo));
                                    break;


                                case 8:
                                    this.UniTrack.UniPTEffect((short)0xe, (short)(0x80 | lo));
                                    break;


                                case (short)(0xb):
                                    this.UniTrack.UniPTEffect((short)0xe, (short)(0x60 | lo));
                                    break;


                                case (short)(0xc):
                                    this.UniTrack.UniPTEffect((short)0xe, (short)(0xC0 | lo));
                                    break;


                                case (short)(0xd):
                                    this.UniTrack.UniPTEffect((short)0xe, (short)(0xD0 | lo));
                                    break;


                                case (short)(0xe):
                                    this.UniTrack.UniPTEffect((short)0xe, (short)(0xE0 | lo));
                                    break;
                            }
                            break;


                        case (short)(0x14):
                            if (inf > 0x20)
                            {
                                this.UniTrack.UniWrite(Effects.UNI_S3MEFFECTT);
                                this.UniTrack.UniWrite(inf);
                            }
                            break;


                        case (short)(0x18):
                            this.UniTrack.UniPTEffect((short)0x8, (short)inf);
                            break;
                    }
                }

                this.UniTrack.UniNewline();
            }
            return this.UniTrack.UniDup();
        }


        public bool Load()
        {
            try
            {

                int t, u = 0;
                //INSTRUMENT *d;
                //SAMPLE *q;
                int inst_num;
                short[] isused = new short[16];
                sbyte[] pan = new sbyte[32];

                /* try to read module header */

                mh.songname = Reader.ReadString(28); //MmIO.Instance.ReadStringSBytes(mh.songname, 28, this.ModStream);
                mh.t1a = Reader.ReadSByte();// MmIO.Instance.ReadSByte(this.ModStream);
                mh.type = Reader.ReadSByte();// MmIO.Instance.ReadSByte(this.ModStream);
                Reader.ReadSBytes(mh.unused1, 2);  //MmIO.Instance.ReadSBytes(mh.unused1, 2, this.ModStream);
                mh.ordnum = Reader.ReadIntelUWord();//MmIO.Instance._mm_read_I_UWORD(this.ModStream);
                mh.insnum = Reader.ReadIntelUWord();//MmIO.Instance._mm_read_I_UWORD(this.ModStream);
                mh.patnum = Reader.ReadIntelUWord();//MmIO.Instance._mm_read_I_UWORD(this.ModStream);
                mh.flags = Reader.ReadIntelUWord();//MmIO.Instance._mm_read_I_UWORD(this.ModStream);
                mh.tracker = Reader.ReadIntelUWord();//MmIO.Instance._mm_read_I_UWORD(this.ModStream);
                mh.fileformat = Reader.ReadIntelUWord();//MmIO.Instance._mm_read_I_UWORD(this.ModStream);
                mh.scrm = Reader.ReadString(4);// MmIO.Instance.ReadStringSBytes(mh.scrm, 4, this.ModStream);

                mh.mastervol = Reader.ReadUByte();//MmIO.Instance.ReadUByte(this.ModStream);
                mh.initspeed = Reader.ReadUByte();//MmIO.Instance.ReadUByte(this.ModStream);
                mh.inittempo = Reader.ReadUByte();//MmIO.Instance.ReadUByte(this.ModStream);
                mh.mastermult = Reader.ReadUByte();//MmIO.Instance.ReadUByte(this.ModStream);
                mh.ultraclick = Reader.ReadUByte();//MmIO.Instance.ReadUByte(this.ModStream);
                mh.pantable = Reader.ReadUByte();//MmIO.Instance.ReadUByte(this.ModStream);
                Reader.ReadSBytes(mh.unused2, 8); //MmIO.Instance.ReadSBytes(mh.unused2, 8, this.ModStream);
                mh.special = Reader.ReadIntelUWord();//MmIO.Instance._mm_read_I_UWORD(this.ModStream);
                Reader.ReadUBytes(mh.channels, 32);// MmIO.Instance.ReadUByteS2(mh.channels, 32, this.ModStream);

                //if(feof(this.ModStream)){
                //if (this.ModStream.FilePointer >= this.ModStream.length())
                if (Reader.isEOF())
                {
                    throw new SharpModException(SharpModExceptionResources.ERROR_LOADING_HEADER);
                }

                /* set module variables */

                this._module.ModType = new System.String(S3M_Version.ToCharArray());
		this._module.SongName = mh.songname;//System.Text.UTF8Encoding.UTF8.GetString((byte[])(Array)mh.songname, 0, 28);// m_.MLoader.DupStr(mh.songname, 28); /* make a cstr this.UniModule songname */
                //this._module.NumPat = (short)mh.patnum;
                //this._module.NumIns = (short)mh.insnum;
                this._module.InitialSpeed = mh.initspeed;
                this._module.InitialTempo = mh.inittempo;

                // count the number this.UniModule channels used
                this._module.ChannelsCount = 0;

                /*      for(t=0;t<32;t++) printf("%2.2x ",mh.channels[t]);*/
                for (t = 0; t < 32; t++)
                    remap[t] = 0;
                for (t = 0; t < 16; t++)
                    isused[t] = 0;

                // set a flag for each channel (1 out this.UniModule this.UniModule 16) thats being used: 

                for (t = 0; t < 32; t++)
                {
                    if (mh.channels[t] < 16)
                    {
                        isused[mh.channels[t]] = 1;
                    }
                }

                // give each this.UniModule them a different number
                for (t = 0; t < 16; t++)
                {
                    if (isused[t] != 0)
                    {
                        isused[t] = (short)this._module.ChannelsCount;
                        this._module.ChannelsCount++;
                    }
                }

                /* build the remap array */

                for (t = 0; t < 32; t++)
                {
                    if (mh.channels[t] < 16)
                    {
                        remap[t] = isused[mh.channels[t]];
                    }
                }

                /* set panning positions */

                for (t = 0; t < 32; t++)
                {
                    if (mh.channels[t] < 16)
                    {
                        if (mh.channels[t] < 8)
                        {
                            this._module.Panning[remap[t]] = 0x30;
                        }
                        else
                        {
                            this._module.Panning[remap[t]] = 0xc0;
                        }
                    }
                }

                //this.UniModule.NumTrk = (short)(this.UniModule.NumPat * this.UniModule.NumChn);

                /*      printf("Uses %d channels\n",this.UniModule.numchn);*/
                /* read the order data */

                short[] tmp = new short[mh.ordnum];
                Reader.ReadUBytes(tmp/*this._module.Positions*/, mh.ordnum); //MmIO.Instance.ReadUByteS2(this.UniModule.Positions, mh.ordnum, this.ModStream);

                foreach (short pos in tmp)
                    this._module.Positions.Add(pos);

                /*this._module.NumPos = 0;
                for (t = 0; t < mh.ordnum; t++)
                {
                    this._module.Positions[this._module.NumPos] = this._module.Positions[t];
                    if (this._module.Positions[t] < 254)
                        this._module.NumPos++;
                }*/

                //if((paraptr=(int *)m_.MLoader.MyMalloc((this.UniModule.numins+this.UniModule.numpat)*sizeof(int)))==null) return 0;
                paraptr = new int[mh.insnum + mh.patnum]; //new int[this.UniModule.NumIns + this.UniModule.NumPat];

                /* read the instrument+pattern parapointers */

                Reader.ReadIntelUWords(paraptr, mh.insnum + mh.patnum /*this.UniModule.NumIns + this.UniModule.NumPat*/); //MmIO.Instance._mm_read_I_UWORDS2(paraptr, this.UniModule.NumIns + this.UniModule.NumPat, this.ModStream);

                /*      printf("pantab %d\n",mh.pantable);*/
                if (mh.pantable == 252)
                {

                    /* read the panning table */

                    Reader.ReadSBytes(pan, 32); //MmIO.Instance.ReadSBytes(pan, 32, this.ModStream);

                    /* set panning positions according to panning table (new for st3.2) */

                    for (t = 0; t < 32; t++)
                    {
                        if (((pan[t] & 0x20) != 0) && mh.channels[t] < 16)
                        {
                            this._module.Panning[remap[t]] = (short)((pan[t] & 0xf) << 4);
                        }
                    }
                }

                /* now is a good time to check if the header was too short :) */

                //if(feof(this.ModStream)){
                //if (this.ModStream.FilePointer >= this.ModStream.length())
                if (Reader.isEOF())
                {
                    throw new SharpModException(SharpModExceptionResources.ERROR_LOADING_HEADER);
                    
                }

               if (AllocInstruments != null && !AllocInstruments(_module,mh.insnum))
                    return false;

                //d=this.UniModule.instruments;
                inst_num = 0;

                for (t = 0; t < this.mh.insnum; t++)
                {
                    S3MSAMPLE s = new S3MSAMPLE();

                    this._module.Instruments[inst_num].NumSmp = 1;
                    /*if (AllocSamples != null && !AllocSamples(this.UniModule.Instruments[inst_num]))
                        return false;*/
                    //q=this.UniModule.instruments[inst_num].samples;

                    /* seek to instrument position */

                    Reader.Seek(((int)paraptr[t]) << 4, SeekOrigin.Begin); //MmIO.Instance.Seek(this.ModStream, ((int)paraptr[t]) << 4, SeekEnum.SEEK_SET);

                    /* and load sample info */

                    s.type = Reader.ReadUByte();// MmIO.Instance.ReadUByte(this.ModStream);
                    s.filename = Reader.ReadString(12);// MmIO.Instance.ReadStringSBytes(s.filename, 12, this.ModStream);
                    s.memsegh = Reader.ReadUByte();//MmIO.Instance.ReadUByte(this.ModStream);
                    s.memsegl = Reader.ReadIntelUWord();//MmIO.Instance._mm_read_I_UWORD(this.ModStream);
                    s.length = Reader.ReadIntelULong();//MmIO.Instance._mm_read_I_ULONG(this.ModStream);
                    s.loopbeg = Reader.ReadIntelULong();//MmIO.Instance._mm_read_I_ULONG(this.ModStream);
                    s.loopend = Reader.ReadIntelULong();//MmIO.Instance._mm_read_I_ULONG(this.ModStream);
                    s.volume = Reader.ReadUByte();//MmIO.Instance.ReadUByte(this.ModStream);
                    s.dsk = Reader.ReadUByte();//MmIO.Instance.ReadUByte(this.ModStream);
                    s.pack = Reader.ReadUByte();//MmIO.Instance.ReadUByte(this.ModStream);
                    s.flags = Reader.ReadUByte();//MmIO.Instance.ReadUByte(this.ModStream);
                    s.c2spd = Reader.ReadIntelULong(); //MmIO.Instance._mm_read_I_ULONG(this.ModStream);
                    Reader.ReadSBytes(s.unused, 12); //MmIO.Instance.ReadSBytes(s.unused, 12, this.ModStream);
                    s.sampname = Reader.ReadString(28);// MmIO.Instance.ReadStringSBytes(s.sampname, 28, this.ModStream);
                    s.scrs = Reader.ReadString(4);// MmIO.Instance.ReadStringSBytes(s.scrs, 4, this.ModStream);

                    //if(feof(this.ModStream)){
                    //if (this.ModStream.FilePointer >= this.ModStream.length())
                    if (Reader.isEOF())
                    {
                        throw new SharpModException(SharpModExceptionResources.ERROR_LOADING_HEADER);
                    }

                    this._module.Instruments[inst_num].Samples.Add(new Sample());

                    this._module.Instruments[inst_num].InsName = s.sampname; //System.Text.UTF8Encoding.UTF8.GetString((byte[])(Array)s.sampname, 0, 28);// m_.MLoader.DupStr(s.sampname, 28);
                    this._module.Instruments[inst_num].Samples[0].C2Spd = s.c2spd;
                    this._module.Instruments[inst_num].Samples[0].Length = s.length;
                    this._module.Instruments[inst_num].Samples[0].LoopStart = s.loopbeg;
                    this._module.Instruments[inst_num].Samples[0].LoopEnd = s.loopend;
                    this._module.Instruments[inst_num].Samples[0].Volume = s.volume;
                    this._module.Instruments[inst_num].Samples[0].SeekPos = (((int)s.memsegh) << 16 | s.memsegl) << 4;

                    this._module.Instruments[inst_num].Samples[0].Flags = 0;

                    if ((s.flags & 1) != 0)
                        this._module.Instruments[inst_num].Samples[0].Flags |= (SampleFormatFlags.SF_LOOP);
                    if ((s.flags & 4) != 0)
                        this._module.Instruments[inst_num].Samples[0].Flags |= (SampleFormatFlags.SF_16BITS);
                    if (mh.fileformat == 1)
                        this._module.Instruments[inst_num].Samples[0].Flags |= (SampleFormatFlags.SF_SIGNED);

                    this._module.Instruments[inst_num].Samples[0].SampleRate = 22050;
                    /* DON'T load sample if it doesn't have the SCRS tag */

                    //if(memcmp(s.scrs,"SCRS",4)!=0) this.UniModule.instruments[inst_num].samples[0].length=0;
                    if (s.scrs.Length <4 || (!(((char)s.scrs[0] == 'S') && ((char)s.scrs[1] == 'C') && ((char)s.scrs[2] == 'R') && ((char)s.scrs[3] == 'S'))))
                    {
                        this._module.Instruments[inst_num].Samples[0].Length = 0;
                    }

                    /*              printf("%s\n",this.UniModule.instruments[inst_num].insname);*/
                    //d++;
                    inst_num++;
                }

                /*if (this.AllocPatterns != null && !AllocPatterns())
                    return false;
                if (this.AllocTracks != null && !AllocTracks())
                    return false;*/

                for (t = 0; t < this.mh.patnum; t++)
                {
                    if (this.AllocPatterns != null && !AllocPatterns(_module,t,64))
                        return false;
                    if (this.AllocTracks != null && !AllocTracks(_module.Patterns[t],_module.ChannelsCount))
                        return false;

                    /* seek to pattern position ( + 2 skip pattern length ) */

                    //MmIO.Instance.Seek(this.ModStream, (((int)paraptr[this.UniModule.NumIns + t]) << 4) + 2, SeekEnum.SEEK_SET);
                    Reader.Seek((((int)paraptr[this.mh.insnum+ t]) << 4) + 2, SeekOrigin.Begin);
                    if (!S3M_ReadPattern())
                        return false;

                    for (u = 0; u < this._module.ChannelsCount; u++)
                    {
                        this._module.Patterns[t].Tracks[u].UniTrack = S3M_ConvertTrack(s3mbuf, u * 64);
                        //if ((this.UniModule.Tracks[track++] = S3M_ConvertTrack(s3mbuf, u * 64)) == null)
                        //    return false;
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
        public event AllocSamplesHandler AllocSamples;




        #endregion
    }

    #region Module Internals structures
    public class S3MNOTE
    {
        public short note;
        public short ins;
        public short vol;
        public short cmd;
        public short inf;
    }

    /* Raw S3M header struct: */

    public class S3MHEADER
    {
        public string songname;
        public sbyte t1a;
        public sbyte type;
        public sbyte[] unused1;
        public int ordnum;
        public int insnum;
        public int patnum;
        public int flags;
        public int tracker;
        public int fileformat;
        public string scrm;
        public short mastervol;
        public short initspeed;
        public short inittempo;
        public short mastermult;
        public short ultraclick;
        public short pantable;
        public sbyte[] unused2;
        public int special;
        public short[] channels;

        public S3MHEADER()
        {
            songname = String.Empty;
            unused1 = new sbyte[2];
            scrm = String.Empty;
            unused2 = new sbyte[8];
            channels = new short[32];
        }
    }

    /* Raw S3M sampleinfo struct: */

    class S3MSAMPLE
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

        public S3MSAMPLE()
        {
            filename = string.Empty;
            unused = new sbyte[12];
            sampname = string.Empty;
            scrs = string.Empty;
        }
    }
    #endregion

}