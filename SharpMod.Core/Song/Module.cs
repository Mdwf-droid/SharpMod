using SharpMod.UniTracker;
using System;
using System.Collections.Generic;

namespace SharpMod.Song
{

    ///<summary>
    /// Container for all elements of a given song module
    ///</summary>
    public class SongModule
    {
        /// <summary>
        /// All positions, that define the order of patterns while playing
        /// </summary>
        public List<int> Positions { get; set; }

        /// <summary>
        /// initial Speed
        /// </summary>
        public short InitialSpeed { get; set; }

        /// <summary>
        /// Initial Tempo
        /// </summary>
        public short InitialTempo { get; set; }

        /// <summary>
        /// List of patterns
        /// </summary>
        public List<Pattern> Patterns { get; set; }

        /// <summary>
        /// List of instruments
        /// </summary>
        public List<Instrument> Instruments { get; set; }

        /// <summary>
        /// Total channels of the song
        /// </summary>
        public int ChannelsCount { get; set; }

        /// <summary>
        /// 32 panning positions
        /// </summary>
        public short[] Panning { get; set; }

        /// <summary>
        /// Flags that define period type
        /// </summary>
        public UniModPeriods Flags { get; set; }

        /// <summary>
        /// name of the song
        /// </summary>
        public String SongName { get; set; }

        /// <summary>
        /// string type of module 
        /// </summary>
        public String ModType { get; set; }

        /// <summary>
        /// module comments
        /// </summary>
        public String Comment { get; set; }

        /// <summary>
        /// restart position 
        /// </summary>
        public short RepPos { get; set; }


        ///<summary>
        /// Default Constructor
        ///</summary>
        public SongModule()
        {
            Panning = new short[32];
            Patterns = [];
            Instruments = [];
            Positions = [];
        }


    }
}
