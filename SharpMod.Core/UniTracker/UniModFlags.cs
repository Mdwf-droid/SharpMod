using System;

namespace SharpMod.UniTracker
{
    /// <summary>
    /// UniMod flags
    /// </summary>
    [Flags]
    public enum UniModPeriods : short
    {
        /// <summary>
        /// if set use XM periods/finetuning
        /// </summary>
        UF_XMPERIODS = 1,
        /// <summary>
        /// if set use LINEAR periods
        /// </summary>
		UF_LINEAR = 2
    }
}
