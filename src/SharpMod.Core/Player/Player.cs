using System;
using SharpMod;
using SharpMod.UniTracker;
using SharpMod.Song;
using SharpMod.Mixer;

namespace SharpMod.Player;

internal delegate void UpdateUIHandler();
public delegate ActionsEnum GetUIEventHandler();
internal delegate void CurrentModEndHandler();

/// <summary>
/// The actual modplaying routines
/// </summary>
public partial class SharpModPlayer
{
    public event GetUIEventHandler OnGetUIActions;
    internal event UpdateUIHandler OnUpdateUI;
    internal event CurrentModEndHandler OnCurrentModEnd;

    public ChannelsMixer _mixer { get; set; }

    private readonly UniTrk _uniTrack;
    //private DriverContainer _driver;

    public float SpeedConstant { get; set; }
    public bool Quit { get; set; }
    public int PauseFlag { get; set; }
    public bool PlayCurrent { get; set; }
    public ActionsEnum UIResult { get; set; }

    /// <summary>
    /// this modfile is being played
    /// </summary>
    public SongModule CurrentUniMod { get; set; }

    /// <summary>
    ///  patternloop position
    /// </summary>
    public short PatternLoopPosition { get; set; }

    /// <summary>
    /// times to loop 
    /// </summary>
    public short RepeatCounter { get; set; }

    /// <summary>
    /// Tick Counter
    /// </summary>
    public short TickCounter { get; set; }

    /// <summary>
    /// position where to start a new pattern
    /// </summary>
    public short PatternBreakPosition { get; set; }

    /// <summary>
    /// Pattern Delay Counter
    /// </summary>
    public short PatternDelayCounter { get; set; }

    /// <summary>
    /// Pattern Delay Counter 2
    /// </summary>
    public short SecondPatternDelayCounter { get; set; }

    /// <summary>
    /// number of rows on current pattern
    /// </summary>
    public int numrow { get; set; }

    /// <summary>
    /// flag to indicate a position jump is needed...
    /// changed since 1.00: now also indicates the
    /// direction the position has to jump to:
    ///
    /// 0: Don't do anything
    /// 1: Jump back 1 position
    /// 2: Restart on current position
    /// 3: Jump forward 1 position
    /// </summary>
    public short posjmp { get; set; }

    /// <summary>
    /// forbidflag
    /// Set forbid to 1 when you want to modify any of the mp_sngpos, mp_patpos etc.
    /// variables and clear it when you're done. This prevents getting strange
    /// results due to intermediate interrupts.       
    /// </summary>
    public bool forbid { get; set; }

    protected internal int isfirst { get; set; }

    public ChannelMemory[] mp_audio { get; set; }//[32];    /* max 32 channels */

    /// <summary>
    /// beats-per-minute speed
    /// </summary>
    internal short mp_bpm { get; set; }

    /// <summary>
    /// current row number (0-255)
    /// </summary>
    private short _mp_patpos;
    public short mp_patpos
    {
        get { return _mp_patpos; }
        set
        {
            _mp_patpos = value;
            OnUpdateUI?.Invoke();
        }
    }

    /// <summary>
    /// current song position
    /// </summary>
    public short mp_sngpos { get; set; }

    /// <summary>
    /// current songspeed
    /// </summary>
    public short mp_sngspd { get; set; }

    /// <summary>
    /// channel it's working on 
    /// </summary>
    public short mp_channel { get; set; }

    /// <summary>
    ///  extended speed flag, default enabled
    /// </summary>
    public bool mp_extspd { get; set; }

    /// <summary>
    /// panning flag, default enabled
    /// </summary>
    public bool mp_panning { get; set; }

    /// <summary>
    /// loop module ?
    /// </summary>
    public bool mp_loop { get; set; }

    /// <summary>
    /// song volume (0-100) (or user volume)
    /// </summary>
    public short mp_volume { get; set; }

    /// <summary>
    ///  global volume
    /// </summary>
    protected internal sbyte globalvolume { get; set; }
    protected internal short globalslide { get; set; }

    /// <summary>
    /// current ChannelMemory it's working on
    /// </summary>
    public ChannelMemory a { get; set; }

    public float old_bpm { get; set; }

    internal static short[] toshortarray(int[] intarray)
    {
        short[] shortarray = new short[intarray.Length];
        int i;
        for (i = 0; i < intarray.Length; i++)
            shortarray[i] = (short)intarray[i];
        return shortarray;
    }

    protected internal static short[] mytab = [(short)(1712 * 16), (short)(1616 * 16), (short)(1524 * 16), (short)(1440 * 16), (short)(1356 * 16), (short)(1280 * 16), (short)(1208 * 16), (short)(1140 * 16), (short)(1076 * 16), (short)(1016 * 16), (short)(960 * 16), (short)(907 * 16)];
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="uniTrack"></param>
    /// <param name="driver"></param>
    public SharpModPlayer(UniTrk uniTrack/*, DriverContainer driver*/)
    {
        _uniTrack = uniTrack;
        //_driver = driver;

        mp_extspd = true;
        mp_panning = true;
        mp_loop = false;
        mp_volume = 100;
        isfirst = 0;
        globalvolume = 64;
        globalslide = 0;

        mp_audio = new ChannelMemory[32];

        for (int i = 0; i < 32; i++)
            mp_audio[i] = new ChannelMemory();


        //memset(mp_audio, 0, sizeof(mp_audio));
        for (int i = 0; i < 32; i++)
        {
            mp_audio[i].FadeVol = mp_audio[i].Start = mp_audio[i].Period = mp_audio[i].C2spd = mp_audio[i].TmpPeriod = mp_audio[i].WantedPeriod = mp_audio[i].SlideSpeed = mp_audio[i].PortSpeed = mp_audio[i].SampleOffset = 0;

            mp_audio[i].Volume = (sbyte)(mp_audio[i].Transpose = (sbyte)(mp_audio[i].Retrig = (sbyte)(mp_audio[i].TmpVolume = (sbyte)(mp_audio[i].VibPos = (sbyte)(mp_audio[i].TrmPos = ((sbyte)0))))));

            mp_audio[i].SampleNumber = (short)(mp_audio[i].Handle = (short)(mp_audio[i].Panning = (short)(mp_audio[i].PanSlideSpd = (short)(mp_audio[i].Note = (short)(mp_audio[i].OwnPer = (short)(mp_audio[i].OwnVol = (short)(mp_audio[i].S3mTremor = (short)(mp_audio[i].S3mTrOnOff = (short)(mp_audio[i].S3mVolSlide = (short)(mp_audio[i].S3mRtgSpeed = (short)(mp_audio[i].S3mRtgSlide = (short)(mp_audio[i].Glissando = (short)(mp_audio[i].WaveControl = (short)(mp_audio[i].VibSpd = (short)(mp_audio[i].VibDepth = (short)(mp_audio[i].TrmSpd = (short)(mp_audio[i].TrmDepth = ((short)0))))))))))))))))));

            mp_audio[i].KeyOn = mp_audio[i].Kick = false;

            mp_audio[i].Instrument = null;
            mp_audio[i].Sample = null;
            mp_audio[i].Row = null;

            mp_audio[i].VolEnv.Flg = (short)(mp_audio[i].VolEnv.Pts = (short)(mp_audio[i].VolEnv.Sus = (short)(mp_audio[i].VolEnv.Beg = (short)(mp_audio[i].VolEnv.End = (short)(mp_audio[i].VolEnv.CurrentCounter = (short)(mp_audio[i].VolEnv.EnvIdxA = mp_audio[i].VolEnv.EnvIdxB))))));
            mp_audio[i].VolEnv.EnvPoints = null;

            mp_audio[i].PanEnv.Flg = (short)(mp_audio[i].PanEnv.Pts = (short)(mp_audio[i].PanEnv.Sus = (short)(mp_audio[i].PanEnv.Beg = (short)(mp_audio[i].PanEnv.End = (short)(mp_audio[i].PanEnv.CurrentCounter = (short)(mp_audio[i].PanEnv.EnvIdxA = mp_audio[i].PanEnv.EnvIdxB))))));
            mp_audio[i].PanEnv.EnvPoints = null;
        }

    }

    public static short Interpolate(short p, short p1, short p2, short v1, short v2)
    {
        short dp, dv, di;

        if (p1 == p2)
            return v1;

        dv = (short)(v2 - v1);
        dp = (short)(p2 - p1);
        di = (short)(p - p1);

        return (short)(v1 + ((int)(di * dv) / dp));
    }


    public virtual void DoVibrato()
    {
        short q;
        int temp = 0;

        q = (short)((a.VibPos >> 2) & 0x1f);

        switch (a.WaveControl & 3)
        {
            case 0:
                temp = VibratoTable[q];
                break;

            case 1:
                q <<= 3;
                if (a.VibPos < 0)
                    q = (short)(255 - q);
                temp = q;
                break;

            case 2:
                temp = 255;
                break;
        }

        temp *= a.VibDepth;
        temp >>= 7;
        temp <<= 2;

        if (a.VibPos >= 0)
            a.Period = a.TmpPeriod + temp;
        else
            a.Period = a.TmpPeriod - temp;

        /* do not update when vbtick==0 */
        if (TickCounter != 0)
            a.VibPos = (sbyte)(a.VibPos + a.VibSpd);

    }


    public virtual void DoTremolo()
    {
        short q;
        int temp = 0;

        q = (short)((a.TrmPos >> 2) & 0x1f);

        switch ((a.WaveControl >> 4) & 3)
        {
            case 0:
                temp = VibratoTable[q];
                break;

            case 1:
                q <<= 3;
                if (a.TrmPos < 0)
                    q = (short)(255 - q);
                temp = q;
                break;

            case 2:
                temp = 255;
                break;
        }

        temp *= a.TrmDepth;
        temp >>= 6;

        if (a.TrmPos >= 0)
        {
            a.Volume = (sbyte)(a.TmpVolume + temp);
            if (a.Volume > 64)
                a.Volume = 64;
        }
        else
        {
            a.Volume = (sbyte)(a.TmpVolume - temp);
            if (a.Volume < 0)
                a.Volume = 0;
        }

        /* do not update when vbtick==0 */
        if (TickCounter != 0)
            a.TrmPos = (sbyte)(a.TrmPos + a.TrmSpd);

    }
            
    public virtual void DoToneSlide()
    {
        int dist;

        if (TickCounter == 0)
        {
            a.TmpPeriod = a.Period;
            return;
        }

        // We have to slide a.period towards a.wantedperiod, so
        //compute the difference between those two values
        dist = a.Period - a.WantedPeriod;

        // or if portamentospeed is too big 
        if (dist == 0 || a.PortSpeed > System.Math.Abs(dist))
        {
            // make tmpperiod equal tperiod 
            a.Period = a.WantedPeriod;
        }
        // dist>0 ? 
        else if (dist > 0)
        {
            // then slide up 
            a.Period -= a.PortSpeed;
        }
        else
        {
            // dist<0 . slide down 
            a.Period += a.PortSpeed;
        }

        /*      if(a.glissando){
			
        If glissando is on, find the nearest
        halfnote to a.tmpperiod
			
        for(t=0;t<60;t++){
        if(a.tmpperiod>=npertab[a.finetune][t]) break;
        }
			
        a.period=npertab[a.finetune][t];
        }
        else*/
        a.TmpPeriod = a.Period;
    }

   

    /// <summary>
    /// Volume Slide (MOD/XM: Axy, S3M/IT: Dxy)
    /// This command will slide up or down the current volume:
    /// A0x will decrease the current volume by x on every tick.
    /// Ax0 will increase the current volume by x on every tick.
    /// Total slide amount is x * (current_speed-1)
    /// Special note for S3M/IT:
    /// AFx will do a fine volume down by x.
    /// AxF will do a fine volume up by x.
    /// For fine volume slides, the total slide amount is x (The current speed doesn't matter).
    /// </summary>
    /// <param name="dat"></param>
    public virtual void DoVolSlide(short dat) // DoPTEffect9
    {
        dat &= 0xFF;

        // do not update when vbtick==0 
        if (TickCounter == 0)
            return;

        // volume slide
        a.TmpVolume = (sbyte)(a.TmpVolume + dat >> 4);
        a.TmpVolume = (sbyte)(a.TmpVolume - dat & 0xf);
        if (a.TmpVolume < 0)
            a.TmpVolume = 0;
        if (a.TmpVolume > 64)
            a.TmpVolume = 64;
    }

    

    /// <summary>
    /// Extended MOD Commands (MOD/XM: Exy, S3M/IT:undefined)
    /// Most of these can be mapped to on of the Sxy: Extended S3M Commands:
    /// E0x Filter On/Off : On the Amiga, this would set the enable (E00) or disable (E01) the analog 7 KHz low-pass filter on all channels. It has no effect in SharpMod.
    /// E1x: Fine (pitch) Slide Up
    /// E2x: Fine (pitch) Slide Down
    /// E3x: Glissando Control
    /// E4x: Vibrato Control
    /// E5x: Set Finetune
    /// E6x: Patternloop
    /// E7x: Tremolo Control
    /// E8x: Panning Control
    /// E9x: Retrig Note
    /// EAx: Fine Volume Slide Up
    /// EBx: Fine Volume Slide Down
    /// ECx: NoteCut
    /// EDx: NoteDelay
    /// EEx: PatternDelay
    /// EFx: Invert Loop (unsupported)
    /// See also: Pro Tracker Effect Commands. Original Amiga chipset - Audio features.
    /// </summary>
    /// <param name="dat"></param>
    public virtual void DoEEffects(short dat) //DoPTEffectE
    {
        short nib;

        dat &= 0xFF;

        nib = (short)(dat & 0xf);

        switch (dat >> 4)
        {
            //hardware filter toggle, not supported 
            case (short)(0x0):
                break;

            //fineslide up
            case (short)(0x1):
                if (TickCounter == 0)
                    a.TmpPeriod -= (nib << 2);
                break;

            //fineslide dn
            case (short)(0x2):
                if (TickCounter == 0)
                    a.TmpPeriod += (nib << 2);
                break;

            //glissando ctrl
            case (short)(0x3):
                a.Glissando = nib;
                break;

            //set vibrato waveform
            case (short)(0x4):
                a.WaveControl &= 0xf0;
                a.WaveControl |= nib;
                break;

            //set finetune
            case (short)(0x5):
                break;

            //set patternloop
            case (short)(0x6):

                if (TickCounter != 0)
                    break;

                //hmm.. this one is a real kludge. But now it works.
                if (nib != 0)
                {
                    // set reppos or repcnt ? 

                    // set repcnt, so check if repcnt already is set,
                    // which means we are already looping 
                    if (RepeatCounter > 0)
                        // already looping, decrease counter
                        RepeatCounter--;
                    else
                        // not yet looping, so set repcnt
                        RepeatCounter = nib;


                    if (RepeatCounter != 0)
                        // jump to reppos if repcnt>0 
                        mp_patpos = PatternLoopPosition;
                }
                else
                {
                    // set reppos 
                    PatternLoopPosition = (short)(mp_patpos - 1);
                }
                break;

            //set tremolo waveform
            case (short)(0x7):
                a.WaveControl &= 0x0f;
                a.WaveControl |= (short)(nib << 4);
                break;

            //set panning 
            case (short)(0x8):
                if (mp_panning)
                {
                    nib <<= 4;
                    a.Panning = nib;
                    CurrentUniMod.Panning[mp_channel] = nib;
                }
                break;

            //retrig note
            case (short)(0x9):

                if (nib > 0)
                {
                    if (a.Retrig == 0)
                    {

                        // when retrig counter reaches 0,
                        // reset counter and restart the sample
                        a.Kick = true;
                        a.Retrig = (sbyte)nib;
                    }
                    // countdown
                    a.Retrig--;
                }
                break;

            //fine volume slide up
            case (short)(0xa):
                if (TickCounter != 0)
                    break;

                a.TmpVolume = (sbyte)(a.TmpVolume + nib);
                if (a.TmpVolume > 64)
                    a.TmpVolume = 64;
                break;

            //fine volume slide dn
            case (short)(0xb):
                if (TickCounter != 0)
                    break;

                a.TmpVolume = (sbyte)(a.TmpVolume - nib);
                if (a.TmpVolume < 0)
                    a.TmpVolume = 0;
                break;

            // cut note
            case (short)(0xc):

                if (TickCounter >= nib)
                {
                    // just turn the volume down
                    a.TmpVolume = 0;
                }
                break;

            //note delay
            case (short)(0xd):

                if (TickCounter == nib)
                {
                    a.Kick = true;
                }
                else
                    a.Kick = false;
                break;

            //pattern delay
            case (short)(0xe):
                if (TickCounter != 0)
                    break;

                // only once (when vbtick=0)
                if (SecondPatternDelayCounter == 0)
                    PatternDelayCounter = (short)(nib + 1);
                break;

            //invert loop, not supported
            case (short)(0xf):
                break;
        }
    }

    

   


    public virtual void PlayNote()
    {
        int period;
        Effects c;
        short inst;
        short note;

        if (a.Row == null)
            return;

        _uniTrack.UniSetRow(a.Row, a.RowPos);

        while ((c = (Effects)_uniTrack.UniGetByte()) != 0)
        {
            switch (c)
            {
                case Effects.UNI_NOTE:
                    note = _uniTrack.UniGetByte();

                    if (note == 96)
                    {
                        /* key off ? */
                        a.KeyOn = false;
                        if ((a.Instrument != null) && ((a.Instrument.VolFlg & (short)EnvelopeFlags.EF_ON) == 0))
                        {
                            a.TmpVolume = 0;
                        }
                    }
                    else
                    {
                        a.Note = note;

                        period = GetPeriod((short)(note + a.Transpose), a.C2spd);

                        a.WantedPeriod = period;
                        a.TmpPeriod = period;

                        a.Kick = true;
                        a.Start = 0;

                        /* retrig tremolo and vibrato waves ? */
                        if ((a.WaveControl & 0x80) == 0)
                            a.TrmPos = 0;
                        if ((a.WaveControl & 0x08) == 0)
                            a.VibPos = 0;
                    }
                    break;

                case Effects.UNI_INSTRUMENT:
                    inst = _uniTrack.UniGetByte();
                    if (inst >= CurrentUniMod.Instruments.Count)
                        break; /* <- safety valve */

                    a.SampleNumber = inst;

                    // i=&pf.instruments[inst];
                    a.Instrument = CurrentUniMod.Instruments[inst];

                    if (CurrentUniMod.Instruments[inst].SampleNumber[a.Note] >= CurrentUniMod.Instruments[inst].NumSmp)
                        break;

                    //s=&i.samples[i.samplenumber[a.note]];
                    a.Sample = CurrentUniMod.Instruments[inst].Samples[CurrentUniMod.Instruments[inst].SampleNumber[a.Note]];


                    /* channel or instrument determined panning ? */
                    if ((CurrentUniMod.Instruments[inst].Samples[CurrentUniMod.Instruments[inst].SampleNumber[a.Note]].Flags & (SampleFormatFlags.SF_OWNPAN)) != 0)
                    {
                        a.Panning = CurrentUniMod.Instruments[inst].Samples[CurrentUniMod.Instruments[inst].SampleNumber[a.Note]].Panning;
                    }
                    else
                    {
                        a.Panning = CurrentUniMod.Panning[mp_channel];
                    }

                    a.Transpose = CurrentUniMod.Instruments[inst].Samples[CurrentUniMod.Instruments[inst].SampleNumber[a.Note]].Transpose;
                    a.Handle = CurrentUniMod.Instruments[inst].Samples[CurrentUniMod.Instruments[inst].SampleNumber[a.Note]].Handle;
                    a.TmpVolume = (sbyte)(CurrentUniMod.Instruments[inst].Samples[CurrentUniMod.Instruments[inst].SampleNumber[a.Note]].Volume);
                    a.Volume = (sbyte)(CurrentUniMod.Instruments[inst].Samples[CurrentUniMod.Instruments[inst].SampleNumber[a.Note]].Volume);
                    a.C2spd = CurrentUniMod.Instruments[inst].Samples[CurrentUniMod.Instruments[inst].SampleNumber[a.Note]].C2Spd;
                    a.Retrig = 0;
                    a.S3mTremor = 0;

                    period = GetPeriod((short)(a.Note + a.Transpose), (short)a.C2spd);

                    a.WantedPeriod = period;
                    a.TmpPeriod = period;
                    break;

                default:
                    _uniTrack.UniSkipOpcode((short)c);
                    break;

            }
        }
    }




    public virtual void PlayEffects()
    {
        Effects c;
        //short dat;

        if (a.Row == null)
            return;

        _uniTrack.UniSetRow(a.Row, a.RowPos);

        a.OwnPer = 0;
        a.OwnVol = 0;

        while ((c = (Effects)_uniTrack.UniGetByte()) != 0)
        {
            switch (c)
            {
                case Effects.UNI_NOTE:
                case Effects.UNI_INSTRUMENT:
                    _uniTrack.UniSkipOpcode((short)c);
                    break;

                case Effects.UNI_PTEFFECT0:
                    DoPTEffect0(_uniTrack.UniGetByte());
                    break;

                case Effects.UNI_PTEFFECT1:
                    DoPTEffect1(_uniTrack.UniGetByte());
                    break;

                case Effects.UNI_PTEFFECT2:
                    DoPTEffect2(_uniTrack.UniGetByte());
                    break;

                case Effects.UNI_PTEFFECT3:
                    DoPTEffect3(_uniTrack.UniGetByte());
                    break;

                case Effects.UNI_PTEFFECT4:
                    DoPTEffect4(_uniTrack.UniGetByte());
                    break;

                case Effects.UNI_PTEFFECT5:
                    DoPTEffect5(_uniTrack.UniGetByte());
                    break;

                case Effects.UNI_PTEFFECT6:
                    DoPTEffect6(_uniTrack.UniGetByte());
                    break;

                case Effects.UNI_PTEFFECT7:
                    DoPTEffect7(_uniTrack.UniGetByte());
                    break;

                case Effects.UNI_PTEFFECT8:
                    DoPTEffect8(_uniTrack.UniGetByte());
                    break;

                case Effects.UNI_PTEFFECT9:
                    DoPTEffect9(_uniTrack.UniGetByte());
                    break;

                case Effects.UNI_PTEFFECTA:
                    DoVolSlide(_uniTrack.UniGetByte());
                    break;

                case Effects.UNI_PTEFFECTB:
                    DoPTEffectB(_uniTrack.UniGetByte());
                    break;

                case Effects.UNI_PTEFFECTC:
                    DoPTEffectC(_uniTrack.UniGetByte());
                    break;

                case Effects.UNI_PTEFFECTD:
                    DoPTEffectD(_uniTrack.UniGetByte());
                    break;

                case Effects.UNI_PTEFFECTE:
                    DoEEffects(_uniTrack.UniGetByte());
                    break;

                case Effects.UNI_PTEFFECTF:
                    DoPTEffectF(_uniTrack.UniGetByte());
                    break;

                case Effects.UNI_S3MEFFECTD:
                    DoS3MVolSlide(_uniTrack.UniGetByte());
                    break;

                case Effects.UNI_S3MEFFECTE:
                    DoS3MSlideDn(_uniTrack.UniGetByte());
                    break;

                case Effects.UNI_S3MEFFECTF:
                    DoS3MSlideUp(_uniTrack.UniGetByte());
                    break;

                case Effects.UNI_S3MEFFECTI:
                    DoS3MTremor(_uniTrack.UniGetByte());
                    a.OwnVol = 1;
                    break;

                case Effects.UNI_S3MEFFECTQ:
                    DoS3MRetrig(_uniTrack.UniGetByte());
                    break;

                case Effects.UNI_S3MEFFECTA:
                    DoS3MSpeed(_uniTrack.UniGetByte());
                    break;

                case Effects.UNI_S3MEFFECTT:
                    DoS3MTempo(_uniTrack.UniGetByte());
                    break;

                case Effects.UNI_XMEFFECTA:
                    DoXMVolSlide(_uniTrack.UniGetByte());
                    break;

                case Effects.UNI_XMEFFECTG:
                    globalvolume = (sbyte)_uniTrack.UniGetByte();
                    break;

                case Effects.UNI_XMEFFECTH:
                    DoXMGlobalSlide(_uniTrack.UniGetByte());
                    break;

                case Effects.UNI_XMEFFECTP:
                    DoXMPanSlide(_uniTrack.UniGetByte());
                    break;

                default:
                    _uniTrack.UniSkipOpcode((short)c);
                    break;

            }
        }

        if (a.OwnPer == 0)
        {
            a.Period = a.TmpPeriod;
        }

        if (a.OwnVol == 0)
        {
            a.Volume = a.TmpVolume;
        }
    }


    public static short DoPan(short envpan, short pan)
    {
        return (short)(pan + (((envpan - 128) * (128 - System.Math.Abs(pan - 128))) / 128));
    }

    public static short DoVol(int a, short b, short c)
    {
        a *= b;
        a *= c;

        return (short)(a >> 23);
    }

    public static double rint(double x)
    {
        return Math.Round(x);
    }
}