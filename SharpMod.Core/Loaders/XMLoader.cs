using System;
using System.IO;
using SharpMod.Exceptions;
using SharpMod.IO;
using SharpMod.UniTracker;
using SharpMod.Song;

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

        private UniTrk _uniTrack;
        private SongModule _module;
        private Stream _modStream;


        public XMNOTE[] xmpat;
        public XMHEADER mh;

        public String LoaderType
        {
            get
            {
                return "XM";
            }
        }

        public String LoaderVersion
        {
            get
            {
                return "Portable XM loader v0.4 - for your ears only / MikMak";
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
        }*/

        public ModBinaryReader Reader { get; set; }



        public XMLoader()
        {
            mh = null;

        }


        public bool Test()
        {
            try
            {
                byte[] id = new byte[17];
                byte[] should_be = new byte[20];

                System.String szShould = "Extended Module: ";
                //SupportClass.GetSBytesFromString(szShould, 0, 17, ref should_be, 0);
                should_be = System.Text.UTF8Encoding.UTF8.GetBytes(szShould);
                int a;
                //if(!fread(id,17,1,this.ModStream)) return 0;
                //if (!this.ModStream.read(id,0,17)) return 0;
                //if (this.ModStream.read(id, 0, 17) != 17)
                //if (SupportClass.BulkReader.read(this.ModStream, id, 0, 17) != 17)
                //if (this.ModStream.Read(id, 0, 17) != 17)
                if (Reader.Read(id, 0, 17) != 17)
                    return false;
                for (a = 0; a < 17; a++)
                {
                    if (id[a] != should_be[a])
                        return false;
                }
                return true;
            }
            catch (System.IO.IOException)
            {
                return false;
            }
        }


        public bool Init(SongModule module)
        {
            this._module = module;
            
            mh = null;
            //if(!(mh=(XMHEADER *)m_.MLoader.MyCalloc(1,sizeof(XMHEADER)))) return 0;
            mh = new XMHEADER();

            mh.version = mh.headersize = mh.restart = mh.tempo = mh.bpm = 0;
            mh.songlength = (short)(mh.numchn = (short)(mh.numpat = (short)(mh.numins = (short)(mh.flags = (short)0))));

           /* for (i = 0; i < 17; i++)
                mh.id[i] = 0;
            for (i = 0; i < 21; i++)
                mh.songname[i] = 0;
            for (i = 0; i < 20; i++)
                mh.trackername[i] = 0;
            for (i = 0; i < 256; i++)
                mh.orders[i] = 0;*/
            mh.orders.Initialize();

            return true;
        }


        public void Cleanup()
        {
            if (mh != null)
                mh = null;
        }


        public virtual void XM_ReadNote(XMNOTE n)
        {
            try
            {
                short cmp;
                //memset(n,0,sizeof(XMNOTE));
                n.note = (short)(n.ins = (short)(n.vol = (short)(n.eff = (short)(n.dat = 0))));

                //cmp=fgetc(this.ModStream);
                //cmp = (short) this.ModStream.read();
                cmp = Reader.ReadByte();//(short)this.ModStream.ReadByte(); //SupportClass.BulkReader.read(this.ModStream);

                if ((cmp & 0x80) != 0)
                {
                    //					if ((cmp & 1) != 0)
                    //						n.note = (short) this.ModStream.read();
                    //					if ((cmp & 2) != 0)
                    //						n.ins = (short) this.ModStream.read();
                    //					if ((cmp & 4) != 0)
                    //						n.vol = (short) this.ModStream.read();
                    //					if ((cmp & 8) != 0)
                    //						n.eff = (short) this.ModStream.read();
                    //					if ((cmp & 16) != 0)
                    //						n.dat = (short) this.ModStream.read();
                    if ((cmp & 1) != 0)
                        n.note = Reader.ReadByte(); //(short)this.ModStream.ReadByte();//SupportClass.BulkReader.read(this.ModStream);
                    if ((cmp & 2) != 0)
                        n.ins = Reader.ReadByte(); //(short)this.ModStream.ReadByte();//SupportClass.BulkReader.read(this.ModStream);
                    if ((cmp & 4) != 0)
                        n.vol = Reader.ReadByte(); //(short)this.ModStream.ReadByte();//SupportClass.BulkReader.read(this.ModStream);
                    if ((cmp & 8) != 0)
                        n.eff = Reader.ReadByte(); //(short)this.ModStream.ReadByte();//SupportClass.BulkReader.read(this.ModStream);
                    if ((cmp & 16) != 0)
                        n.dat = Reader.ReadByte(); //(short)this.ModStream.ReadByte();//SupportClass.BulkReader.read(this.ModStream);

                }
                else
                {
                    n.note = cmp;
                    //					n.ins = (short) this.ModStream.read();
                    //					n.vol = (short) this.ModStream.read();
                    //					n.eff = (short) this.ModStream.read();
                    //					n.dat = (short) this.ModStream.read();
                    n.ins = Reader.ReadByte(); //(short)this.ModStream.ReadByte();//SupportClass.BulkReader.read(this.ModStream);
                    n.vol = Reader.ReadByte(); //(short)this.ModStream.ReadByte();//SupportClass.BulkReader.read(this.ModStream);
                    n.eff = Reader.ReadByte(); //(short)this.ModStream.ReadByte();//SupportClass.BulkReader.read(this.ModStream);
                    n.dat = Reader.ReadByte(); //(short)this.ModStream.ReadByte();//SupportClass.BulkReader.read(this.ModStream);
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
            catch (System.IO.IOException)
            {
            }
        }


        public virtual short[] XM_Convert(XMNOTE[] xmtrack, int offset, int rows)
        {
            int t;
            short note, ins, vol, eff, dat;

            this.UniTrack.UniReset();

            int xmi = offset;

            for (t = 0; t < rows; t++)
            {

                note = xmtrack[xmi].note;
                ins = xmtrack[xmi].ins;
                vol = xmtrack[xmi].vol;
                eff = xmtrack[xmi].eff;
                dat = xmtrack[xmi].dat;

                if (note != 0)
                    this.UniTrack.UniNote((short)(note - 1));

                if (ins != 0)
                    this.UniTrack.UniInstrument((short)(ins - 1));

                /*              printf("Vol:%d\n",vol); */

                switch (vol >> 4)
                {


                    case (short)(0x6):
                        if ((vol & 0xf) != 0)
                        {
                            this.UniTrack.UniWrite(Effects.UNI_XMEFFECTA);
                            this.UniTrack.UniWrite((short)(vol & 0xf));
                        }
                        break;


                    case (short)(0x7):
                        if ((vol & 0xf) != 0)
                        {
                            this.UniTrack.UniWrite(Effects.UNI_XMEFFECTA);
                            this.UniTrack.UniWrite((short)(vol << 4));
                        }
                        break;

                    /* volume-row fine volume slide is compatible with protracker
                    EBx and EAx effects i.e. a zero nibble means DO NOT SLIDE, as
                    opposed to 'take the last sliding value'.
                    */


                    case (short)(0x8):
                        this.UniTrack.UniPTEffect((short)0xe, (short)(0xb0 | (vol & 0xf)));
                        break;


                    case (short)(0x9):
                        this.UniTrack.UniPTEffect((short)0xe, (short)(0xa0 | (vol & 0xf)));
                        break;


                    case (short)(0xa):
                        this.UniTrack.UniPTEffect((short)0x4, (short)(vol << 4));
                        break;


                    case (short)(0xb):
                        this.UniTrack.UniPTEffect((short)0x4, (short)(vol & 0xf));
                        break;


                    case (short)(0xc):
                        this.UniTrack.UniPTEffect((short)0x8, (short)(vol << 4));
                        break;


                    case (short)(0xd):

                        if ((vol & 0xf) != 0)
                        {
                            this.UniTrack.UniWrite(Effects.UNI_XMEFFECTP);
                            this.UniTrack.UniWrite((short)(vol & 0xf));
                        }
                        break;


                    case (short)(0xe):

                        if ((vol & 0xf) != 0)
                        {
                            this.UniTrack.UniWrite(Effects.UNI_XMEFFECTP);
                            this.UniTrack.UniWrite((short)(vol << 4));
                        }
                        break;


                    case (short)(0xf):
                        this.UniTrack.UniPTEffect((short)0x3, (short)(vol << 4));
                        break;


                    default:
                        if (vol >= 0x10 && vol <= 0x50)
                        {
                            this.UniTrack.UniPTEffect((short)0xc, (short)(vol - 0x10));
                        }
                        break;

                }

                /*              if(eff>0xf) printf("Effect %d",eff); */

                switch (eff)
                {


                    case 'G' - 55:
                        if (dat > 64)
                            dat = 64;
                        this.UniTrack.UniWrite(Effects.UNI_XMEFFECTG);
                        this.UniTrack.UniWrite(dat);
                        break;


                    case 'H' - 55:
                        this.UniTrack.UniWrite(Effects.UNI_XMEFFECTH);
                        this.UniTrack.UniWrite(dat);
                        break;


                    case 'K' - 55:
                        this.UniTrack.UniNote((short)96);
                        break;


                    case 'L' - 55:
                        break;


                    case 'P' - 55:
                        this.UniTrack.UniWrite(Effects.UNI_XMEFFECTP);
                        this.UniTrack.UniWrite(dat);
                        break;


                    case 'R' - 55:
                        this.UniTrack.UniWrite(Effects.UNI_S3MEFFECTQ);
                        this.UniTrack.UniWrite(dat);
                        break;


                    case 'T' - 55:
                        this.UniTrack.UniWrite(Effects.UNI_S3MEFFECTI);
                        this.UniTrack.UniWrite(dat);
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
                            this.UniTrack.UniWrite(Effects.UNI_XMEFFECTA);
                            this.UniTrack.UniWrite(dat);
                        }
                        else if (eff <= 0xf)
                            this.UniTrack.UniPTEffect(eff, dat);
                        break;

                }

                this.UniTrack.UniNewline();
                xmi++;
            }
            return this.UniTrack.UniDup();
        }



        public bool Load()
        {
            try
            {
                //INSTRUMENT *d;
                //SAMPLE *q;
                int inst_num;
                int t, u, v, p;
                int next;
                int i;

                /* try to read module header */
                
                //MmIO.Instance.ReadStringSBytes(mh.id, 17, this.ModStream);
                mh.id = Reader.ReadString(17);
                //MmIO.Instance.ReadStringSBytes(mh.songname, 21, this.ModStream);
                mh.songname = Reader.ReadString(21);
                //MmIO.Instance.ReadStringSBytes(mh.trackername, 20, this.ModStream);
                mh.trackername = Reader.ReadString(20);

                /*mh.version = MmIO.Instance._mm_read_I_UWORD(this.ModStream);
                mh.headersize = MmIO.Instance._mm_read_I_ULONG(this.ModStream);
                mh.songlength = (short)MmIO.Instance._mm_read_I_UWORD(this.ModStream);
                mh.restart = MmIO.Instance._mm_read_I_UWORD(this.ModStream);
                mh.numchn = (short)MmIO.Instance._mm_read_I_UWORD(this.ModStream);
                mh.numpat = (short)MmIO.Instance._mm_read_I_UWORD(this.ModStream);
                mh.numins = (short)MmIO.Instance._mm_read_I_UWORD(this.ModStream);
                mh.flags = (short)MmIO.Instance._mm_read_I_UWORD(this.ModStream);
                mh.tempo = MmIO.Instance._mm_read_I_UWORD(this.ModStream);
                mh.bpm = MmIO.Instance._mm_read_I_UWORD(this.ModStream);
                MmIO.Instance.ReadUByteS2(mh.orders, 256, this.ModStream);*/

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
                

                //if(feof(this.ModStream)){
                //if (this.ModStream.FilePointer >= this.ModStream.length())
                if (Reader.isEOF())
                {
                    /*MmIO.Instance.myerr = m_.ERROR_LOADING_HEADER;
                    return false;*/
                    throw new SharpModException(SharpModExceptionResources.ERROR_LOADING_HEADER);
                }

                /* set module variables */

                this._module.InitialSpeed = (short)mh.tempo;
                this._module.InitialTempo = (short)mh.bpm;
                //this.UniModule.ModType = m_.MLoader.DupStr(mh.trackername, 20);
                this._module.ModType = mh.trackername;// System.Text.UTF8Encoding.UTF8.GetString((byte[])(Array)mh.trackername, 0, 20);
                this._module.ChannelsCount = mh.numchn;
                //this._module.NumPat = mh.numpat;
                //this._module.NumTrk = (short)(this._module.NumPat * this._module.NumChn); /* get number of channels */
                //this.UniModule.SongName = m_.MLoader.DupStr(mh.songname, 20); /* make a cstr of songname */
                this._module.SongName = mh.songname; //System.Text.UTF8Encoding.UTF8.GetString((byte[])(Array)mh.songname, 0, 20); /* make a cstr of songname */
                //this._module.NumPos = mh.songlength; /* copy the songlength */
                this._module.RepPos = (short)mh.restart;
                //this._module.NumIns = mh.numins;
                this._module.Flags |= UniModFlags.UF_XMPERIODS;
                if ((mh.flags & 1) != 0)
                    this._module.Flags |= UniModFlags.UF_LINEAR;

                //memcpy(this.UniModule.positions,mh.orders,256);po
                _module.Positions = new System.Collections.Generic.List<int>(mh.songlength);
                for (t = 0; t < 256; t++)
                    if (t >= mh.songlength)
                        break;
                    else
                    this._module.Positions.Add(mh.orders[t]);
                //    this.UniModule.Positions[t] = mh.orders[t];
                

                /*
                WHY THIS CODE HERE?? I CAN'T REMEMBER!
				
                this.UniModule.numpat=0;
                for(t=0;t<this.UniModule.numpos;t++){
                if(this.UniModule.positions[t]>this.UniModule.numpat) this.UniModule.numpat=this.UniModule.positions[t];
                }
                this.UniModule.numpat++;*/

                /*if (AllocTracks != null && !AllocTracks())
                    return false;
                if (AllocPatterns != null && !AllocPatterns())
                    return false;*/

                //int numtrk = 0;

               

                for (t = 0; t < mh.numpat; t++)
                {
                    XMPATHEADER ph = new XMPATHEADER();

                    /*		printf("Reading pattern %d\n",t); */

                    ph.size = Reader.ReadIntelULong(); //MmIO.Instance._mm_read_I_ULONG(this.ModStream);
                    ph.packing = Reader.ReadUByte();//MmIO.Instance.ReadUByte(this.ModStream);
                    ph.numrows = (short)Reader.ReadIntelUWord(); //MmIO.Instance._mm_read_I_UWORD(this.ModStream);
                    ph.packsize = Reader.ReadIntelUWord();// MmIO.Instance._mm_read_I_UWORD(this.ModStream);

                    /*		printf("headln:  %ld\n",ph.size); */
                    /*		printf("numrows: %d\n",ph.numrows); */
                    /*		printf("packsize:%d\n",ph.packsize); */

                    //TODO this.UniModule.PattRows[t] = ph.numrows;
                    if (this.AllocPatterns != null && !AllocPatterns(_module, t, ph.numrows))
                        return false;
                    if (this.AllocTracks != null && !AllocTracks(_module.Patterns[t],_module.ChannelsCount))
                        return false;

                    /*
                    Gr8.. when packsize is 0, don't try to load a pattern.. it's empty.
                    This bug was discovered thanks to Khyron's module..
                    */

                    //if(!(xmpat=(XMNOTE *)m_.MLoader.MyCalloc(ph.numrows*this.UniModule.numchn,sizeof(XMNOTE)))) return false;
                    xmpat = new XMNOTE[ph.numrows * this._module.ChannelsCount];
                    for (i = 0; i < ph.numrows * this._module.ChannelsCount; i++)
                        xmpat[i] = new XMNOTE();

                    for (i = 0; i < ph.numrows * this._module.ChannelsCount; i++)
                    {
                        xmpat[i].note = (short)(xmpat[i].ins = (short)(xmpat[i].vol = (short)(xmpat[i].eff = (short)(xmpat[i].dat = 0))));
                    }


                    if (ph.packsize > 0)
                    {
                        for (u = 0; u < ph.numrows; u++)
                        {
                            for (v = 0; v < this._module.ChannelsCount; v++)
                            {
                                XM_ReadNote(xmpat[(v * ph.numrows) + u]);
                            }
                        }
                    }

                    for (v = 0; v < this._module.ChannelsCount; v++)
                    {
                        //this.UniModule.Tracks[numtrk++] = XM_Convert(xmpat, v * ph.numrows, ph.numrows);
                        this._module.Patterns[t].Tracks[v].Cells = new System.Collections.Generic.List<PatternCell>(new PatternCell[ph.numrows]);
                        this._module.Patterns[t].Tracks[v].UniTrack = XM_Convert(xmpat, v * ph.numrows, ph.numrows);
                    }

                    xmpat = null;
                }

                if (AllocInstruments != null && !AllocInstruments(_module,mh.numins))
                    return false;

                //d=this.UniModule.instruments;
                inst_num = 0;

                for (t = 0; t < this._module.Instruments.Count; t++)
                {
                    XMINSTHEADER ih = new XMINSTHEADER();

                    /* read instrument header */

                    ih.size = Reader.ReadIntelULong(); //MmIO.Instance._mm_read_I_ULONG(this.ModStream);
                    //MmIO.Instance.ReadStringSBytes(ih.name, 22, this.ModStream);
                    ih.name = Reader.ReadString(22);
                    ih.type = Reader.ReadUByte(); //MmIO.Instance.ReadUByte(this.ModStream);
                    ih.numsmp = (short)Reader.ReadIntelUWord();//MmIO.Instance._mm_read_I_UWORD(this.ModStream);
                    ih.ssize = Reader.ReadIntelULong();//MmIO.Instance._mm_read_I_ULONG(this.ModStream);

                    /*      printf("Size: %ld\n",ih.size);
                    printf("Name: 	%22.22s\n",ih.name);
                    printf("Samples:%d\n",ih.numsmp);
                    printf("sampleheadersize:%ld\n",ih.ssize);*/
                    //this.UniModule.Instruments[inst_num].InsName = m_.MLoader.DupStr(ih.name, 22);
                    this._module.Instruments[inst_num].InsName = ih.name;//System.Text.UTF8Encoding.UTF8.GetString((byte[])(Array)ih.name, 0, 22);
                    this._module.Instruments[inst_num].NumSmp = ih.numsmp;

                   /* if (AllocSamples != null && !AllocSamples((this.UniModule.Instruments[inst_num])))
                        return false;*/

                    if (ih.numsmp > 0)
                    {
                        XMPATCHHEADER pth = new XMPATCHHEADER();
                        XMWAVHEADER wh = new XMWAVHEADER();

                        Reader.ReadUBytes(pth.what,96);//MmIO.Instance.ReadUByteS2(pth.what, 96, this.ModStream);
                        Reader.ReadUBytes(pth.volenv, 48); //MmIO.Instance.ReadUByteS2(pth.volenv, 48, this.ModStream);
                        Reader.ReadUBytes(pth.panenv, 48); //MmIO.Instance.ReadUByteS2(pth.panenv, 48, this.ModStream);
                        pth.volpts = Reader.ReadUByte();//MmIO.Instance.ReadUByte(this.ModStream);
                        pth.panpts = Reader.ReadUByte();//MmIO.Instance.ReadUByte(this.ModStream);
                        pth.volsus = Reader.ReadUByte();//MmIO.Instance.ReadUByte(this.ModStream);
                        pth.volbeg = Reader.ReadUByte();//MmIO.Instance.ReadUByte(this.ModStream);
                        pth.volend = Reader.ReadUByte();//MmIO.Instance.ReadUByte(this.ModStream);
                        pth.pansus = Reader.ReadUByte();//MmIO.Instance.ReadUByte(this.ModStream);
                        pth.panbeg = Reader.ReadUByte();//MmIO.Instance.ReadUByte(this.ModStream);
                        pth.panend = Reader.ReadUByte();//MmIO.Instance.ReadUByte(this.ModStream);
                        pth.volflg = Reader.ReadUByte();//MmIO.Instance.ReadUByte(this.ModStream);
                        pth.panflg = Reader.ReadUByte();//MmIO.Instance.ReadUByte(this.ModStream);
                        pth.vibflg = Reader.ReadUByte();//MmIO.Instance.ReadUByte(this.ModStream);
                        pth.vibsweep = Reader.ReadUByte();//MmIO.Instance.ReadUByte(this.ModStream);
                        pth.vibdepth = Reader.ReadUByte();//MmIO.Instance.ReadUByte(this.ModStream);
                        pth.vibrate = Reader.ReadUByte();//MmIO.Instance.ReadUByte(this.ModStream);
                        pth.volfade = Reader.ReadIntelUWord();//MmIO.Instance._mm_read_I_UWORD(this.ModStream);
                        Reader.readIntelSWords(pth.reserved, 11);//MmIO.Instance._mm_read_I_SWORDS(pth.reserved, 11, this.ModStream);

                        //memcpy(this.UniModule.instruments[inst_num].samplenumber,pth.what,96);
                        for (i = 0; i < 96; i++)
                        {
                            this._module.Instruments[inst_num].SampleNumber[i] = pth.what[i];
                        }

                        this._module.Instruments[inst_num].VolFade = pth.volfade;

                        /*			printf("Volfade %x\n",this.UniModule.instruments[inst_num].volfade); */

                        //memcpy(this.UniModule.instruments[inst_num].volenv,pth.volenv,24);
                        for (i = 0; i < 6; i++)
                        {
                            this._module.Instruments[inst_num].VolEnv[i].Pos = (short)(pth.volenv[i * 4] + (pth.volenv[i * 4 + 1] << 8));
                            this._module.Instruments[inst_num].VolEnv[i].Val = (short)(pth.volenv[i * 4 + 2] + (pth.volenv[i * 4 + 3] << 8));
                        }
                        /*
                        for (i = 0; i < 12; i++)
                        {
                        byte tmp = ((byte*)this.UniModule.instruments[inst_num].volenv)[i*2];
						
                        ((byte*)this.UniModule.instruments[inst_num].volenv)[i*2] = ((byte*)this.UniModule.instruments[inst_num].volenv)[i*2+1];
                        ((byte*)this.UniModule.instruments[inst_num].volenv)[i*2+1] = tmp;
                        }*/

                        this._module.Instruments[inst_num].VolFlg = pth.volflg;
                        this._module.Instruments[inst_num].VolSus = pth.volsus;
                        this._module.Instruments[inst_num].VolBeg = pth.volbeg;
                        this._module.Instruments[inst_num].VolEnd = pth.volend;
                        this._module.Instruments[inst_num].VolPts = pth.volpts;

                        /*			printf("volume points	: %d\n"
                        "volflg			: %d\n"
                        "volbeg			: %d\n"
                        "volend			: %d\n"
                        "volsus			: %d\n",
                        this.UniModule.instruments[inst_num].volpts,
                        this.UniModule.instruments[inst_num].volflg,
                        this.UniModule.instruments[inst_num].volbeg,
                        this.UniModule.instruments[inst_num].volend,
                        this.UniModule.instruments[inst_num].volsus);*/
                        /* scale volume envelope: */

                        for (p = 0; p < 12; p++)
                        {
                            this._module.Instruments[inst_num].VolEnv[p].Val <<= 2;
                            /*				printf("%d,%d,",this.UniModule.instruments[inst_num].volenv[p].pos,this.UniModule.instruments[inst_num].volenv[p].val); */
                        }

                        //memcpy(this.UniModule.instruments[inst_num].panenv,pth.panenv,24);
                        for (i = 0; i < 6; i++)
                        {
                            this._module.Instruments[inst_num].PanEnv[i].Pos = (short)(pth.panenv[i * 4] + (pth.panenv[i * 4 + 1] << 8));
                            this._module.Instruments[inst_num].PanEnv[i].Val = (short)(pth.panenv[i * 4 + 2] + (pth.panenv[i * 4 + 3] << 8));
                        }


                        /*
                        for (i = 0; i < 12; i++)
                        {
                        short tmp = ((byte*)this.UniModule.instruments[inst_num].panenv)[i*2];
						
                        ((byte*)this.UniModule.instruments[inst_num].panenv)[i*2] = ((byte*)this.UniModule.instruments[inst_num].panenv)[i*2+1];
                        ((byte*)this.UniModule.instruments[inst_num].panenv)[i*2+1] = tmp;
                        }*/
                        this._module.Instruments[inst_num].PanFlg = pth.panflg;
                        this._module.Instruments[inst_num].PanSus = pth.pansus;
                        this._module.Instruments[inst_num].PanBeg = pth.panbeg;
                        this._module.Instruments[inst_num].PanEnd = pth.panend;
                        this._module.Instruments[inst_num].PanPts = pth.panpts;

                        /*					  printf("Panning points	: %d\n"
                        "panflg			: %d\n"
                        "panbeg			: %d\n"
                        "panend			: %d\n"
                        "pansus			: %d\n",
                        this.UniModule.instruments[inst_num].panpts,
                        this.UniModule.instruments[inst_num].panflg,
                        this.UniModule.instruments[inst_num].panbeg,
                        this.UniModule.instruments[inst_num].panend,
                        this.UniModule.instruments[inst_num].pansus);*/
                        /* scale panning envelope: */

                        for (p = 0; p < 12; p++)
                        {
                            this._module.Instruments[inst_num].PanEnv[p].Val <<= 2;
                            /*				printf("%d,%d,",this.UniModule.instruments[inst_num].panenv[p].pos,this.UniModule.instruments[inst_num].panenv[p].val); */
                        }

                        /*                      for(u=0;u<256;u++){ */
                        /*                              printf("%2.2x ",fgetc(this.ModStream)); */
                        /*                      } */

                        next = 0;

                        for (u = 0; u < ih.numsmp; u++)
                        {
                            //q=&this.UniModule.instruments[inst_num].samples[u];

                            wh.length = Reader.ReadIntelULong();//MmIO.Instance._mm_read_I_ULONG(this.ModStream);
                            wh.loopstart = Reader.ReadIntelULong();//MmIO.Instance._mm_read_I_ULONG(this.ModStream);
                            wh.looplength = Reader.ReadIntelULong();//MmIO.Instance._mm_read_I_ULONG(this.ModStream);
                            wh.volume = Reader.ReadUByte();//MmIO.Instance.ReadUByte(this.ModStream);
                            wh.finetune = Reader.ReadSByte();//MmIO.Instance.ReadSByte(this.ModStream);
                            wh.type = Reader.ReadUByte();//MmIO.Instance.ReadUByte(this.ModStream);
                            wh.panning = Reader.ReadUByte();//MmIO.Instance.ReadUByte(this.ModStream);
                            wh.relnote = Reader.ReadSByte();//MmIO.Instance.ReadSByte(this.ModStream);
                            wh.reserved = (sbyte)Reader.ReadUByte();//(sbyte)MmIO.Instance.ReadUByte(this.ModStream);
                            wh.samplename = Reader.ReadString(22);//MmIO.Instance.ReadStringSBytes(wh.samplename, 22, this.ModStream);

                            /*printf("wav %d:%22.22s\n",u,wh.samplename);*/
                                                        
                            //this.UniModule.instruments[t].samples[u].samplename = new String(wh.samplename, 0, 0, 22);
                            if (u == this._module.Instruments[t].Samples.Count)
                                this._module.Instruments[t].Samples.Add(new Sample());
                            this._module.Instruments[t].Samples[u].SampleName = wh.samplename;//System.Text.UTF8Encoding.UTF8.GetString((byte[])(Array)wh.samplename, 0, wh.samplename.Length).Trim();// SupportClass.SbyteToString(wh.samplename);
                            this._module.Instruments[t].Samples[u].Length = wh.length;
                            this._module.Instruments[t].Samples[u].LoopStart = wh.loopstart;
                            this._module.Instruments[t].Samples[u].LoopEnd = wh.loopstart + wh.looplength;
                            this._module.Instruments[t].Samples[u].Volume = wh.volume;
                            this._module.Instruments[t].Samples[u].C2Spd = wh.finetune + 128;
                            this._module.Instruments[t].Samples[u].Transpose = wh.relnote;
                            this._module.Instruments[t].Samples[u].Panning = wh.panning;
                            this._module.Instruments[t].Samples[u].SeekPos = next;
                            this._module.Instruments[t].Samples[u].SampleRate = 22050; // Seems to be good...

                            if ((wh.type & 0x10) != 0)
                            {
                                this._module.Instruments[t].Samples[u].Length >>= 1;
                                this._module.Instruments[t].Samples[u].LoopStart >>= 1;
                                this._module.Instruments[t].Samples[u].LoopEnd >>= 1;
                            }

                            next += wh.length;

                            /*                              printf("Type %u\n",wh.type); */
                            /*				printf("Trans %d\n",wh.relnote); */

                            this._module.Instruments[t].Samples[u].Flags |= (SampleFormatFlags.SF_OWNPAN);
                            if ((wh.type & 0x3) != 0)
                                this._module.Instruments[t].Samples[u].Flags |= (SampleFormatFlags.SF_LOOP);
                            if ((wh.type & 0x2) != 0)
                                this._module.Instruments[t].Samples[u].Flags |= (SampleFormatFlags.SF_BIDI);

                            if ((wh.type & 0x10) != 0)
                                this._module.Instruments[t].Samples[u].Flags |= (SampleFormatFlags.SF_16BITS);

                            this._module.Instruments[t].Samples[u].Flags |= (SampleFormatFlags.SF_DELTA);
                            this._module.Instruments[t].Samples[u].Flags |= (SampleFormatFlags.SF_SIGNED);
                        }

                        for (u = 0; u < ih.numsmp; u++)
                            this._module.Instruments[inst_num].Samples[u].SeekPos += Reader.Tell();//MmIO.Instance.Tell(this.ModStream);

                        //MmIO.Instance.Seek(this.ModStream, next, SeekEnum.SEEK_CUR);
                        Reader.Seek(next, SeekOrigin.Current);
                    }

                    //d++;
                    inst_num++;
                }


                return true;
            }
            catch (System.IO.IOException)
            {
                return false;
            }
        }


    }

    #region Internal structs
    public class XMHEADER
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

        public XMHEADER()
        {
            //id = new sbyte[17];
            //songname = new sbyte[21];
            //trackername = new sbyte[20];
            orders = new short[256];
        }
    }

    public class XMNOTE
    {
        internal short note, ins, vol, eff, dat;
    }

    class XMINSTHEADER
    {
        internal int size; /* (dword) Instrument size */
        internal string name; /* (char) Instrument name */
        internal short type; /* (byte) Instrument type (always 0) */
        internal short numsmp; /* (word) Number of samples in instrument */
        internal int ssize;
        /* */

        public XMINSTHEADER()
        {
            //name = new sbyte[22];
        }
    }


    class XMPATCHHEADER
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
        internal short[] reserved;
        /* (word) Reserved */

        public XMPATCHHEADER()
        {
            what = new short[96];
            volenv = new short[48];
            panenv = new short[48];
            reserved = new short[11];
        }
    }


    class XMWAVHEADER
    {
        internal int length; /* (dword) Sample length */
        internal int loopstart; /* (dword) Sample loop start */
        internal int looplength; /* (dword) Sample loop length */
        internal short volume; /* (byte) Volume */
        internal sbyte finetune; /* (byte) Finetune (signed byte -128..+127) */
        internal short type; /* (byte) Type: Bit 0-1: 0 = No loop, 1 = Forward loop, */
        /*                                        2 = Ping-pong loop; */
        /*                                        4: 16-bit sampledata */
        internal short panning; /* (byte) Panning (0-255) */
        internal sbyte relnote; /* (byte) Relative note number (signed byte) */
        internal sbyte reserved; /* (byte) Reserved */
        internal string samplename;
        /* (char) Sample name */

        public XMWAVHEADER()
        {
            //samplename = new sbyte[22];
        }
    }


    class XMPATHEADER
    {
        internal int size; /* (dword) Pattern header length */
        internal short packing; /* (byte) Packing type (always 0) */
        internal short numrows; /* (word) Number of rows in pattern (1..256) */
        internal int packsize; /* (word) Packed patterndata size */
    }

    #endregion
}