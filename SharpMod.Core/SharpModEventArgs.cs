using SharpMod.Player;
using System;
using System.Collections.Generic;

namespace SharpMod
{
    ///<summary>
    ///</summary>
    public class SharpModEventArgs : EventArgs
    {
        ///<summary>
        ///</summary>
        public int SongPosition { get; set; }

        ///<summary>
        ///</summary>
        public int PatternPosition { get; set; }

        public int PatternNumber { get; set; }

        public int CurrentPatternPositionsCount { get; set; }

        private Dictionary<int, ChannelMemory> _audioValues;

        ///<summary>
        ///</summary>
        public Dictionary<int, ChannelMemory> AudioValues
        {
            get
            {
                _audioValues ??= [];
                return _audioValues;
            }
            set { _audioValues = value; }
        }
    }
}
