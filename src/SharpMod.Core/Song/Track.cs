using System.Collections.Generic;

namespace SharpMod.Song;

public class Track
{
    public List<PatternCell> Cells { get; set; }

    private short[] _uniTrack;

    public short[] UniTrack
    {
        get => _uniTrack;
        set
        {
            _uniTrack = value;
            UniTrkHelper.Instance.fromUniTrk(this);
        }
    }

    public Track() : this(64) { }

    public Track(int rowCount)
    {
        Cells = new List<PatternCell>(rowCount);
        for (int i = 0; i < rowCount; i++)
            Cells.Add(null); // sera rempli par fromUniTrk
    }

    public void ValidateChanges()
    {
        _uniTrack = UniTrkHelper.Instance.ToUniTrk(this);
    }
}
