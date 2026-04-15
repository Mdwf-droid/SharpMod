using System;
using SharpMod.Song;
using System.Collections.Generic;

namespace SharpMod.UniTracker
{
    public class UniMod
    {
        /// <summary>
        /// number of channels
        /// </summary>
        public short NumChn  {get;set;}

        /// <summary>
        /// number of positions in this song
        /// </summary>
        public short NumPos { get; set; }

        /// <summary>
        /// restart position 
        /// </summary>
        public short RepPos { get; set; }

        /// <summary>
        /// number of patterns in this song
        /// </summary>
        public short NumPat { get; set; }

        /// <summary>
        /// number of tracks
        /// </summary>
        public short NumTrk { get; set; }

        /// <summary>
        /// Number of instruments
        /// </summary>
        public short NumIns { get; set; }

        /// <summary>
        /// initial Speed
        /// </summary>
        public short InitSpeed { get; set; }

        /// <summary>
        /// Initial Tempo
        /// </summary>
        public short InitTempo { get; set; }
                
        /// <summary>
        /// all positions
        /// </summary>
        public short[] Positions { get; set; }

        /// <summary>
        /// 32 panning positions
        /// </summary>
        public short[] Panning { get; set; }

        public short Flags { get; set; }

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
        /// List of instrument
        /// </summary>
        public List<Instrument> Instruments { get; set; }

        /// <summary>
        /// array of PATTERN
        /// </summary>
        public short[] Patterns { get; set; }

        /// <summary>
        /// array of number of rows for each pattern
        /// </summary>
        public int[] PattRows { get; set; }

        /// <summary>
        /// array of pointers to tracks
        /// </summary>
        public short[][] Tracks { get; set; }


        public List<Sample> Samples { get; set; }

        public UniMod()
        {
            Positions = new short[256];
            Panning = new short[32];
        }
    }
}