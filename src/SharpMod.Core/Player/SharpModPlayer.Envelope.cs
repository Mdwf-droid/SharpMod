using System;
using System.Collections.Generic;
using System.Text;

namespace SharpMod.Player;

public partial class SharpModPlayer
{
    public static void StartEnvelope(EnvPr t, short flg, short pts, short sus, short beg, short end, EnvPt[] p)
    {
        flg &= 0xFF;
        pts &= 0xFF;
        sus &= 0xFF;
        beg &= 0xFF;
        end &= 0xFF;

        t.Flg = flg;
        t.Pts = pts;
        t.Sus = sus;
        t.Beg = beg;
        t.End = end;
        t.EnvPoints = p;
        t.CurrentCounter = 0;
        t.EnvIdxA = 0;
        t.EnvIdxB = 1;
    }

    public static short ProcessEnvelope(EnvPr t, short v, bool keyon)
    {
        /* panning active? . copy variables */
        if ((t.Flg & (short)EnvelopeFlags.EF_ON) != 0)
        {
            short a, b;
            int p;

            a = t.EnvIdxA;
            b = t.EnvIdxB;
            p = t.CurrentCounter;

            /* compute the envelope value between points a and b */
            v = InterpolateEnv((short)p, t.EnvPoints[a], t.EnvPoints[b]);

            /* Should we sustain? (sustain flag on, key-on, point a is the sustain
            point, and the pointer is exactly on point a) */
            if (((t.Flg & (short)EnvelopeFlags.EF_SUSTAIN) != 0) && keyon && a == t.Sus && p == t.EnvPoints[a].Pos)
            {
                /* do nothing */
            }
            else
            {
                /* don't sustain, so increase pointer. */
                p++;

                /* pointer reached point b? */
                if (p >= t.EnvPoints[b].Pos)
                {

                    /* shift points a and b */
                    a = b; b++;

                    if ((t.Flg & (short)EnvelopeFlags.EF_LOOP) != 0)
                    {
                        if (b > t.End)
                        {
                            a = t.Beg;
                            b = (short)(a + 1);
                            p = t.EnvPoints[a].Pos;
                        }
                    }
                    else
                    {
                        if (b >= t.Pts)
                        {
                            b--;
                            p--;
                        }
                    }
                }
            }
            t.EnvIdxA = a;
            t.EnvIdxB = b;
            t.CurrentCounter = (short)p;
        }
        return v;
    }

    public static short InterpolateEnv(short p, EnvPt a, EnvPt b)
    {
        return (Interpolate(p, a.Pos, b.Pos, a.Val, b.Val));
    }
}
