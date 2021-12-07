using System;
using System.Collections.Generic;
using SharpMod.Player;

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

        private Dictionary<int, ChannelMemory> _audioValues;

        ///<summary>
        ///</summary>
        public Dictionary<int, ChannelMemory> AudioValues
        {
            get
            {
                if (_audioValues == null)
                    _audioValues = new Dictionary<int, ChannelMemory>();
                return
                _audioValues;
            }
            set { _audioValues = value; }
        }
    }
}
