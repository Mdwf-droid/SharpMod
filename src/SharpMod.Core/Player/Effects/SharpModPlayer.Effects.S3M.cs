using System;
using System.Collections.Generic;
using System.Text;

namespace SharpMod.Player;

public partial class SharpModPlayer
{
    /// <summary>
    /// Set Speed (MOD/XM: undefined, S3M/IT:Axx)
    /// This command will set the speed of the current song (Hex). 
    /// Avoid using values bigger than 20, for better MOD/XM compatibility.
    /// </summary>
    /// <param name="speed"></param>
    public virtual void DoS3MSpeed(short speed)
    {
        speed &= 0xFF;

        if ((TickCounter != 0) || (SecondPatternDelayCounter != 0))
            return;

        if (speed != 0)
        {
            mp_sngspd = speed;
            TickCounter = 0;
        }
    }

    /// <summary>
    /// Set Tempo (MOD/XM: undefined, S3M/IT:Txx)
    /// This command will change the tempo of the song (Hex). 
    /// The minimum value is T20, and the maximum possible value is TFF. 
    /// The default tempo is 125 (T7D), which is equivalent to one tick every 20ms (50Hz)
    /// Note: T0x will decrease the current tempo by x. T1x will increase the current tempo by x.
    /// </summary>
    /// <param name="tempo"></param>
    public virtual void DoS3MTempo(short tempo)
    {
        tempo &= 0xFF;

        if ((TickCounter != 0) || (SecondPatternDelayCounter != 0))
            return;
        old_bpm = tempo;

        mp_bpm = (short)rint(old_bpm * SpeedConstant);
    }

    /// <summary>
    /// Tremor (MOD: undefined, XM: Txy, S3M/IT:Ixy)
    /// This effect will turn on and off the current channel every frame: T[ontime][offtime].
    /// x=ontime, y=offtime: the volume will stay unchanged for x frames, and then muted for y frames.
    /// Note: The exact duration of the ontime/offtime is different for MOD, XM and S3M/IT.
    /// </summary>
    /// <param name="inf"></param>
    public virtual void DoS3MTremor(short inf)
    {
        short on, off;

        inf &= 0xFF;

        if (inf != 0)
            a.S3mTrOnOff = inf;
        else
            inf = a.S3mTrOnOff;

        if (TickCounter == 0)
            return;

        on = (short)((inf >> 4) + 1);
        off = (short)((inf & 0xf) + 1);

        a.S3mTremor = (short)(a.S3mTremor % (on + off));
        a.Volume = (a.S3mTremor < on) ? a.TmpVolume : (sbyte)0;
        a.S3mTremor++;
    }

    /// <summary>
    /// Retrig Note(MOD:undefined, XM:Rxy, S3M/IT:Qxy)
    /// This command will retrig the same note before playing the next. 
    /// Where to retrig depends on the speed of the song. 
    /// If you retrig with 1 in speed 6 that note will be trigged 6 times in one note row. 
    /// Example:
    /// ... .. .. F06  (Set speed to 6)
    /// C-3 42 .. Q03  (Retrig at tick 3 out of 6)
    /// Retrig on hi-hats!
    /// </summary>
    /// <param name="inf"></param>
    public virtual void DoS3MRetrig(short inf)
    {
        short hi, lo;

        inf &= 0xFF;

        hi = (short)(inf >> 4);
        lo = (short)(inf & 0xf);

        if (lo != 0)
        {
            a.S3mRtgSlide = hi;
            a.S3mRtgSpeed = lo;
        }

        if (hi != 0)
        {
            a.S3mRtgSlide = hi;
        }

        // only retrigger if lo nibble > 0
        if (a.S3mRtgSpeed > 0)
        {
            if (a.Retrig == 0)
            {
                // when retrig counter reaches 0,
                // reset counter and restart the sample
                a.Kick = true;
                a.Retrig = (sbyte)a.S3mRtgSpeed;

                // don't slide on first retrig 
                if (TickCounter != 0)
                {
                    switch (a.S3mRtgSlide)
                    {
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                            a.TmpVolume = (sbyte)(a.TmpVolume - (1 << (a.S3mRtgSlide - 1)));
                            break;

                        case 6:
                            a.TmpVolume = (sbyte)((2 * a.TmpVolume) / 3);
                            break;

                        case 7:
                            a.TmpVolume = (sbyte)(a.TmpVolume >> 1);
                            break;

                        case 9:
                        case (short)(0xa):
                        case (short)(0xb):
                        case (short)(0xc):
                        case (short)(0xd):
                            a.TmpVolume = (sbyte)(a.TmpVolume + (1 << (a.S3mRtgSlide - 9)));
                            break;

                        case (short)(0xe):
                            a.TmpVolume = (sbyte)((3 * a.TmpVolume) / 2);
                            break;

                        case (short)(0xf):
                            a.TmpVolume = (sbyte)(a.TmpVolume << 1);
                            break;
                    }
                    if (a.TmpVolume < 0)
                        a.TmpVolume = 0;
                    if (a.TmpVolume > 64)
                        a.TmpVolume = 64;
                }
            }

            // countdown
            a.Retrig--;
        }
    }

    public virtual void DoS3MVolSlide(short inf)
    {
        short lo, hi;

        inf &= 0xFF;

        if (inf != 0)
        {
            a.S3mVolSlide = inf;
        }
        inf = a.S3mVolSlide;

        lo = (short)(inf & 0xf);
        hi = (short)(inf >> 4);

        if (hi == 0)
        {
            a.TmpVolume = (sbyte)(a.TmpVolume - lo);
        }
        else if (lo == 0)
        {
            a.TmpVolume = (sbyte)(a.TmpVolume + hi);
        }
        else if (hi == 0xf)
        {
            if (TickCounter == 0)
                a.TmpVolume = (sbyte)(a.TmpVolume - lo);
        }
        else if (lo == 0xf)
        {
            if (TickCounter == 0)
                a.TmpVolume = (sbyte)(a.TmpVolume + hi);
        }

        if (a.TmpVolume < 0)
            a.TmpVolume = 0;
        if (a.TmpVolume > 64)
            a.TmpVolume = 64;
    }

    public virtual void DoS3MSlideDn(short inf)
    {
        short hi, lo;

        inf &= 0xFF;

        if (inf != 0)
            a.SlideSpeed = inf;
        else
            inf = (short)(a.SlideSpeed);

        hi = (short)(inf >> 4);
        lo = (short)(inf & 0xf);

        if (hi == 0xf)
        {
            if (TickCounter == 0)
                a.TmpPeriod += lo << 2;
        }
        else if (hi == 0xe)
        {
            if (TickCounter == 0)
                a.TmpPeriod += lo;
        }
        else
        {
            if (TickCounter != 0)
                a.TmpPeriod += inf << 2;
        }
    }

    public virtual void DoS3MSlideUp(short inf)
    {
        short hi, lo;

        inf &= 0xFF;

        if (inf != 0)
            a.SlideSpeed = inf;
        else
            inf = (short)(a.SlideSpeed);

        hi = (short)(inf >> 4);
        lo = (short)(inf & 0xf);

        if (hi == 0xf)
        {
            if (TickCounter == 0)
                a.TmpPeriod -= lo << 2;
        }
        else if (hi == 0xe)
        {
            if (TickCounter == 0)
                a.TmpPeriod -= lo;
        }
        else
        {
            if (TickCounter != 0)
                a.TmpPeriod -= inf << 2;
        }
    }
}
