using System.Collections.Generic;

namespace SharpMod.Song
{
    /// <summary>
    /// A track is a list of cells for one channel in the pattern
    /// </summary>
    public class Track
    {
        public List<PatternCell> Cells { get; set; }

        private short[] _uniTrack;
        public short[] UniTrack
        {
            get { return _uniTrack; }
            set
            {
                _uniTrack = value;
                UniTrkHelper.Instance.FromUniTrk(this);
            }
        }

        public Track()
        {
            Cells = [.. new PatternCell[64]];
        }

        internal void RegisterPatternCellEvent(PatternCell pc)
        {
            pc.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Pc_PropertyChanged);
        }

        void Pc_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            /* --If (Cells[0] == null)
                return
           this._uniTrack = UniTrkHelper.Instance.ToUniTrk(this)*/
            /* --If(_trackTotal == Cells.Count)
                 UniTrkHelper.Instance.ToUniTrk(this)*/
        }

        public void ValidateChanges()
        {
            _uniTrack = UniTrkHelper.Instance.ToUniTrk(this);
        }
    }
}
