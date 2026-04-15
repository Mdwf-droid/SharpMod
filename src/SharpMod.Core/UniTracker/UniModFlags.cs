using System;
using System.Collections.Generic;
using System.Text;

namespace SharpMod.UniTracker
{
    /// <summary>
    /// UniMod flags
    /// </summary>
	public enum UniModFlags : short
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
