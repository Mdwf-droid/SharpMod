using System;
using System.Collections.Generic;
using System.Text;

namespace SharpMod.Player;

public partial class SharpModPlayer
{
    public virtual void DoXMVolSlide(short inf)
    {
        short lo, hi;

        inf &= 0xFF;

        if (inf != 0)
        {
            a.S3mVolSlide = inf;
        }
        inf = a.S3mVolSlide;

        if (TickCounter == 0)
            return;

        lo = (short)(inf & 0xf);
        hi = (short)(inf >> 4);

        if (hi == 0)
            a.TmpVolume = (sbyte)(a.TmpVolume - lo);
        else
            a.TmpVolume = (sbyte)(a.TmpVolume + hi);

        if (a.TmpVolume < 0)
            a.TmpVolume = 0;
        else if (a.TmpVolume > 64)
            a.TmpVolume = 64;
    }



    public virtual void DoXMGlobalSlide(short inf)
    {
        short lo, hi;

        inf &= 0xFF;

        if (inf != 0)
        {
            globalslide = inf;
        }
        inf = globalslide;

        if (TickCounter == 0)
            return;

        lo = (short)(inf & 0xf);
        hi = (short)(inf >> 4);

        if (hi == 0)
            globalvolume = (sbyte)(globalvolume - lo);
        else
            globalvolume = (sbyte)(globalvolume + hi);

        if (globalvolume < 0)
            globalvolume = 0;
        else if (globalvolume > 64)
            globalvolume = 64;
    }



    public virtual void DoXMPanSlide(short inf)
    {
        short lo, hi;
        short pan;

        inf &= 0xFF;

        if (inf != 0)
            a.PanSlideSpd = inf;
        else
            inf = a.PanSlideSpd;

        if (TickCounter == 0)
            return;

        lo = (short)(inf & 0xf);
        hi = (short)(inf >> 4);

        /* slide right has absolute priority: */
        if (hi != 0)
            lo = 0;

        pan = a.Panning;

        pan = (short)(pan - lo);
        pan = (short)(pan + hi);

        if (pan < 0)
            pan = 0;
        if (pan > 255)
            pan = 255;

        a.Panning = pan;
    }
}
