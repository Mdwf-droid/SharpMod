using System;
using System.Collections.Generic;
using System.Text;

namespace SharpMod.Player;

public partial class SharpModPlayer
{
    /// <summary>
    /// Arpeggio
    /// Cycles between note, note+x halftones, note+y halftones. 
    /// Ex: S3M/IT: C-4 01 .. J37 (MOD/XM: C-4 01 .. J37) 
    /// This will play C-4, C-4+3 semitones and C-4+7 semitones (C-4, D#4 and G-4) 
    /// Note: if both x and y are zero, this command is ignored in MOD/XM. 
    /// In S3M/IT modules, J00 uses the previous value.
    /// </summary>
    /// <param name="dat"></param>
    public virtual void DoPTEffect0(short dat)
    {
        short note;

        dat &= 0xFF;
        note = a.Note;

        if (dat != 0)
        {
            switch (TickCounter % 3)
            {
                case 1:
                    note = (short)(note + (dat >> 4));
                    break;

                case 2:
                    note = (short)(note + (dat & 0xf));
                    break;
            }
            a.Period = GetPeriod((short)(note + a.Transpose), a.C2spd);
            a.OwnPer = 1;
        }
    }

    /// <summary>
    /// Portamento Up (MOD/XM: 1xy, S3M/IT: Fxy)
    /// This will slide up the pitch of the current note being played by the given speed. 
    /// In S3M/IT mode, FFx is a fine portamento up by x, and FEx is a extra-fine portamento up.
    /// </summary>
    /// <param name="dat"></param>
    public virtual void DoPTEffect1(short dat)
    {
        if (dat != 0)
            a.SlideSpeed = (int)dat << 2;
        if (TickCounter != 0)
            a.TmpPeriod -= a.SlideSpeed;

    }

    /// <summary>
    /// Portamento Down (MOD/XM: 2xy, S3M/IT: Exy)
    /// This will slide down the pitch of the current note being played by the given speed. 
    /// In S3M/IT mode, EFx is a fine portamento down by x, and EEx is a extra-fine portamento up.
    /// </summary>
    /// <param name="dat"></param>
    public virtual void DoPTEffect2(short dat)
    {
        if (dat != 0)
            a.SlideSpeed = (int)dat << 2;
        if (TickCounter != 0)
            a.TmpPeriod += a.SlideSpeed;
    }

    /// <summary>
    /// Tone-Portamento (MOD/XM: 3xy, S3M/IT: Gxy)
    /// This command is used together with a note, and will bend the current pitch at the given speed towards the specified note. 
    /// Example:
    /// C-4 01 .. ...
    /// F-4 .. .. G05 (bend the note up towards F-4)
    /// ... .. .. G00 (continue to slide up, until F-4 is reached)
    /// If the glissando command has been used before, the pitch will be rounded to the nearest halftone.
    /// </summary>
    /// <param name="dat"></param>
    public virtual void DoPTEffect3(short dat)
    {
        // temp XM fix
        a.Kick = false;

        if (dat != 0)
        {
            a.PortSpeed = dat;
            a.PortSpeed <<= 2;
        }
        DoToneSlide();
        a.OwnPer = 1;
    }

    /// <summary>
    /// Vibrato (MOD/XM: 4xy, S3M/IT: Hxy)
    /// Vibrato with speed x and depth y. 
    /// This command will oscillate the frequency of the current note with a sine wave. 
    /// (You can change the vibrato waveform to a triangle wave, a square wave, or a random table by using the E4x (MOD/XM) or S3x (S3M/IT) command)
    /// </summary>
    /// <param name="dat"></param>
    public virtual void DoPTEffect4(short dat)
    {
        if ((dat & 0x0f) != 0)
            a.VibDepth = (short)(dat & 0xf);
        if ((dat & 0xf0) != 0)
            a.VibSpd = (short)((dat & 0xf0) >> 2);
        DoVibrato();
        a.OwnPer = 1;
    }

    /// <summary>
    /// Tone-Portamento + Volume Slide (MOD/XM: 5xy, S3M/IT: Lxy)
    /// See also: Tone-Portamento, Volume Slide. 
    /// This command is equivalent to Tone-Portamento and Volume Slide. 
    /// (MOD/XM: 300 + Axy, S3M/IT: G00 + Dxy)
    /// </summary>
    /// <param name="dat"></param>
    public virtual void DoPTEffect5(short dat)
    {
        a.Kick = false;
        DoToneSlide();
        DoVolSlide(dat);
        a.OwnPer = 1;
    }

    /// <summary>
    /// Vibrato + Volume Slide (MOD/XM: 6xy, S3M/IT: Kxy)
    /// See also: Vibrato, Volume Slide. 
    /// This command is equivalent to Vibrato and Volume Slide. 
    /// (MOD/XM: 400 + Axy, S3M/IT: H00 + Dxy or U00 + Dxy)
    /// </summary>
    /// <param name="dat"></param>
    public virtual void DoPTEffect6(short dat)
    {
        DoVibrato();
        DoVolSlide(dat);
        a.OwnPer = 1;
    }

    /// <summary>
    /// Tremolo (MOD/XM: 7xy, S3M/IT: Rxy)
    /// Similar to the vibrato, but changes the volume instead of the pitch.
    /// </summary>
    /// <param name="dat"></param>
    public virtual void DoPTEffect7(short dat)
    {
        if ((dat & 0x0f) != 0)
            a.TrmDepth = (short)(dat & 0xf);
        if ((dat & 0xf0) != 0)
            a.TrmSpd = (short)((dat & 0xf0) >> 2);
        DoTremolo();
        a.OwnVol = 1;
    }

    /// <summary>
    /// Set Panning (MOD/XM: 8xx, S3M/IT: Xxy)
    /// This commands sets the pan position of the current channel. 
    /// In XM/IT, the value ranges from 00 (left) to FF (right). 
    /// In MOD/S3M, the value ranges from 00 (left) to 80 (right). 
    /// If the value is A4 (In MOD/S3M), the command sets the channel panning as Surround.
    /// </summary>
    /// <param name="dat"></param>
    public virtual void DoPTEffect8(short dat)
    {
        if (mp_panning)
        {
            a.Panning = dat;
            CurrentUniMod.Panning[mp_channel] = dat;
        }
    }

    /// <summary>
    /// Set Sample Offset (MOD/XM: 9xx, S3M/IT: Oxx)
    /// This command, when used together with a note, will start playing the sample at the position xx*256 
    /// (instead of position 0). If xx is 00 (900 or O00), the previous value will be used.
    /// </summary>
    /// <param name="dat"></param>
    public virtual void DoPTEffect9(short dat)
    {
        if (dat != 0)
            a.SampleOffset = (int)dat << 8; /* <- 0.43 fix.. */
        a.Start = a.SampleOffset;
        if (a.Start > a.Sample.Length)
            a.Start = a.Sample.Length;
    }

    /// <summary>
    /// Position Jump (MOD/XM/S3M/IT: Bxy)
    /// This command will cause the player to jump to the pattern position xy (hex). 
    /// Ie: B00 will restart the song from the start. 
    /// If used together with a pattern break, you can also specify the starting row (by default, it will play from the start of the pattern). 
    /// Note that most players disable backward jumps in the song if looping mode isn't enabled, so that it is not possible to loop a song forever (pretty annoying in a playlist).
    /// </summary>
    /// <param name="dat"></param>
    public virtual void DoPTEffectB(short dat)
    {
        /* if (SecondPatternDelayCounter != 0)
             return;
         if (dat < mp_sngpos)
             // avoid eternal looping
             return;

         PatternBreakPosition = 0;
         mp_sngpos = (short)(dat - 1);
         posjmp = 3;*/

        if (TickCounter != 0 || SecondPatternDelayCounter != 0)
            return;

        /* Vincent Voois uses a nasty trick in "Universal Bolero" */
        if (dat == mp_sngpos && PatternBreakPosition == _mp_patpos)
            return;

        if (mp_loop && PatternBreakPosition == 0 &&
            (dat < mp_sngpos ||
                 (mp_sngpos == (CurrentUniMod.Positions.Count - 1 - 1) && PatternBreakPosition == 0)
                ))
        {
            /* if we don't loop, better not to skip the end of the
               pattern, after all... so:
            mod.patbrk=0; */
            posjmp = 3;
        }
        else
        {
            /* if we were fading, adjust... */
            /*if (mp_sngpos == (CurrentUniMod.Positions.Count - 1 - 1))
                mp_volume = CurrentUniMod.vol initvolume > 128 ? 128 : mod.initvolume;*/
            mp_sngpos = dat;
            posjmp = 2;
            _mp_patpos = 0;
        }

    }

    /// <summary>
    /// Set Volume (MOD/XM: Cxx, S3M/IT: undefined)
    /// This command will set the current volume to xx (hex). 
    /// Note that the maximum value is 40 (hex). 
    /// It is better to use the volume column for volume effects, except in MOD songs, since the volume column isn't saved in the file.
    /// </summary>
    /// <param name="dat"></param>
    public virtual void DoPTEffectC(short dat)
    {
        if (TickCounter != 0)
            return;

        if (dat > 64)
            dat = 64;

        a.TmpVolume = (sbyte)dat;
    }

    /// <summary>
    /// Pattern Break (MOD/XM: Dxx, S3M/IT:Cxx)
    /// This command will stop playing the current pattern and will jump to the next one in the order list (pattern sequence). 
    /// You can also select the row where to start the next pattern. 
    /// Note that the specified row xx is in Hex (Ie D20 will jump to the 32nd row of the next pattern).
    /// </summary>
    /// <param name="dat"></param>
    public virtual void DoPTEffectD(short dat)
    {
        if (SecondPatternDelayCounter != 0)
            return;
        {
            int hi = (dat & 0xf0) >> 4;
            int lo = (dat & 0xf);
            PatternBreakPosition = (short)((hi * 10) + lo);
        }
        if (PatternBreakPosition > 64)
            PatternBreakPosition = 64; /* <- v0.42 fix */
        posjmp = 3;
    }

    /// <summary>
    /// Set Speed/Tempo (MOD/XM: Fxx, S3M/IT:undefined)
    ///  This command can either set the speed (xx smaller than 20) or the tempo (xx greater than 20) of the song. 
    ///  Avoid using 20 as a parameter, since it can cause problem in some players. 
    ///  In MOD, F20 will set the SPEED of the song, but in XM, F20 will set the TEMPO (bpm) of the song. 
    ///  This value is in Hex.
    /// </summary>
    /// <param name="dat"></param>
    public virtual void DoPTEffectF(short dat)
    {
        if ((TickCounter != 0) || (SecondPatternDelayCounter != 0))
            return;

        if (mp_extspd && dat >= 0x20)
        {
            old_bpm = dat;

            mp_bpm = (short)rint(old_bpm * SpeedConstant);
        }
        else
        {
            if (dat != 0)
            {
                // <- v0.44 bugfix
                mp_sngspd = dat;
                TickCounter = 0;
            }
        }
    }
}

