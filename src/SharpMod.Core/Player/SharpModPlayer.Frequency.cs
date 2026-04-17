using SharpMod.UniTracker;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpMod.Player;

public partial class SharpModPlayer
{
    public static int GetFreq2(int period)
    {
        int okt;
        int frequency;
        period = 7680 - period;
        okt = period / 768;
        frequency = LinearFreqTable[period % 768];
        frequency <<= 2;
        return (frequency >> (7 - okt));
    }

    public static int getlinearperiod(short note, int fine)
    {
        return ((10 * 12 * 16 * 4) - ((int)note * 16 * 4) - (fine / 2) + 64);
    }


    public static int getlogperiod(short note, int fine)
    {
        short n, o;
        int p1, p2, i;

        n = (short)(note % 12);
        o = (short)(note / 12);
        i = (n << 3) + (fine >> 4); /* n*8 + fine/16 */

        p1 = LogFreqTable[i];
        p2 = LogFreqTable[i + 1];

        return (Interpolate((short)(fine / 16), (short)0, (short)15, (short)p1, (short)p2) >> o);
    }


    public static int getoldperiod(short note, int c2spd)
    {
        short n, o;
        int period;

        if (c2spd == 0)
            return 4242;/* <- prevent divide overflow.. (42 eheh) */

        n = (short)(note % 12);
        o = (short)(note / 12);
        period = (short)(((8363L * mytab[n]) >> o) / c2spd);
        return period;
    }



    public virtual int GetPeriod(short note, int c2spd)
    {
        if ((CurrentUniMod.Flags & UniModFlags.UF_XMPERIODS) != 0)
        {
            return ((CurrentUniMod.Flags & UniModFlags.UF_LINEAR) != 0) ? getlinearperiod(note, c2spd) : getlogperiod(note, c2spd);
        }
        return (getoldperiod(note, c2spd));
    }

}
