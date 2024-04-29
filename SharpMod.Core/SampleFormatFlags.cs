using System;

namespace SharpMod
{
    /// <summary>
    /// Sample format flags
    /// </summary>
    [Flags]
    public enum SampleFormats 
    {
        ///<summary>
        ///</summary>
        SF_16BITS = 1,
        ///<summary>
        ///</summary>
        SF_SIGNED = 2,
        ///<summary>
        ///</summary>
        SF_DELTA = 4,
        ///<summary>
        ///</summary>
        SF_BIG_ENDIAN = 8,
        ///<summary>
        ///</summary>
        SF_LOOP = 16,
        ///<summary>
        ///</summary>
        SF_BIDI = 32,
        ///<summary>
        ///</summary>
        SF_OWNPAN = 64,
        ///<summary>
        ///</summary>
        SF_REVERSE = 128
    }
}
