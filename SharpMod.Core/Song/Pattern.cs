using System;
using System.Collections.Generic;
using System.Text;
using SharpMod.UniTracker;

namespace SharpMod.Song
{    
	public class Pattern
	{
        /// <summary>
        /// List of tracks of the pattern (One track per channel)
        /// </summary>
        public List<Track> Tracks { get; set; }

        /// <summary>
        /// Rows count of the pattern
        /// </summary>
        public int RowsCount { get; set; }


        public Pattern(int rowsCount)
        {
            this.Tracks = new List<Track>();
            this.RowsCount = rowsCount;
        }
	}
}
