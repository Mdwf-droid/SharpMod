using System;

namespace SharpMod.UniTracker
{
    /// <summary>
    /// All routines dealing with the manipulation of UNITRK(tm) streams
    /// 
    /// Ok.. I'll try to explain the new internal module format.. so here it goes:
    ///	
    ///	
    ///	The UNITRK(tm) Format:
    ///	======================
    ///	
    ///	A UNITRK stream is an array of bytes representing a single track
    ///	of a pattern. It's made up of 'repeat/length' bytes, opcodes and
    ///	operands (sort of a assembly language):
    ///	
    ///	rrrlllll
    ///	[REP/LEN][OPCODE][OPERAND][OPCODE][OPERAND] [REP/LEN][OPCODE][OPERAND]..
    ///	^                                         ^ ^
    ///	|-------ROWS 0 - 0+REP of a track---------| |-------ROWS xx - xx+REP of a track...
    ///	
    ///	
    ///	The rep/len byte contains the number of bytes in the current row,
    ///	_including_ the length byte itself (So the LENGTH byte of row 0 in the
    ///	previous example would have a value of 5). This makes it easy to search
    ///	through a stream for a particular row. A track is concluded by a 0-value
    ///	length byte.
    ///	
    ///	The upper 3 bits of the rep/len byte contain the number of times -1 this
    ///	row is repeated for this track. (so a value of 7 means this row is repeated
    ///	8 times)
    ///	
    ///	Opcodes can range from 1 to 255 but currently only opcodes 1 to 19 are
    ///	being used. Each opcode can have a different number of operands. You can
    ///	find the number of operands to a particular opcode by using the opcode
    ///	as an index into the 'unioperands' table.
    /// </summary>
    public class UniTrk
    {
        protected internal const int BUFPAGE = 128; /* smallest unibuffer size */
        protected internal const int TRESHOLD = 16;	/* unibuffer is increased by BUFPAGE bytes when unipc reaches unimax-TRESHOLD */
        protected internal static short[] UniOperands = new short[] { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };


        private short[] _rowStart; /* startadress of a row */
        private int _rowEnd; /* endaddress of a row (exclusive) */
        private int _rowPC; /* current UniMod(tm) programcounter */
        private short[] _uniBuf; /* pointer to the temporary unitrk buffer */
        private int _uniMax; /* maximum number of bytes to be written to this buffer */
        private int _uniPC; /* index in the buffer where next opcode will be written */
        private int _uniTT; /* holds index of the rep/len byte of a row */
        private int _lastP; /* holds index to the previous row (needed for compressing) */

        /// <summary>
        /// startadress of a row
        /// </summary>
        public short[] RowStart
        {
            get { return _rowStart; }
            set { _rowStart = value; }
        }

        /// <summary>
        /// endaddress of a row (exclusive)
        /// </summary>
        public int RowEnd
        {
            get { return _rowEnd; }
            set { _rowEnd = value; }
        }

        /// <summary>
        /// current UniMod(tm) programcounter
        /// </summary>
        public int RowPC
        {
            get { return _rowPC; }
            set { _rowPC = value; }
        }

        /// <summary>
        /// pointer to the temporary unitrk buffer
        /// </summary>
        public short[] UniBuf
        {
            get { return _uniBuf; }
            set { _uniBuf = value; }
        }

        /// <summary>
        /// maximum number of bytes to be written to this buffer
        /// </summary>
        public int UniMax
        {
            get { return _uniMax; }
            set { _uniMax = value; }
        }

        /// <summary>
        /// index in the buffer where next opcode will be written
        /// </summary>
        public int UniPC
        {
            get { return _uniPC; }
            set { _uniPC = value; }
        }

        /// <summary>
        /// holds index of the rep/len byte of a row
        /// </summary>
        public int UniTT
        {
            get { return _uniTT; }
            set { _uniTT = value; }
        }

        /// <summary>
        /// holds index to the previous row (needed for compressing)
        /// </summary>
        public int LastP
        {
            get { return _lastP; }
            set { _lastP = value; }
        }


        #region routines for reading a UNITRK stream:
        public UniTrk()
        {
            RowStart = null;
            RowEnd = 0;
            RowPC = 0;
            UniBuf = null;
            UniMax = 0;
            UniPC = 0;
            UniTT = 0;
            LastP = 0;
        }

        public virtual void UniSetRow(short[] t, int start_at)
        {
            RowStart = t;
            RowPC = start_at; //rowpc=rowstart;
            RowEnd = RowPC + (RowStart[RowPC++] & 0x1f); //rowend=rowstart+(*(rowpc++)&0x1f);
        }


        public virtual short UniGetByte()
        {
            //return (rowpc<rowend) ? *(rowpc++) : 0;
            return (short)((RowPC < RowEnd) ? RowStart[RowPC++] : (short)0);
        }


        public virtual void UniSkipOpcode(short op)
        {
            short t = UniOperands[op];
            while ((t--) != 0)
                UniGetByte();
        }

        /// <summary>
        /// Finds the address of row number 'row' in the UniMod(tm) stream 't'		
        /// </summary>
        /// <param name="t"></param>
        /// <param name="row"></param>
        /// <returns>returns NULL if the row can't be found.</returns>
        public virtual int UniFindRow(short[] t, int row)
        {
            short c, l;
            int tp = 0;

            while (true)
            {

                c = t[tp]; /* get rep/len byte */

                if (c == 0)
                    return -1;
                /* zero ? -> end of track.. */

                l = (short)((c >> 5) + 1); /* extract repeat value */

                if (l > row)
                    break;
                /* reached wanted row? -> return pointer */

                row -= l; /* havn't reached row yet.. update row */
                tp += c & 0x1f; /* point t to the next row */
            }

            return tp;
        }

        #endregion

        #region routines for CREATING UNITRK streams:

        /// <summary>
        /// Resets index-pointers to create a new track.
        /// </summary>
        public virtual void UniReset()       
        {
            UniTT = 0; /* reset index to rep/len byte */
            UniPC = 1; /* first opcode will be written to index 1 */
            LastP = 0; /* no previous row yet */
            UniBuf[0] = 0; /* clear rep/len byte */
        }

        public virtual void UniWrite(Effects data)
        {
            this.UniWrite((short)data);
        }

        /// <summary>
        ///  Appends one byte of data to the current row of a track.
        /// </summary>
        /// <param name="data"></param>
        public virtual void UniWrite(short data)
        {
            /* write byte to current position and update */

            data &= 0xFF;
            UniBuf[UniPC++] = data;

            /* Check if we've reached the end of the buffer */

            if (UniPC > (UniMax - TRESHOLD))
            {

                short[] newbuf;

                /* We've reached the end of the buffer, so expand
                the buffer by BUFPAGE bytes */

                // newbuf=(short *)realloc(unibuf,(unimax+BUFPAGE)*2);
                newbuf = new short[UniMax + BUFPAGE];

                /* Check if realloc succeeded */

                if (newbuf != null)
                {
                    int i;
                    for (i = 0; i < UniMax; i++)
                        newbuf[i] = UniBuf[i];
                    //delete [] unibuf;
                    UniBuf = null;

                    UniBuf = newbuf;
                    UniMax += BUFPAGE;
                }
                else
                {
                    /* realloc failed, so decrease unipc so we won't write beyond
                    the end of the buffer.. I don't report the out-of-memory
                    here; the UniDup() will fail anyway so that's where the
                    loader sees that something went wrong */

                    UniPC--;
                }
            }
        }

        /// <summary>
        /// Appends UNI_INSTRUMENT opcode to the unitrk stream.
        /// </summary>
        /// <param name="ins"></param>
        public virtual void UniInstrument(short ins)
        {
            UniWrite(Effects.UNI_INSTRUMENT);
            UniWrite(ins);
        }

        /// <summary>
        /// Appends UNI_NOTE opcode to the unitrk stream.
        /// </summary>
        /// <param name="note"></param>
        public virtual void UniNote(short note)
        {
            UniWrite(Effects.UNI_NOTE);
            UniWrite(note);
        }

        /// <summary>
        /// Appends UNI_PTEFFECTX opcode to the unitrk stream.
        /// </summary>
        /// <param name="eff"></param>
        /// <param name="dat"></param>
        public virtual void UniPTEffect(short eff, short dat)
        {
            eff &= 0xFF;
            dat &= 0xFF;

            if (eff != 0 || dat != 0)
            {
                /* don't write empty effect */
                UniWrite((short)(Effects.UNI_PTEFFECT0 + eff));
                UniWrite(dat);
            }
        }


        public virtual bool MyCmp(short[] a, int a_offset, short[] b, int b_offset, int l)
        {
            int t;

            for (t = 0; t < l; t++)
            {
                if (a[t + a_offset] != b[t + b_offset])
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Closes the current row of a unitrk stream (updates the rep/len byte)
        /// and sets pointers to start a new row.
        /// </summary>
        public virtual void UniNewline()
        {
            int n, l, len;

            n = (UniBuf[LastP] >> 5) + 1; /* repeat of previous row */
            l = (UniBuf[LastP] & 0x1f); /* length of previous row */

            len = UniPC - UniTT; /* length of current row */

            /* Now, check if the previous and the current row are identical..
            when they are, just increase the repeat field of the previous row */

            if (n < 8 && len == l && MyCmp(UniBuf, LastP + 1, UniBuf, UniTT + 1, (len - 1)))
            {
                UniBuf[LastP] = (short)(UniBuf[LastP] + (short)0x20);
                UniPC = UniTT + 1;
            }
            else
            {
                /* current and previous row aren't equal.. so just update the pointers */

                UniBuf[UniTT] = (short)len;
                LastP = UniTT;
                UniTT = UniPC;
                UniPC++;
            }
        }

        /// <summary>
        ///  Terminates the current unitrk stream and returns a pointer
        /// to a copy of the stream.
        /// </summary>
        /// <returns></returns>
        public virtual short[] UniDup()
        {
            int i;
            short[] d;

            UniBuf[UniTT] = 0;

            /*if((d=(short *)malloc(unipc*2))==NULL){
            m_->mmIO->myerr=m_->ERROR_ALLOC_STRUCT;
            return NULL;
            }*/
            d = new short[UniPC];

            //memcpy(d,unibuf,unipc*2);
            for (i = 0; i < UniPC; i++)
                d[i] = UniBuf[i];

            return d;
        }

        /// <summary>
        /// Determines the length (in rows) of a unitrk stream 't'
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public virtual int TrkLen(short[] t)
        {
            int len = 0;
            short c;
            int tp = 0;

            while ((c = (short)(t[tp] & 0x1f)) != 0)
            {
                len += c;
                tp += c;
            }
            len++;

            return len;
        }


        public virtual bool UniInit()
        {
            UniMax = BUFPAGE;

            /*if(!(unibuf=(short *)malloc(unimax*2))){
            m_->mmIO->myerr=m_->ERROR_ALLOC_STRUCT;
            return 0;
            }*/
            UniBuf = new short[UniMax];
            return true;
        }


        public virtual void UniCleanup()
        {
            UniBuf = null;
        }

#endregion
    }
}