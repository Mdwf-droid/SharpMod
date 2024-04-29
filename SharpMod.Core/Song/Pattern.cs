using System.Collections.Generic;


namespace SharpMod.Song


{
    public class Pattern(int rowsCount)
    {
        /// <summary>
        /// List of tracks of the pattern (One track per channel)
        /// </summary>
        public List<Track> Tracks { get; set; } = [];

        /// <summary>
        /// Rows count of the pattern
        /// </summary>
        public int RowsCount { get; set; } = rowsCount;
    }
}
