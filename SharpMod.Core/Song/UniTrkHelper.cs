using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpMod.UniTracker;

namespace SharpMod.Song
{
    internal class UniTrkHelper
    {
        private object locker=new object();
        private static UniTrkHelper _instance;

        public static UniTrkHelper Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new UniTrkHelper();
                return _instance;
            }
        }

        /// <summary>
        /// Refresh the UniTrk Stream from the Track
        /// </summary>
        /// <param name="track"></param>
        internal short[] ToUniTrk(Track track)
        {
            lock (locker)
            {
                UniTrk trk = new UniTrk();

                //trk.UniReset();
                trk.UniInit();
                trk.UniReset();
                //int n_ptr = offset;
                foreach (PatternCell pc in track.Cells)
                {
                    if (pc == null)
                        continue;
                    if (pc.Instrument != 0)
                        trk.UniInstrument((short)(pc.Instrument));

                    if (pc.Period != 0 && pc.Note != null)
                        trk.UniNote((short)(pc.Period));

                    if (pc.Effect > 0)
                        trk.UniPTEffect((short)(pc.Effect /*- 3*/), pc.EffectData);

                    trk.UniNewline();

                }

                return trk.UniDup();
            }
            //track.UniModSong.Tracks[track.TrackNumber + (track.PatternNumber * track.UniModSong.NumChn)] = trk.UniDup();
        }

        /// <summary>
        /// Decode the UniTrk Stream from the Track
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        internal void fromUniTrk(Track track)
        {
            lock (locker)
            {
                UniTrk trk = new UniTrk();

                List<PatternCell> lpc = new List<PatternCell>();
                for (int r = 0; r < track.Cells.Count; r++)
                {
                    // Avoid if unitrack is empty
                    if (track.UniTrack.Length > 0)
                    {
                        int f = trk.UniFindRow(track.UniTrack, r);
                        trk.UniSetRow(track.UniTrack, f);
                    }

                    Effects c;
                    short note = 0;
                    int? RealNote = null;
                    short inst = -1;
                    short effect = 3;
                    short effectData = 0;
                    int? octave = null;
                    while ((c = (Effects)trk.UniGetByte()) != 0)
                    {
                        switch (c)
                        {
                            case Effects.UNI_NOTE:
                                note = trk.UniGetByte();
                                if (note != 96)
                                {
                                    short n = (short)((note) % 12);
                                    short o = (short)(note / 12);
                                    RealNote = n;
                                    octave = o;
                                }

                                break;

                            case Effects.UNI_INSTRUMENT:
                                inst = trk.UniGetByte();
                                //if (inst >= uniMod.NumIns)
                                //break; /* <- safety valve */

                                break;
                            default:
                                effect = (short)c;
                                effectData = trk.UniGetByte();
                                break;

                        }
                    }

                    PatternCell pc = new PatternCell(track);
                    pc.Period = (short)note;
                    pc.Note = RealNote;
                    pc.Octave = octave;
                    pc.Instrument = (short)(inst + 1);
                    pc.Effect = (short)(effect - 3);
                    pc.EffectData = (short)effectData;
                    lpc.Add(pc);
                }

                track.Cells = lpc;
            }
        }
    }
}
