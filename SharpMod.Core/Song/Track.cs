using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpMod.Song;
using SharpMod.UniTracker;

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
            get { return this._uniTrack; }
            set
            {
                this._uniTrack = value;
                UniTrkHelper.Instance.fromUniTrk(this);
            }
        }

        public Track()
        {
            this.Cells = new List<PatternCell>();
            this.Cells.AddRange(new PatternCell[64]);
        }

        internal void RegisterPatternCellEvent(PatternCell pc)
        {
            pc.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(pc_PropertyChanged);
        }

        void pc_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {            
            /*if (Cells[0] == null)
                return;
           this._uniTrack = UniTrkHelper.Instance.ToUniTrk(this);*/
            /* if(_trackTotal == Cells.Count)
                 UniTrkHelper.Instance.ToUniTrk(this);*/
        }

        public void ValidateChanges()
        {
            this._uniTrack = UniTrkHelper.Instance.ToUniTrk(this);
        }
    }
}
